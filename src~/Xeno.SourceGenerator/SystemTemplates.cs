using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Xeno.SourceGenerator
{
    public partial class SystemSourceGenerator
    {
        private enum SystemMethodType
        {
            Startup,
            PreUpdate,
            Update,
            PostUpdate,
            Shutdown
        }

        private SourceText SystemTemplate(in string ns, in string systemName, in string[] genericArgs, IReadOnlyDictionary<int, List<(IMethodSymbol method, int order, bool includeDisabled, bool changedOnly)>> systemMethods)
        {
            return CSharpSyntaxTree.ParseText($@"
using Xeno;

namespace {ns}
{{
    public partial class {systemName}{(genericArgs.Length > 0 ? $"<{string.Join(", ", genericArgs)}>" : "")} : global::Xeno.System
    {{
        {PlaceDelegates(systemMethods.Where(g => g.Key != (int)SystemMethodType.Startup && g.Key != (int)SystemMethodType.Shutdown).SelectMany(kv => kv.Value))}
        
        protected override bool IsWorldStartSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Startup) ? "true" : "false")};
        protected override bool IsPreUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.PreUpdate) ? "true" : "false")};
        protected override bool IsUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Update) ? "true" : "false")};
        protected override bool IsPostUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.PostUpdate) ? "true" : "false")};
        protected override bool IsWordStopSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Shutdown) ? "true" : "false")};

        public {systemName}()
        {{
            {PlaceDelegateInitializers(systemMethods.Where(g => g.Key != (int)SystemMethodType.Startup && g.Key != (int)SystemMethodType.Shutdown).SelectMany(kv => kv.Value))}
        }}

        protected override void Start()
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Startup))}
        }}

        protected override void PreUpdate(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.PreUpdate))}
        }}

        protected override void Update(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Update))}
        }}

        protected override void PostUpdate(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.PostUpdate))}
        }}

        protected override void Stop()
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Shutdown))}
        }}
    }}
}}
", encoding: Encoding.UTF8)
                .GetRoot()
                .NormalizeWhitespace()
                .SyntaxTree
                .GetText();
        }

        private string PlaceOrderedCalls(List<(IMethodSymbol method, int order, bool includeDisabled, bool changedOnly)> systemMethods)
        {
            if (systemMethods == null) return string.Empty;
            
            var sb = new StringBuilder();
            systemMethods.Sort((a, b) => a.order - b.order);
            for (var i = 0; i < systemMethods.Count; i++)
            {
                var method = systemMethods[i].method;
                // simple call
                var parameters = method.Parameters;
                if (parameters.Length == 0)
                {
                    sb.AppendLine($"{method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}();");
                }
                else
                {
                    sb.AppendLine($"world.Entities({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()});");
                }
                    
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string PlaceDelegates(IEnumerable<(IMethodSymbol method, int order, bool includeDisabled, bool changedOnly)> systemMethods)
        {
            var sb = new StringBuilder();
            foreach (var (method, _, includeDisabled, changedOnly) in systemMethods)
            {
                var parameters = method.Parameters;

                // void (ref C1, ref C2, ref C3...)
                if (parameters.All(p => IsComponentType(p.Type) && p.RefKind == RefKind.Ref))
                {
                    var typeListFormatted = string.Join(", ", parameters.Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                    sb.AppendLine($"private readonly ComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                    continue;
                }
                
                // void (in Entity, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.Equals(entityType, SymbolEqualityComparer.Default) && method.Parameters[0].RefKind == RefKind.In
                         && method.Parameters.Skip(1).All(p => IsComponentType(p.Type) && p.RefKind == RefKind.Ref))
                {
                    var typeListFormatted = string.Join(", ", parameters.Skip(1).Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                    sb.AppendLine($"private readonly EntityComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                    continue;
                }
                
                // void (in Uniform, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.IsValueType && method.Parameters[0].RefKind == RefKind.In
                    && method.Parameters.Skip(1).All(p => IsComponentType(p.Type) && p.RefKind == RefKind.Ref))
                {
                    var typeListFormatted = string.Join(", ", parameters.Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                    sb.AppendLine($"private readonly UniformComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                    continue;
                }
                
                // void (in Entity, in Uniform, ref C1, ref C2, ref C3...)
                if (method.Parameters[0].Type.Equals(entityType, SymbolEqualityComparer.Default) && method.Parameters[0].RefKind == RefKind.In
                    && method.Parameters[1].Type.IsValueType && method.Parameters[1].RefKind == RefKind.In
                    && method.Parameters.Skip(2).All(p => IsComponentType(p.Type) && p.RefKind == RefKind.Ref))
                {
                    var typeListFormatted = string.Join(", ", parameters.Skip(1).Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                    sb.AppendLine($"private readonly EntityUniformComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                }
                
                // INVALID SIGNATURE!
            }
            return sb.ToString();
        }
        
        private string PlaceDelegateInitializers(IEnumerable<(IMethodSymbol method, int order, bool includeDisabled, bool changedOnly)> systemMethods)
        {
            var sb = new StringBuilder();
            foreach (var (method, _, includeDisabled, changedOnly) in systemMethods)
            {
                sb.AppendLine($"{method.Name}Delegate_{method.GetHashCode()} = {method.Name};");
            }
            return sb.ToString();
        }

        private bool IsComponentType(ITypeSymbol type)
        {
            if (type.Kind == SymbolKind.NamedType) return type.AllInterfaces.Contains(componentInterfaceType);
            if (type.Kind == SymbolKind.TypeParameter) return ((ITypeParameterSymbol)type).ConstraintTypes.Contains(componentInterfaceType);
            throw new InvalidCastException();
        }
    }
    
    internal static class RefKindExtensions
    {
        internal static string ToParameterPrefix(this RefKind kind)
        {
            switch (kind)
            {
                case RefKind.Out: return "out ";
                case RefKind.Ref: return "ref ";
                case RefKind.In: return "in ";
                case RefKind.None: return string.Empty;

                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}

