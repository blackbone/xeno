using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS8500
namespace {ns}
{{
    public partial class {systemName}{(genericArgs.Length > 0 ? $"<{string.Join(", ", genericArgs)}>" : "")} : global::Xeno.System
    {{
        {PlaceCommonVariables()}
        {PlaceUsedStoreVariables(systemMethods.SelectMany(kv => kv.Value))}
        {PlaceUniforms(systemMethods.SelectMany(kv => kv.Value))}
        
        protected override bool IsWorldStartSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Startup) ? "true" : "false")};
        protected override bool IsPreUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.PreUpdate) ? "true" : "false")};
        protected override bool IsUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Update) ? "true" : "false")};
        protected override bool IsPostUpdateSystem => {(systemMethods.ContainsKey((int)SystemMethodType.PostUpdate) ? "true" : "false")};
        protected override bool IsWordStopSystem => {(systemMethods.ContainsKey((int)SystemMethodType.Shutdown) ? "true" : "false")};

        [SkipLocalsInit]
        protected unsafe override void Start()
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Startup))}
        }}

        [SkipLocalsInit]
        protected unsafe override void PreUpdate(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.PreUpdate))}
        }}

        [SkipLocalsInit]
        protected unsafe override void Update(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Update))}
        }}

        [SkipLocalsInit]
        protected unsafe override void PostUpdate(in float delta)
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.PostUpdate))}
        }}

        [SkipLocalsInit]
        protected unsafe override void Stop()
        {{
            {PlaceOrderedCalls(systemMethods.GetValueOrDefault((int)SystemMethodType.Shutdown))}
        }}

        [SkipLocalsInit]
        protected override void OnAfterAttachToWorld()
        {{
            {PlaceStoresInitializers(systemMethods.SelectMany(kv => kv.Value))}
        }}

        [SkipLocalsInit]
        protected override void OnBeforeDetachFromWorld()
        {{
            {PlaceStoresDeinitializers(systemMethods.SelectMany(kv => kv.Value))}
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
                var componentParameters = parameters.Where(p => p.IsComponentParameter()).ToImmutableArray();
                var componentsListFormatted = string.Join(", ", componentParameters.Select(mp => mp.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

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
                    if (parameters.All(p => p.IsComponentParameter())) {
                        sb.AppendLine($"// {method.ToDisplayString()}");
                        foreach (var p in componentParameters) {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            sb.AppendLine($"_p_{name.GetHashCode():X8} = _s_{name.GetHashCode():X8}.pages;");
                        }
                        sb.AppendLine("buf = world.buffer;");
                        sb.AppendLine($"c = world.Match<{componentsListFormatted}>();");
                        sb.AppendLine("for (i = 0; i < c; i++)");
                        sb.AppendLine("{");
                        sb.AppendLine("eid = buf[i];");
                        sb.AppendLine("pid = eid >> Store3.Shift;");
                        sb.AppendLine("slot = eid & Store3.Mask;");
                        sb.AppendLine("Update(");
                        var args = componentParameters.Select(p => {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            return $"ref _p_{name.GetHashCode():X8}[pid][slot]";
                        });
                        sb.AppendLine(string.Join(", ", args));
                        sb.AppendLine(");");
                        sb.AppendLine("}");
                        continue;
                    }
                }

                { // void (in Entity, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"// {method.ToDisplayString()}");
                        foreach (var p in componentParameters) {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            sb.AppendLine($"_p_{name.GetHashCode():X8} = _s_{name.GetHashCode():X8}.pages;");
                        }
                        sb.AppendLine("buf = world.buffer;");
                        sb.AppendLine("ents = world.entities;");
                        sb.AppendLine($"c = world.Match<{componentsListFormatted}>();");
                        sb.AppendLine("for (i = 0; i < c; i++)");
                        sb.AppendLine("{");
                        sb.AppendLine("eid = buf[i];");
                        sb.AppendLine("pid = eid >> Store3.Shift;");
                        sb.AppendLine("slot = eid & Store3.Mask;");
                        sb.AppendLine("Update(");
                        var args = componentParameters.Select(p => {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            return $"ref _p_{name.GetHashCode():X8}[pid][slot]";
                        });
                        args = args.Prepend("ents[eid]");
                        sb.AppendLine(string.Join(", ", args));
                        sb.AppendLine(");");
                        sb.AppendLine("}");

                        continue;
                    }
                }

                { // void (in/ref Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsUniformParameter(out var kind, out var uniformName)
                        && parameters.Skip(1).All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"// {method.ToDisplayString()}");
                        foreach (var p in componentParameters) {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            sb.AppendLine($"_p_{name.GetHashCode():X8} = _s_{name.GetHashCode():X8}.pages;");
                        }
                        sb.AppendLine("buf = world.buffer;");
                        sb.AppendLine($"var c = world.Match<{componentsListFormatted}>();");
                        sb.AppendLine("for (i = 0; i < c; i++)");
                        sb.AppendLine("{");
                        sb.AppendLine("eid = buf[i];");
                        sb.AppendLine("pid = eid >> Store3.Shift;");
                        sb.AppendLine("slot = eid & Store3.Mask;");
                        sb.AppendLine("Update(");
                        var uniformPrefix = parameters[0].RefKind.ToParameterPrefix();
                        var uniformParameter = kind switch {
                            UniformKind.None => $"{uniformPrefix}{method.Name}Uniform_{method.GetHashCode()}",
                            UniformKind.Delta => "delta",
                            UniformKind.Named => $"{uniformPrefix}{uniformName}",
                        };
                        var args = componentParameters.Select(p => {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            return $"ref _p_{name.GetHashCode():X8}[pid][slot]";
                        });
                        args = args.Prepend(uniformParameter);
                        sb.AppendLine(string.Join(", ", args));
                        sb.AppendLine(");");
                        sb.AppendLine("}");
                        continue;
                    }
                }

                { // void (in Entity, in/ref Uniform, ref C1, ref C2, ref C3...)
                    if (parameters[0].IsEntityParameter()
                        && parameters[1].IsUniformParameter(out var kind, out var uniformName)
                        && parameters.Skip(2).All(p => p.IsComponentParameter()))
                    {
                        sb.AppendLine($"// {method.ToDisplayString()}");
                        foreach (var p in componentParameters) {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            sb.AppendLine($"_p_{name.GetHashCode():X8} = _s_{name.GetHashCode():X8}.pages;");
                        }
                        sb.AppendLine("buf = world.buffer;");
                        sb.AppendLine("ents = world.entities;");
                        sb.AppendLine($"c = world.Match<{componentsListFormatted}>();");
                        sb.AppendLine("for (i = 0; i < c; i++)");
                        sb.AppendLine("{");
                        sb.AppendLine("eid = buf[i];");
                        sb.AppendLine("pid = eid >> Store3.Shift;");
                        sb.AppendLine("slot = eid & Store3.Mask;");
                        sb.AppendLine("Update(");
                        var uniformPrefix = parameters[0].RefKind.ToParameterPrefix();
                        var uniformParameter = kind switch {
                            UniformKind.None => $"{uniformPrefix}{method.Name}Uniform_{method.GetHashCode()}",
                            UniformKind.Delta => "delta",
                            UniformKind.Named => $"{uniformPrefix}{uniformName}",
                        };
                        var args = componentParameters.Select(p => {
                            var name = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                            return $"ref _p_{name.GetHashCode():X8}[pid][slot]";
                        });
                        args = args.Prepend(uniformParameter);
                        args = args.Prepend("ents[eid]");
                        sb.AppendLine(string.Join(", ", args));
                        sb.AppendLine(");");
                        sb.AppendLine("}");
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

        private static string PlaceCommonVariables() {
            return @"
private int i;
private int c;
private uint eid;
private uint pid;
private uint slot;
private uint[] buf;
";
        }

        private static string PlaceUsedStoreVariables(IEnumerable<(IMethodSymbol method, int _)> systemMethods) {
            var types = systemMethods.SelectMany(m => m.method.Parameters.Where(p => p.IsComponentParameter()).Select(p => p.Type))
                .Distinct();

            var sb = new StringBuilder();

            int i = 0;
            foreach (var usedComponentType in types) {
                var name = usedComponentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"private Store3<{name}> _s_{name.GetHashCode():X8};");
                sb.AppendLine($"private {name}[][] _p_{name.GetHashCode():X8};");
            }

            return sb.ToString();
        }

        private static string PlaceStoresInitializers(IEnumerable<(IMethodSymbol method, int order)> systemMethods) {
            var types = systemMethods.SelectMany(m => m.method.Parameters.Where(p => p.IsComponentParameter()).Select(p => p.Type))
                .Distinct();

            var sb = new StringBuilder();

            int i = 0;
            foreach (var usedComponentType in types) {
                var name = usedComponentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"_s_{name.GetHashCode():X8} = world.GetStore<{name}>();");
            }

            return sb.ToString();
        }

        private static string PlaceStoresDeinitializers(IEnumerable<(IMethodSymbol method, int order)> systemMethods) {
            var types = systemMethods.SelectMany(m => m.method.Parameters.Where(p => p.IsComponentParameter()).Select(p => p.Type))
                            .Distinct();

            var sb = new StringBuilder();

            int i = 0;
            foreach (var usedComponentType in types) {
                var name = usedComponentType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                sb.AppendLine($"_s_{name.GetHashCode():X8} = null;");
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
