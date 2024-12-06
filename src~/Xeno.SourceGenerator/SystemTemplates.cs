using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

#pragma warning disable RS1024
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

        private SourceText SystemTemplate(in string ns, in string systemName, in string[] genericArgs, IReadOnlyDictionary<int, List<(IMethodSymbol method, int order)>> systemMethods)
        {
            return CSharpSyntaxTree.ParseText($@"
using Xeno;

namespace {ns}
{{
    public partial class {systemName}{(genericArgs.Length > 0 ? $"<{string.Join(", ", genericArgs)}>" : "")} : global::Xeno.System
    {{
        {PlaceUniforms(systemMethods.SelectMany(kv => kv.Value))}
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

        private string PlaceOrderedCalls(List<(IMethodSymbol method, int order)> systemMethods)
        {
            if (systemMethods == null) return string.Empty;

            var sb = new StringBuilder();
            systemMethods.Sort((a, b) => a.order - b.order);

            foreach (var (method, _) in systemMethods)
            {
                var parameters = method.Parameters;

                { // void ()
                    if (parameters.Length == 0)
                    {
                        sb.AppendLine($"{method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}();");
                        continue;
                    }
                }

                { // void (in/ref Uniform)
                    if (parameters.Length == 1 && parameters[0].IsUniformParameter(out var kind, out var name))
                    {
                        var uniformPrefix = parameters[0].RefKind.ToParameterPrefix();
                        switch (kind) {
                            case UniformKind.None:
                                sb.AppendLine($"{method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}({uniformPrefix}{method.Name}Uniform_{method.GetHashCode()});");
                                break;
                            case UniformKind.Named:
                                sb.AppendLine($"{method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}({uniformPrefix}{name});");
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        continue;
                    }
                }

                { // void (ref C1, ref C2, ref C3...)
                    if (parameters.All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()});");
                        continue;
                    }
                }


                { // void (in Entity, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()});");

                        continue;
                    }
                }

                { // void (in/ref Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsUniformParameter(out var kind, out var name)
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        var uniformPrefix = parameters[0].RefKind.ToParameterPrefix();
                        switch (kind) {
                            case UniformKind.None:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, {uniformPrefix}{method.Name}Uniform_{method.GetHashCode()});");
                                break;
                            case UniformKind.Delta:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, delta);");
                                break;
                            case UniformKind.Named:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, {uniformPrefix}{name});");
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                        continue;
                    }
                }

                { // void (in Entity, in/ref Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters[1].IsUniformParameter(out var kind, out var name)
                        && parameters.Skip(2).All(p => p.IsComponentParameter()))
                    {
                        var uniformPrefix = parameters[0].RefKind.ToParameterPrefix();
                        switch (kind) {
                            case UniformKind.None:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, {uniformPrefix}{method.Name}Uniform_{method.GetHashCode()});");
                                break;
                            case UniformKind.Delta:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, delta);");
                                break;
                            case UniformKind.Named:
                                sb.AppendLine($"world.Iterate({method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}Delegate_{method.GetHashCode()}, {uniformPrefix}{name});");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                // INVALID SIGNATURE!
            }
            return sb.ToString();
        }

        private static string PlaceUniforms(IEnumerable<(IMethodSymbol method, int order)> systemMethods)
        {
            var sb = new StringBuilder();
            foreach (var (method, _) in systemMethods)
            {
                var parameters = method.Parameters;
                if (parameters.Length == 0) continue;

                { // void (in/ref Uniform)
                    if (parameters.Length == 1 && parameters[0].IsUniformParameter(out var kind, out _) && kind == UniformKind.None)
                    {
                        sb.AppendLine($"private {parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {method.Name}Uniform_{method.GetHashCode()};");
                        continue;
                    }
                }

                { // void (in Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsUniformParameter(out var kind, out _) && kind == UniformKind.None
                        && parameters.Skip(1).All(p => p.IsComponentParameter())) {
                        sb.AppendLine($"private {parameters[0].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {method.Name}Uniform_{method.GetHashCode()};");
                        continue;
                    }
                }

                { // void (in Entity, in Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters[1].IsUniformParameter(out var kind, out _) && kind == UniformKind.None
                        && parameters.Skip(2).All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"private {parameters[1].Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {method.Name}Uniform_{method.GetHashCode()};");
                    }
                }

                // INVALID SIGNATURE!
            }
            return sb.ToString();
        }

        private string PlaceDelegates(IEnumerable<(IMethodSymbol method, int order)> systemMethods)
        {
            var sb = new StringBuilder();
            foreach (var (method, _) in systemMethods)
            {
                var parameters = method.Parameters;

                { // void (ref C1, ref C2, ref C3...)
                    if (parameters.All(p => p.IsComponentParameter()))
                    {
                        var typeListFormatted = string.Join(", ", parameters.Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                        sb.AppendLine($"private readonly ComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                        continue;
                    }
                }
                
                { // void (in Entity, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        var typeListFormatted = string.Join(", ", parameters.Skip(1).Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                        sb.AppendLine($"private readonly EntityComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                        continue;
                    }
                }
                
                { // void (in Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsUniformParameter(out _, out _)
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        var typeListFormatted = string.Join(", ", parameters.Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                        sb.AppendLine($"private readonly Uniform{parameters[0].RefKind}ComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                        continue;
                    }
                }

                { // void (in Entity, in Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters[1].IsUniformParameter(out _, out  _)
                        && parameters.Skip(2).All(p => p.IsComponentParameter()))
                    {
                        var typeListFormatted = string.Join(", ", parameters.Skip(1).Select(mp => $"{mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}"));
                        sb.AppendLine($"private readonly EntityUniform{parameters[0].RefKind}ComponentDelegate<{typeListFormatted}> {method.Name}Delegate_{method.GetHashCode()};");
                    }
                }
                
                // INVALID SIGNATURE!
            }
            return sb.ToString();
        }
        
        private string PlaceDelegateInitializers(IEnumerable<(IMethodSymbol method, int order)> systemMethods)
        {
            var sb = new StringBuilder();
            foreach (var (method, _) in systemMethods)
            {
                sb.AppendLine($"{method.Name}Delegate_{method.GetHashCode()} = {method.Name};");
            }
            return sb.ToString();
        }
    }

    internal static class RefKindExtensions
    {
        internal static bool IsComponentParameter(this IParameterSymbol parameter) {
            if (parameter.RefKind != RefKind.Ref) return false;
            if (parameter.Type.Kind == SymbolKind.NamedType) return parameter.Type.AllInterfaces.Contains(SystemSourceGenerator.componentInterfaceType);
            if (parameter.Type.Kind == SymbolKind.TypeParameter) return ((ITypeParameterSymbol)parameter.Type).ConstraintTypes.Contains(SystemSourceGenerator.componentInterfaceType);
            return false;
        }

        internal static bool IsEntityParameter(this IParameterSymbol parameter) {
            return parameter.RefKind is RefKind.In && parameter.Type.Equals(SystemSourceGenerator.entityType, SymbolEqualityComparer.Default);
        }

        internal static bool IsUniformParameter(this IParameterSymbol parameter, out UniformKind kind, out string name) {
            kind = default;
            name = null;

            if (parameter.RefKind != RefKind.In && parameter.RefKind != RefKind.Ref)
                return false;

            var attribute = parameter.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Equals(SystemSourceGenerator.uniformAttributeType, SymbolEqualityComparer.Default) ?? false);
            if (attribute == null)
                return false;

            switch (attribute.ConstructorArguments[0].Value) {
                case bool b:
                    kind = b ? UniformKind.Delta : UniformKind.None;
                    return true;
                case string s:
                    name = s;
                    kind = UniformKind.Named;
                    return true;
            }

            return false;
        }

        internal static bool HasUniformAttributes(this IParameterSymbol parameter) {
            var attributes = parameter.GetAttributes();
            return attributes.Any(a => a.AttributeClass.Name == "UniformAttribute");
        }

        internal static string ToParameterPrefix(this RefKind kind)
        {
            switch (kind)
            {
                case RefKind.Out: return "out ";
                case RefKind.Ref: return "ref ";
                case RefKind.In: return "";
                case RefKind.None: return string.Empty;

                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }
    }
}

#pragma warning restore RS1024
