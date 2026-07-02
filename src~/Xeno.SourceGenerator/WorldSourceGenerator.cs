using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator;

[Generator]
public sealed class WorldSourceGenerator : ISourceGenerator
{
    private static readonly DiagnosticDescriptor ConstructorNotSupported = new(
        "XENO001",
        "Generated worlds must not declare constructors",
        "Generated world '{0}' declares a constructor, but Xeno generates its own constructor for component storage and systems",
        "Xeno.SourceGenerator",
        DiagnosticSeverity.Error,
        true);

    private enum SystemMethodType
    {
        Startup,
        PreUpdate,
        Update,
        PostUpdate,
        Shutdown
    }

    private enum RequestedApiMethodKind
    {
        CreateEntity,
        Add,
        Count,
        HasAll,
        HasAny,
        Remove,
    }

    private sealed class SyntaxReceiver : ISyntaxReceiver
    {
        public readonly List<ClassDeclarationSyntax> Candidates = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclaration)
                Candidates.Add(classDeclaration);
        }
    }

    private sealed class RegisteredSystem
    {
        public INamedTypeSymbol Type;
        public int Order;
        public int Index;
        public bool RequiresInstance;
        public string FieldName;
        public IMethodSymbol Constructor;
        public readonly List<SystemCall> Calls = new();
        public bool BakeQuery;
    }

    private sealed class SystemCall
    {
        public RegisteredSystem System;
        public IMethodSymbol Method;
        public SystemMethodType Type;
        public int Order;
        public int Index;
        public bool Pure;
        public bool BakeQuery;
        public ImmutableArray<IParameterSymbol> ComponentParameters;
    }

    private sealed class ComponentInfo
    {
        public INamedTypeSymbol Type;
        public int Index;
        public string ApiName;
        public string HelperName;
        public string PagesFieldName;
        public string PoolFieldName;
        public string PoolCountFieldName;
        public string InlinePageName;
        public bool Inline;
    }

    private sealed class ComponentSetInfo
    {
        public ComponentInfo[] Components;
        public string MaskFieldName;
        public string ArchetypeCacheFieldName;
        public string AddSourceCacheFieldName;
        public string AddTargetCacheFieldName;
        public string RemoveSourceCacheFieldName;
        public string RemoveTargetCacheFieldName;
        public string QuerySlotsFieldName;
        public string QueryPageCountsFieldName;
        public string QueryPageStatesFieldName;
        public string QueryCountFieldName;
        public bool MaterializedQuery;
        public int TransitionKey;
    }

    private sealed class RequestedApiMethod
    {
        public RequestedApiMethodKind Kind;
        public string MethodName;
        public INamedTypeSymbol[] ComponentTypes;
        public string[] ParameterNames;
        public bool DeclaredAsPartial;
    }

    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
            return;

        var compilation = context.Compilation;
        if (!Ensure.Type(compilation, "Xeno.World", out var worldType)) return;
        if (!Ensure.Type(compilation, "Xeno.RegisterComponentAttribute", out var registerComponentAttributeType)) return;
        if (!Ensure.Type(compilation, "Xeno.RegisterSystemAttribute", out var registerSystemAttributeType)) return;
        if (!Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out var systemMethodAttributeType)) return;
        if (!Ensure.Type(compilation, "Xeno.Entity", out var entityType)) return;
        if (!Ensure.Type(compilation, "Xeno.UniformAttribute", out var uniformAttributeType)) return;

        foreach (var candidate in receiver.Candidates)
        {
            var semanticModel = compilation.GetSemanticModel(candidate.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(candidate) is not INamedTypeSymbol worldSymbol)
                continue;

            var worldAttributes = worldSymbol.GetAttributes();
            if (!worldAttributes.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, registerComponentAttributeType)
                                          || SymbolEqualityComparer.Default.Equals(a.AttributeClass, registerSystemAttributeType)))
                continue;

            if (!IsDerivedFrom(worldSymbol, worldType))
                continue;

            if (worldSymbol.InstanceConstructors.Any(c => !c.IsImplicitlyDeclared))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ConstructorNotSupported,
                    candidate.Identifier.GetLocation(),
                    worldSymbol.Name));
                continue;
            }

            var registeredSystems = CollectSystems(
                worldAttributes,
                registerSystemAttributeType,
                systemMethodAttributeType,
                entityType,
                uniformAttributeType);

            var requestedApiMethods = CollectRequestedApiMethods(
                compilation,
                worldSymbol,
                entityType,
                worldAttributes,
                registerComponentAttributeType,
                registeredSystems);

            var components = CollectComponents(
                worldAttributes,
                registerComponentAttributeType,
                registeredSystems,
                requestedApiMethods);
            var componentSets = CollectComponentSets(registeredSystems, requestedApiMethods, components);

            var source = GenerateWorld(worldSymbol, registeredSystems, components, componentSets, requestedApiMethods, entityType, uniformAttributeType);
            context.AddSource($"{worldSymbol.Name}.World.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static List<RequestedApiMethod> CollectRequestedApiMethods(
        Compilation compilation,
        INamedTypeSymbol worldSymbol,
        INamedTypeSymbol entityType,
        ImmutableArray<AttributeData> worldAttributes,
        INamedTypeSymbol registerComponentAttributeType,
        List<RegisteredSystem> registeredSystems)
    {
        var methods = new List<RequestedApiMethod>();
        var indexByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        var knownComponents = CollectKnownComponentTypes(worldAttributes, registerComponentAttributeType, registeredSystems);
        var knownComponentsByApiName = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);
        foreach (var componentType in knownComponents)
        {
            var apiName = BuildApiName(componentType);
            if (!knownComponentsByApiName.ContainsKey(apiName))
                knownComponentsByApiName.Add(apiName, componentType);
        }

        void AddRequested(
            RequestedApiMethodKind kind,
            string methodName,
            INamedTypeSymbol[] componentTypes,
            string[] parameterNames,
            bool declaredAsPartial)
        {
            if (componentTypes.Length == 0 || componentTypes.Any(type => type == null))
                return;

            var key = $"{kind}:{methodName}:{string.Join("|", componentTypes.Select(TypeName))}";
            if (indexByKey.TryGetValue(key, out var index))
            {
                if (declaredAsPartial && parameterNames != null)
                    methods[index].ParameterNames = parameterNames;
                methods[index].DeclaredAsPartial |= declaredAsPartial;
                return;
            }

            indexByKey[key] = methods.Count;
            methods.Add(new RequestedApiMethod {
                Kind = kind,
                MethodName = methodName,
                ComponentTypes = componentTypes,
                ParameterNames = parameterNames,
                DeclaredAsPartial = declaredAsPartial,
            });
        }

        foreach (var syntaxReference in worldSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not ClassDeclarationSyntax classDeclaration)
                continue;

            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            foreach (var methodDeclaration in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
            {
                if (!methodDeclaration.Modifiers.Any(m => m.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PartialKeyword)))
                    continue;

                if (semanticModel.GetDeclaredSymbol(methodDeclaration) is not IMethodSymbol methodSymbol)
                    continue;

                var methodName = methodSymbol.Name;
                if (methodName is "CreateEntity" or "Add")
                {
                    if (methodName == "CreateEntity")
                    {
                        if (methodSymbol.Parameters.Length is < 2 or > 4)
                            continue;

                        AddRequested(
                            RequestedApiMethodKind.CreateEntity,
                            methodName,
                            methodSymbol.Parameters.Select(p => p.Type as INamedTypeSymbol).Where(t => t != null).ToArray(),
                            methodSymbol.Parameters.Select(p => p.Name).ToArray(),
                            declaredAsPartial: true);
                        continue;
                    }

                    if (methodSymbol.Parameters.Length is < 3 or > 5)
                        continue;
                    if (!SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, entityType))
                        continue;

                    AddRequested(
                        RequestedApiMethodKind.Add,
                        methodName,
                        methodSymbol.Parameters.Skip(1).Select(p => p.Type as INamedTypeSymbol).Where(t => t != null).ToArray(),
                        methodSymbol.Parameters.Skip(1).Select(p => p.Name).ToArray(),
                        declaredAsPartial: true);
                    continue;
                }

                if (!TryParseNamedRequestedApiMethod(methodName, out var namedKind, out var apiNames))
                    continue;

                if (!TryResolveComponentApiNames(knownComponentsByApiName, apiNames, out var componentTypes))
                    continue;

                if (namedKind == RequestedApiMethodKind.Count)
                {
                    if (methodSymbol.Parameters.Length != 0)
                        continue;
                }
                else
                {
                    if (methodSymbol.Parameters.Length != 1)
                        continue;
                    if (!SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, entityType))
                        continue;
                }

                AddRequested(namedKind, methodName, componentTypes, Array.Empty<string>(), declaredAsPartial: true);
            }
        }

        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();
            foreach (var invocation in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var methodName = memberAccess.Name.Identifier.ValueText;
                var receiverType = semanticModel.GetTypeInfo(memberAccess.Expression).Type as INamedTypeSymbol;
                if (receiverType == null || !IsSameOrDerived(receiverType, worldSymbol))
                    continue;

                var args = invocation.ArgumentList.Arguments;
                if (methodName is "CreateEntity" or "Add")
                {
                    var kind = methodName == "CreateEntity"
                        ? RequestedApiMethodKind.CreateEntity
                        : RequestedApiMethodKind.Add;

                    if (kind == RequestedApiMethodKind.CreateEntity)
                    {
                        if (args.Count is < 2 or > 4)
                            continue;
                    }
                    else
                    {
                        if (args.Count is < 3 or > 5)
                            continue;

                        var entityArgType = semanticModel.GetTypeInfo(args[0].Expression).Type;
                        if (!SymbolEqualityComparer.Default.Equals(entityArgType, entityType))
                            continue;
                    }

                    var offset = kind == RequestedApiMethodKind.Add ? 1 : 0;
                    var invokedComponentTypes = new INamedTypeSymbol[args.Count - offset];
                    var valid = true;
                    for (var i = offset; i < args.Count; i++)
                    {
                        if (semanticModel.GetTypeInfo(args[i].Expression).Type is not INamedTypeSymbol componentType)
                        {
                            valid = false;
                            break;
                        }

                        invokedComponentTypes[i - offset] = componentType;
                    }

                    if (!valid)
                        continue;

                    AddRequested(kind, methodName, invokedComponentTypes, null, declaredAsPartial: false);
                    continue;
                }

                if (!TryParseNamedRequestedApiMethod(methodName, out var namedKind, out var apiNames))
                    continue;

                if (namedKind == RequestedApiMethodKind.Count)
                {
                    if (args.Count != 0)
                        continue;
                }
                else
                {
                    if (args.Count != 1)
                        continue;

                    var entityArgType = semanticModel.GetTypeInfo(args[0].Expression).Type;
                    if (!SymbolEqualityComparer.Default.Equals(entityArgType, entityType))
                        continue;
                }

                if (!TryResolveComponentApiNames(knownComponentsByApiName, apiNames, out var componentTypes))
                    continue;

                AddRequested(namedKind, methodName, componentTypes, Array.Empty<string>(), declaredAsPartial: false);
            }
        }

        return methods;
    }

    private static List<INamedTypeSymbol> CollectKnownComponentTypes(
        ImmutableArray<AttributeData> worldAttributes,
        INamedTypeSymbol registerComponentAttributeType,
        IEnumerable<RegisteredSystem> systems)
    {
        var components = new List<INamedTypeSymbol>();
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        void Add(INamedTypeSymbol type)
        {
            if (type != null && seen.Add(type))
                components.Add(type);
        }

        foreach (var attribute in worldAttributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, registerComponentAttributeType))
                continue;
            if (attribute.ConstructorArguments.Length > 0)
                Add(attribute.ConstructorArguments[0].Value as INamedTypeSymbol);
        }

        foreach (var parameter in systems.SelectMany(s => s.Calls).SelectMany(c => c.ComponentParameters))
            Add(parameter.Type as INamedTypeSymbol);

        return components;
    }

    private static List<RegisteredSystem> CollectSystems(
        ImmutableArray<AttributeData> worldAttributes,
        INamedTypeSymbol registerSystemAttributeType,
        INamedTypeSymbol systemMethodAttributeType,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var systems = new List<RegisteredSystem>();
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var attribute in worldAttributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, registerSystemAttributeType))
                continue;

            if (attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Value is not INamedTypeSymbol systemType)
                continue;

            if (!seen.Add(systemType))
                continue;

            var system = new RegisteredSystem {
                Type = systemType,
                Order = attribute.ConstructorArguments.Length > 1 && attribute.ConstructorArguments[1].Value is int order ? order : 0,
                Index = systems.Count,
                FieldName = $"__xeno_system_{systems.Count:X2}",
                BakeQuery = TryGetNamedBool(attribute, "BakeQuery")
                            || (attribute.ConstructorArguments.Length > 2
                                && attribute.ConstructorArguments[2].Value is bool positionalBakeQuery
                                && positionalBakeQuery),
            };

            system.Constructor = SelectConstructor(systemType);
            system.RequiresInstance = systemType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => TryGetSystemMethod(m, systemMethodAttributeType, out _, out _, out _))
                .Any(m => !m.IsStatic);

            foreach (var method in systemType.GetMembers().OfType<IMethodSymbol>())
            {
                if (!TryGetSystemMethod(method, systemMethodAttributeType, out var type, out var methodOrder, out var pure))
                    continue;

                var call = new SystemCall {
                    System = system,
                    Method = method,
                    Type = type,
                    Order = methodOrder,
                    Index = system.Calls.Count,
                    Pure = pure,
                    BakeQuery = system.BakeQuery,
                    ComponentParameters = method.Parameters.Where(p => IsComponentParameter(p, entityType, uniformAttributeType)).ToImmutableArray(),
                };

                if (!IsSupportedSystemMethod(method, entityType, uniformAttributeType))
                    continue;

                system.Calls.Add(call);
            }

            systems.Add(system);
        }

        return systems;
    }

    private static IMethodSymbol SelectConstructor(INamedTypeSymbol systemType)
    {
        var constructors = systemType.InstanceConstructors
            .Where(c => !c.IsStatic && !c.IsImplicitlyDeclared && c.DeclaredAccessibility != Accessibility.Private)
            .OrderBy(c => c.Parameters.Length)
            .ToArray();

        return constructors.FirstOrDefault(c => c.Parameters.Length == 0) ?? constructors.FirstOrDefault();
    }

    private static List<ComponentInfo> CollectComponents(
        ImmutableArray<AttributeData> worldAttributes,
        INamedTypeSymbol registerComponentAttributeType,
        IEnumerable<RegisteredSystem> systems,
        IEnumerable<RequestedApiMethod> requestedApiMethods)
    {
        var components = new List<ComponentInfo>();
        var seen = new Dictionary<INamedTypeSymbol, ComponentInfo>(SymbolEqualityComparer.Default);

        void Add(INamedTypeSymbol type, bool inline = false)
        {
            if (type == null)
                return;

            if (seen.TryGetValue(type, out var existing))
            {
                existing.Inline |= inline;
                return;
            }

            var index = components.Count;
            var apiName = BuildApiName(type);
            var component = new ComponentInfo {
                Type = type,
                Index = index,
                ApiName = apiName,
                HelperName = $"__xeno_{apiName}",
                PagesFieldName = $"__xeno_pages_{index:X2}",
                PoolFieldName = $"__xeno_pool_{index:X2}",
                PoolCountFieldName = $"__xeno_pool_{index:X2}_count",
                InlinePageName = $"__xeno_page_{index:X2}",
                Inline = inline,
            };
            components.Add(component);
            seen.Add(type, component);
        }

        foreach (var attribute in worldAttributes)
        {
            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, registerComponentAttributeType))
                continue;
            if (attribute.ConstructorArguments.Length > 0)
                Add(attribute.ConstructorArguments[0].Value as INamedTypeSymbol, TryGetNamedBool(attribute, "Inline"));
        }

        foreach (var parameter in systems.SelectMany(s => s.Calls).SelectMany(c => c.ComponentParameters))
            Add(parameter.Type as INamedTypeSymbol);

        foreach (var componentType in requestedApiMethods.SelectMany(m => m.ComponentTypes))
            Add(componentType);

        return components;
    }

    private static List<ComponentSetInfo> CollectComponentSets(List<RegisteredSystem> systems, List<RequestedApiMethod> requestedApiMethods, List<ComponentInfo> components)
    {
        var sets = new List<ComponentSetInfo>();
        var seen = new HashSet<string>();
        var materialized = new HashSet<string>();

        foreach (var call in systems.SelectMany(s => s.Calls).Where(c => c.BakeQuery && c.ComponentParameters.Length > 0))
        {
            var set = call.ComponentParameters
                .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
                .OrderBy(c => c.Index)
                .ToArray();
            materialized.Add(string.Join(",", set.Select(c => c.Index)));
        }

        void Add(IEnumerable<ComponentInfo> source)
        {
            var set = source.OrderBy(c => c.Index).ToArray();
            if (set.Length == 0)
                return;

            var key = string.Join(",", set.Select(c => c.Index));
            if (!seen.Add(key))
                return;

            var name = string.Join("_", set.Select(c => c.ApiName));
            sets.Add(new ComponentSetInfo {
                Components = set,
                MaskFieldName = $"__xeno_mask_{name}",
                ArchetypeCacheFieldName = $"__xeno_archetype_{name}",
                AddSourceCacheFieldName = $"__xeno_add_source_{name}",
                AddTargetCacheFieldName = $"__xeno_add_target_{name}",
                RemoveSourceCacheFieldName = $"__xeno_remove_source_{name}",
                RemoveTargetCacheFieldName = $"__xeno_remove_target_{name}",
                QuerySlotsFieldName = $"__xeno_query_{name}_slots",
                QueryPageCountsFieldName = $"__xeno_query_{name}_pageCounts",
                QueryPageStatesFieldName = $"__xeno_query_{name}_pageStates",
                QueryCountFieldName = $"__xeno_query_{name}_count",
                MaterializedQuery = materialized.Contains(key),
                TransitionKey = sets.Count + 1,
            });
        }

        foreach (var component in components)
            Add(new[] { component });

        foreach (var call in systems.SelectMany(s => s.Calls).Where(c => c.ComponentParameters.Length > 0))
            Add(call.ComponentParameters.Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type))));

        foreach (var method in requestedApiMethods.Where(m => m.ComponentTypes.Length > 1))
            Add(method.ComponentTypes.Select(type => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, type))));

        return sets;
    }

    private static string GenerateWorld(
        INamedTypeSymbol worldSymbol,
        List<RegisteredSystem> systems,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        List<RequestedApiMethod> requestedApiMethods,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#pragma warning disable CS0169");
        sb.AppendLine("#pragma warning disable CS0649");
        sb.AppendLine("using System.Runtime.CompilerServices;");
        sb.AppendLine();

        var ns = worldSymbol.ContainingNamespace;
        if (!ns.IsGlobalNamespace)
        {
            sb.Append("namespace ").Append(ns.ToDisplayString()).AppendLine(" {");
        }

        sb.Append("public partial class ").Append(worldSymbol.Name).AppendLine(" {");
        AppendCommonFields(sb, components, componentSets);
        AppendSystemFields(sb, systems);
        AppendConstructor(sb, worldSymbol, systems, components, componentSets);
        AppendGeneratedComponentApi(sb, worldSymbol, components, componentSets, requestedApiMethods);
        AppendStart(sb, systems);
        AppendTick(sb, systems, components, componentSets, entityType, uniformAttributeType);
        AppendStop(sb, systems);
        sb.AppendLine("}");

        if (!ns.IsGlobalNamespace)
            sb.AppendLine("}");

        return sb.ToString();
    }

    private static void AppendCommonFields(
        StringBuilder sb,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets)
    {
        sb.AppendLine("    private const int __xeno_pageShift = 6;");
        sb.AppendLine("    private const int __xeno_pageMask = (1 << __xeno_pageShift) - 1;");
        sb.AppendLine("    private const int __xeno_pageCap = __xeno_pageMask + 1;");
        sb.AppendLine("    private int[][] __xeno_chunks;");
        sb.AppendLine("    private int[] __xeno_counts;");
        sb.AppendLine("    private int[] __xeno_buf;");
        sb.AppendLine("    private global::Xeno.Entity[] __xeno_entities;");
        sb.AppendLine("    private __XenoPage[] __xeno_pages;");
        sb.AppendLine("    private int __xeno_chunkCount;");
        sb.AppendLine("    private int __xeno_chunk;");
        sb.AppendLine("    private int __xeno_i;");
        sb.AppendLine("    private int __xeno_count;");
        sb.AppendLine("    private float __xeno_delta;");
        sb.AppendLine();

        sb.AppendLine("    private struct __XenoPage {");
        foreach (var component in components)
            sb.Append("        public ").Append(PageStorageTypeName(component)).Append(' ').Append(component.PagesFieldName).AppendLine(";");
        sb.AppendLine("    }");
        sb.AppendLine();

        foreach (var component in components.Where(c => !c.Inline))
        {
            var typeName = TypeName(component.Type);
            sb.Append("    private static ").Append(typeName).Append("[][] ").Append(component.PoolFieldName).AppendLine(";");
            sb.Append("    private static int ").Append(component.PoolCountFieldName).AppendLine(";");
        }

        if (components.Count > 0)
            sb.AppendLine();

        foreach (var component in components.Where(c => !c.Inline))
            sb.Append("    private ").Append(TypeName(component.Type)).Append("[] ").Append(ComponentPageFieldName(component)).AppendLine(";");

        foreach (var set in componentSets)
        {
            sb.Append("    private static readonly global::Xeno.BitSetReadOnly ").Append(set.MaskFieldName).Append(" = CreateGeneratedMask(")
                .Append(string.Join(", ", set.Components.Select(c => $"{c.HelperName}.Index")))
                .AppendLine(");");
            sb.Append("    private object ").Append(set.ArchetypeCacheFieldName).AppendLine(";");
            sb.Append("    private object ").Append(set.AddSourceCacheFieldName).AppendLine(";");
            sb.Append("    private object ").Append(set.AddTargetCacheFieldName).AppendLine(";");
            sb.Append("    private object ").Append(set.RemoveSourceCacheFieldName).AppendLine(";");
            sb.Append("    private object ").Append(set.RemoveTargetCacheFieldName).AppendLine(";");
            if (set.MaterializedQuery)
            {
                sb.Append("    private ulong[] ").Append(set.QuerySlotsFieldName).AppendLine(";");
                sb.Append("    private int[] ").Append(set.QueryPageCountsFieldName).AppendLine(";");
                sb.Append("    private ulong[] ").Append(set.QueryPageStatesFieldName).AppendLine(";");
                sb.Append("    private int ").Append(set.QueryCountFieldName).AppendLine(";");
            }
        }

        if (components.Count > 0)
            sb.AppendLine();
    }

    private static void AppendSystemFields(StringBuilder sb, List<RegisteredSystem> systems)
    {
        foreach (var system in systems.Where(s => s.RequiresInstance))
        {
            sb.Append("    private readonly ").Append(TypeName(system.Type)).Append(' ').Append(system.FieldName).AppendLine(";");
        }

        if (systems.Any(s => s.RequiresInstance))
            sb.AppendLine();
    }

    private static void AppendConstructor(StringBuilder sb, INamedTypeSymbol worldSymbol, List<RegisteredSystem> systems, List<ComponentInfo> components, List<ComponentSetInfo> componentSets)
    {
        var constructorParameters = new List<string> { "string name" };
        var constructorArgumentsBySystem = new Dictionary<RegisteredSystem, List<string>>();

        foreach (var system in systems.Where(s => s.RequiresInstance && s.Constructor != null))
        {
            var args = new List<string>();
            for (var i = 0; i < system.Constructor.Parameters.Length; i++)
            {
                var parameter = system.Constructor.Parameters[i];
                var parameterName = $"{system.FieldName}_{parameter.Name}";
                constructorParameters.Add($"{TypeName(parameter.Type)} {parameterName}");
                args.Add(parameterName);
            }
            constructorArgumentsBySystem[system] = args;
        }

        sb.Append("    public ").Append(worldSymbol.Name).Append('(').Append(string.Join(", ", constructorParameters)).AppendLine(") : base(name) {");

        foreach (var system in systems.Where(s => s.RequiresInstance))
        {
            var args = constructorArgumentsBySystem.TryGetValue(system, out var values) ? string.Join(", ", values) : string.Empty;
            sb.Append("        ").Append(system.FieldName).Append(" = new ").Append(TypeName(system.Type)).Append('(').Append(args).AppendLine(");");
        }

        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    protected override void GrowGeneratedCapacity_Internal(int capacity) {");
        sb.AppendLine("        var __xeno_pageCount = (capacity + __xeno_pageMask) >> __xeno_pageShift;");
        sb.AppendLine("        if (__xeno_pages == null || __xeno_pages.Length < __xeno_pageCount)");
        sb.AppendLine("            global::System.Array.Resize(ref __xeno_pages, __xeno_pageCount);");
        foreach (var set in componentSets.Where(s => s.MaterializedQuery))
        {
            sb.Append("        if (").Append(set.QuerySlotsFieldName).Append(" == null || ").Append(set.QuerySlotsFieldName).AppendLine(".Length < __xeno_pageCount)");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QuerySlotsFieldName).AppendLine(", __xeno_pageCount);");
            sb.Append("        if (").Append(set.QueryPageCountsFieldName).Append(" == null || ").Append(set.QueryPageCountsFieldName).AppendLine(".Length < __xeno_pageCount)");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QueryPageCountsFieldName).AppendLine(", __xeno_pageCount);");
            sb.Append("        if (").Append(set.QueryPageStatesFieldName).Append(" == null || ").Append(set.QueryPageStatesFieldName).AppendLine(".Length < ((__xeno_pageCount + 31) >> 5))");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QueryPageStatesFieldName).AppendLine(", (__xeno_pageCount + 31) >> 5);");
        }
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendGeneratedComponentApi(
        StringBuilder sb,
        INamedTypeSymbol worldSymbol,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        List<RequestedApiMethod> requestedApiMethods)
    {
        var singleMasks = components.ToDictionary(
            component => component,
            component => componentSets.First(set => set.Components.Length == 1 && set.Components[0] == component));

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static void __xeno_ThrowMissingComponent(string componentName) {");
        sb.AppendLine("        throw new global::System.InvalidOperationException(\"Entity does not contain component '\" + componentName + \"'.\");");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static bool __xeno_TryCopyPage<T>(T[] page, int slot, ref T component) {");
        sb.AppendLine("        if (page == null) return false;");
        sb.AppendLine("        component = page[slot];");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private int __xeno_CountMask(in global::Xeno.BitSetReadOnly mask) {");
        sb.AppendLine("        __xeno_chunkCount = MatchGeneratedChunks(in mask, ref __xeno_chunks, ref __xeno_counts);");
        sb.AppendLine("        var total = 0;");
        sb.AppendLine("        for (var i = 0; i < __xeno_chunkCount; i++) total += __xeno_counts[i];");
        sb.AppendLine("        return total;");
        sb.AppendLine("    }");
        sb.AppendLine();

        AppendGeneratedPagePoolMethods(sb, components);
        AppendGeneratedQueryMethods(sb, componentSets);
        AppendGeneratedComponentHelpers(sb, worldSymbol, components);
        AppendGeneratedSingleComponentMethods(sb, components, componentSets, singleMasks);
        AppendRequestedApiMethods(sb, requestedApiMethods, components, componentSets);
        AppendGeneratedCleanupOverride(sb, components, componentSets, singleMasks);
    }

    private static void AppendGeneratedQueryMethods(StringBuilder sb, List<ComponentSetInfo> componentSets)
    {
        var sets = componentSets.Where(s => s.MaterializedQuery).ToArray();
        if (sets.Length == 0)
            return;

        foreach (var set in sets)
        {
            var suffix = QuerySuffix(set);

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private void __xeno_EnsureQueryCapacity_").Append(suffix).AppendLine("(int pageCount) {");
            sb.Append("        if (").Append(set.QuerySlotsFieldName).Append(" == null || ").Append(set.QuerySlotsFieldName).AppendLine(".Length < pageCount)");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QuerySlotsFieldName).AppendLine(", pageCount);");
            sb.Append("        if (").Append(set.QueryPageCountsFieldName).Append(" == null || ").Append(set.QueryPageCountsFieldName).AppendLine(".Length < pageCount)");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QueryPageCountsFieldName).AppendLine(", pageCount);");
            sb.Append("        if (").Append(set.QueryPageStatesFieldName).Append(" == null || ").Append(set.QueryPageStatesFieldName).AppendLine(".Length < ((pageCount + 31) >> 5))");
            sb.Append("            global::System.Array.Resize(ref ").Append(set.QueryPageStatesFieldName).AppendLine(", (pageCount + 31) >> 5);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private void __xeno_SetQueryPageState_").Append(suffix).AppendLine("(int pid, ulong state) {");
            sb.AppendLine("        var __xeno_word = pid >> 5;");
            sb.AppendLine("        var __xeno_shift = (pid & 31) << 1;");
            sb.AppendLine("        var __xeno_mask = 3ul << __xeno_shift;");
            sb.Append("        ").Append(set.QueryPageStatesFieldName).Append("[__xeno_word] = (").Append(set.QueryPageStatesFieldName).AppendLine("[__xeno_word] & ~__xeno_mask) | (state << __xeno_shift);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private void __xeno_AddQuery_").Append(suffix).AppendLine("(int entityId) {");
            sb.AppendLine("        var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("        var bit = 1ul << (entityId & __xeno_pageMask);");
            sb.Append("        if (").Append(set.QuerySlotsFieldName).Append(" == null || pid >= ").Append(set.QuerySlotsFieldName).AppendLine(".Length)");
            sb.Append("            __xeno_EnsureQueryCapacity_").Append(suffix).AppendLine("(pid + 1);");
            sb.Append("        var oldSlots = ").Append(set.QuerySlotsFieldName).AppendLine("[pid];");
            sb.AppendLine("        if ((oldSlots & bit) != 0) return;");
            sb.Append("        var oldCount = ").Append(set.QueryPageCountsFieldName).AppendLine("[pid];");
            sb.AppendLine("        var newCount = oldCount + 1;");
            sb.Append("        ").Append(set.QuerySlotsFieldName).AppendLine("[pid] = oldSlots | bit;");
            sb.Append("        ").Append(set.QueryPageCountsFieldName).AppendLine("[pid] = newCount;");
            sb.Append("        ").Append(set.QueryCountFieldName).AppendLine("++;");
            sb.AppendLine("        if (oldCount == 0) {");
            sb.Append("            __xeno_SetQueryPageState_").Append(suffix).AppendLine("(pid, 1ul);");
            sb.AppendLine("        } else if (newCount == __xeno_pageCap) {");
            sb.Append("            __xeno_SetQueryPageState_").Append(suffix).AppendLine("(pid, 3ul);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private void __xeno_RemoveQuery_").Append(suffix).AppendLine("(int entityId) {");
            sb.AppendLine("        var pid = entityId >> __xeno_pageShift;");
            sb.Append("        if (").Append(set.QuerySlotsFieldName).Append(" == null || pid >= ").Append(set.QuerySlotsFieldName).AppendLine(".Length) return;");
            sb.AppendLine("        var bit = 1ul << (entityId & __xeno_pageMask);");
            sb.Append("        var oldSlots = ").Append(set.QuerySlotsFieldName).AppendLine("[pid];");
            sb.AppendLine("        if ((oldSlots & bit) == 0) return;");
            sb.Append("        var oldCount = ").Append(set.QueryPageCountsFieldName).AppendLine("[pid];");
            sb.AppendLine("        var newCount = oldCount - 1;");
            sb.Append("        ").Append(set.QuerySlotsFieldName).AppendLine("[pid] = oldSlots & ~bit;");
            sb.Append("        ").Append(set.QueryPageCountsFieldName).AppendLine("[pid] = newCount;");
            sb.Append("        ").Append(set.QueryCountFieldName).AppendLine("--;");
            sb.AppendLine("        if (oldCount == __xeno_pageCap) {");
            sb.AppendLine("            if (newCount != 0) {");
            sb.Append("                __xeno_SetQueryPageState_").Append(suffix).AppendLine("(pid, 1ul);");
            sb.AppendLine("            } else {");
            sb.Append("                __xeno_SetQueryPageState_").Append(suffix).AppendLine("(pid, 0ul);");
            sb.AppendLine("            }");
            sb.AppendLine("        } else if (newCount == 0) {");
            sb.Append("            __xeno_SetQueryPageState_").Append(suffix).AppendLine("(pid, 0ul);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private void __xeno_RemoveGeneratedQueries(int entityId) {");
        foreach (var set in sets)
        {
            sb.Append("        __xeno_RemoveQuery_").Append(QuerySuffix(set)).AppendLine("(entityId);");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private void __xeno_UpdateGeneratedQueries(int entityId, in global::Xeno.BitSetReadOnly newMask) {");
        foreach (var set in sets)
        {
            var suffix = QuerySuffix(set);
            sb.Append("        var new_").Append(suffix).Append(" = GeneratedMaskIncludes(in newMask, in ").Append(set.MaskFieldName).AppendLine(");");
            sb.Append("        if (new_").Append(suffix).Append(") __xeno_AddQuery_").Append(suffix).Append("(entityId); else __xeno_RemoveQuery_").Append(suffix).AppendLine("(entityId);");
        }
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendKnownGeneratedQueryAdds(StringBuilder sb, List<ComponentSetInfo> componentSets, ComponentSetInfo knownSet, string entityExpression)
    {
        foreach (var querySet in componentSets.Where(s => s.MaterializedQuery && IsSubset(s.Components, knownSet.Components)))
            sb.Append("        __xeno_AddQuery_").Append(QuerySuffix(querySet)).Append("(").Append(entityExpression).AppendLine(");");
    }

    private static bool HasKnownGeneratedQueryAdds(List<ComponentSetInfo> componentSets, ComponentSetInfo knownSet)
    {
        return componentSets.Any(s => s.MaterializedQuery && IsSubset(s.Components, knownSet.Components));
    }

    private static bool IsSubset(ComponentInfo[] subset, ComponentInfo[] set)
    {
        var setIndex = 0;
        for (var subsetIndex = 0; subsetIndex < subset.Length; subsetIndex++)
        {
            var component = subset[subsetIndex];
            while (setIndex < set.Length && set[setIndex].Index < component.Index)
                setIndex++;
            if (setIndex == set.Length || set[setIndex] != component)
                return false;
        }

        return true;
    }

    private static void AppendGeneratedPagePoolMethods(StringBuilder sb, List<ComponentInfo> components)
    {
        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            var suffix = component.Index.ToString("X2");

            if (component.Inline)
            {
                AppendGeneratedInlinePageMethods(sb, component, typeName, suffix);
                continue;
            }

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static ").Append(typeName).Append("[] __xeno_RentPage_").Append(suffix).AppendLine("() {");
            sb.Append("        if (").Append(component.PoolCountFieldName).AppendLine(" != 0) {");
            sb.Append("            var index = --").Append(component.PoolCountFieldName).AppendLine(";");
            sb.Append("            var page = ").Append(component.PoolFieldName).AppendLine("[index];");
            sb.Append("            ").Append(component.PoolFieldName).AppendLine("[index] = null;");
            sb.AppendLine("            return page;");
            sb.AppendLine("        }");
            sb.Append("        return new ").Append(typeName).AppendLine("[__xeno_pageCap];");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static void __xeno_ReturnPage_").Append(suffix).Append("(").Append(typeName).AppendLine("[] page) {");
            sb.AppendLine("        if (page == null || page.Length != __xeno_pageCap) return;");
            sb.Append("        if (").Append(component.HelperName).AppendLine(".IsReferenceOrContainsReferences)");
            sb.AppendLine("            global::System.Array.Clear(page, 0, page.Length);");
            sb.Append("        if (").Append(component.PoolFieldName).AppendLine(" == null)");
            sb.Append("            ").Append(component.PoolFieldName).Append(" = new ").Append(typeName).AppendLine("[4][];");
            sb.Append("        else if (").Append(component.PoolCountFieldName).Append(" == ").Append(component.PoolFieldName).AppendLine(".Length)");
            sb.Append("            global::System.Array.Resize(ref ").Append(component.PoolFieldName).Append(", ").Append(component.PoolFieldName).AppendLine(".Length << 1);");
            sb.Append(component.PoolFieldName).Append("[").Append(component.PoolCountFieldName).AppendLine("++] = page;");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static void __xeno_WritePage_").Append(suffix).Append("(ref __XenoPage page, int slot, in ").Append(typeName).AppendLine(" component) {");
            sb.Append("        var componentPage = page.").Append(component.PagesFieldName).AppendLine(";");
            sb.AppendLine("        if (componentPage == null) {");
            sb.Append("            componentPage = __xeno_RentPage_").Append(suffix).AppendLine("();");
            sb.Append("            page.").Append(component.PagesFieldName).AppendLine(" = componentPage;");
            sb.AppendLine("        }");
            sb.AppendLine("        componentPage[slot] = component;");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static ref ").Append(typeName).Append(" __xeno_RefPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
            sb.Append("        return ref page.").Append(component.PagesFieldName).AppendLine("[slot];");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static void __xeno_ClearPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
            sb.Append("        if (!").Append(component.HelperName).AppendLine(".IsReferenceOrContainsReferences) return;");
            sb.Append("        var componentPage = page.").Append(component.PagesFieldName).AppendLine(";");
            sb.AppendLine("        if (componentPage == null) return;");
            sb.AppendLine("        componentPage[slot] = default;");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void AppendGeneratedInlinePageMethods(StringBuilder sb, ComponentInfo component, string typeName, string suffix)
    {
        var fixedBufferTypeName = FixedBufferTypeName(component.Type);
        if (fixedBufferTypeName != null)
        {
            sb.Append("    private unsafe struct ").Append(component.InlinePageName).AppendLine(" {");
            sb.Append("        public fixed ").Append(fixedBufferTypeName).AppendLine(" __xeno_values[__xeno_pageCap];");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static unsafe void __xeno_WritePage_").Append(suffix).Append("(ref __XenoPage page, int slot, in ").Append(typeName).AppendLine(" component) {");
            sb.Append("        page.").Append(component.PagesFieldName).AppendLine(".__xeno_values[slot] = component;");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static unsafe bool __xeno_TryCopyPage_").Append(suffix).Append("(ref __XenoPage page, int slot, out ").Append(typeName).AppendLine(" component) {");
            sb.Append("        component = page.").Append(component.PagesFieldName).AppendLine(".__xeno_values[slot];");
            sb.AppendLine("        return true;");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static unsafe ref ").Append(typeName).Append(" __xeno_RefPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
            sb.Append("        return ref page.").Append(component.PagesFieldName).AppendLine(".__xeno_values[slot];");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    private static void __xeno_ClearPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
            sb.AppendLine("    }");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("    [global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential)]");
        sb.Append("    private struct ").Append(component.InlinePageName).AppendLine(" {");
        for (var i = 0; i < 64; i++)
            sb.Append("        public ").Append(typeName).Append(" __xeno_").Append(i.ToString("X2")).AppendLine(";");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.Append("    private static void __xeno_WritePage_").Append(suffix).Append("(ref __XenoPage page, int slot, in ").Append(typeName).AppendLine(" component) {");
        sb.Append("        __xeno_RefPage_").Append(suffix).AppendLine("(ref page, slot) = component;");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.Append("    private static bool __xeno_TryCopyPage_").Append(suffix).Append("(ref __XenoPage page, int slot, out ").Append(typeName).AppendLine(" component) {");
        sb.Append("        component = __xeno_RefPage_").Append(suffix).AppendLine("(ref page, slot);");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.Append("    private static ref ").Append(typeName).Append(" __xeno_RefPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
        sb.Append("        return ref global::System.Runtime.CompilerServices.Unsafe.Add(ref page.")
            .Append(component.PagesFieldName).AppendLine(".__xeno_00, slot);");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.Append("    private static void __xeno_ClearPage_").Append(suffix).AppendLine("(ref __XenoPage page, int slot) {");
        sb.Append("        if (!").Append(component.HelperName).AppendLine(".IsReferenceOrContainsReferences) return;");
        sb.Append("        __xeno_RefPage_").Append(suffix).AppendLine("(ref page, slot) = default;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendGeneratedComponentHelpers(StringBuilder sb, INamedTypeSymbol worldSymbol, List<ComponentInfo> components)
    {
        var worldTypeName = TypeName(worldSymbol);

        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            var suffix = component.Index.ToString("X2");
            sb.Append("    private static class ").Append(component.HelperName).AppendLine(" {");
            sb.Append("        internal const int Index = ").Append(component.Index).AppendLine(";");
            sb.Append("        internal static readonly bool IsReferenceOrContainsReferences = ComponentIsReferenceOrContainsReferences<").Append(typeName).AppendLine(">();");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static void Write(").Append(worldTypeName).Append(" world, int entityId, in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("            var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("            var slot = entityId & __xeno_pageMask;");
            sb.AppendLine("            ref var page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(world.__xeno_pages), pid);");
            sb.Append("            __xeno_WritePage_").Append(suffix).AppendLine("(ref page, slot, in component);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static bool TryCopy(").Append(worldTypeName).Append(" world, int entityId, out ").Append(typeName).AppendLine(" component) {");
            sb.Append("            component = default(").Append(typeName).AppendLine(");");
            sb.AppendLine("            var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("            var slot = entityId & __xeno_pageMask;");
            sb.AppendLine("            if (pid >= world.__xeno_pages.Length) return false;");
            sb.AppendLine("            ref var page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(world.__xeno_pages), pid);");
            if (component.Inline)
                sb.Append("            return __xeno_TryCopyPage_").Append(suffix).AppendLine("(ref page, slot, out component);");
            else
                sb.Append("            return __xeno_TryCopyPage(page.").Append(component.PagesFieldName).AppendLine(", slot, ref component);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static void Clear(").Append(worldTypeName).AppendLine(" world, int entityId) {");
            sb.AppendLine("            var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("            var slot = entityId & __xeno_pageMask;");
            sb.AppendLine("            if (pid >= world.__xeno_pages.Length) return;");
            sb.AppendLine("            ref var page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(world.__xeno_pages), pid);");
            sb.Append("            __xeno_ClearPage_").Append(suffix).AppendLine("(ref page, slot);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static ref ").Append(typeName).Append(" Ref(").Append(worldTypeName).AppendLine(" world, int entityId) {");
            sb.AppendLine("            var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("            var slot = entityId & __xeno_pageMask;");
            sb.AppendLine("            ref var page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(world.__xeno_pages), pid);");
            sb.Append("            return ref __xeno_RefPage_").Append(suffix).AppendLine("(ref page, slot);");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void AppendGeneratedSingleComponentMethods(
        StringBuilder sb,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        IReadOnlyDictionary<ComponentInfo, ComponentSetInfo> singleMasks)
    {
        var hasMaterializedQueries = componentSets.Any(s => s.MaterializedQuery);
        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            var helperName = component.HelperName;
            var maskFieldName = singleMasks[component].MaskFieldName;
            var apiName = component.ApiName;
            var displayName = component.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var suffix = component.Index.ToString("X2");

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    internal global::Xeno.Entity CreateEntity_NoLock(in ").Append(typeName).AppendLine(" component) {");
            sb.Append("        var __xeno_entity = CreateEntityWithMask_NoLock(in ").Append(maskFieldName)
                .Append(", ref ").Append(singleMasks[component].ArchetypeCacheFieldName).AppendLine(");");
            sb.AppendLine("        var __xeno_eid = __xeno_entity.Id;");
            sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
            sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
            sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
            sb.Append("        __xeno_WritePage_").Append(suffix).AppendLine("(ref __xeno_page, __xeno_slot, in component);");
            if (HasKnownGeneratedQueryAdds(componentSets, singleMasks[component]))
            {
                AppendKnownGeneratedQueryAdds(sb, componentSets, singleMasks[component], "__xeno_eid");
            }
            sb.AppendLine("        return __xeno_entity;");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public global::Xeno.Entity CreateEntity(in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        AssertOwnerThread();");
            sb.AppendLine("        return CreateEntity_NoLock(in component);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    internal void Add_NoLock(in global::Xeno.Entity entity, in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
            sb.AppendLine("        var __xeno_eid = entity.Id;");
            sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
            sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
            sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
            sb.Append("        __xeno_WritePage_").Append(suffix).AppendLine("(ref __xeno_page, __xeno_slot, in component);");
            sb.Append("        AddGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(maskFieldName).Append(", ")
                .Append(singleMasks[component].TransitionKey)
                .Append(", ref ").Append(singleMasks[component].AddSourceCacheFieldName)
                .Append(", ref ").Append(singleMasks[component].AddTargetCacheFieldName)
                .AppendLine(");");
            if (hasMaterializedQueries)
            {
                if (hasMaterializedQueries)
                {
                    sb.AppendLine("        var __xeno_newMask = GetGeneratedEntityMask(__xeno_eid);");
                    sb.AppendLine("        __xeno_UpdateGeneratedQueries(__xeno_eid, in __xeno_newMask);");
                }
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public void Add(in global::Xeno.Entity entity, in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        AssertOwnerThread();");
            sb.AppendLine("        Add_NoLock(entity, in component);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    internal void Remove").Append(apiName).AppendLine("_NoLock(in global::Xeno.Entity entity) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
            sb.AppendLine("        var __xeno_eid = entity.Id;");
            sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
            sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
            sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
            sb.Append("        __xeno_ClearPage_").Append(suffix).AppendLine("(ref __xeno_page, __xeno_slot);");
            sb.Append("        RemoveGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(maskFieldName).Append(", ")
                .Append(singleMasks[component].TransitionKey)
                .Append(", ref ").Append(singleMasks[component].RemoveSourceCacheFieldName)
                .Append(", ref ").Append(singleMasks[component].RemoveTargetCacheFieldName)
                .AppendLine(");");
            if (hasMaterializedQueries)
            {
                sb.AppendLine("        var __xeno_newMask = GetGeneratedEntityMask(__xeno_eid);");
                sb.AppendLine("        __xeno_UpdateGeneratedQueries(__xeno_eid, in __xeno_newMask);");
            }
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public void Remove").Append(apiName).AppendLine("(in global::Xeno.Entity entity) {");
            sb.AppendLine("        AssertOwnerThread();");
            sb.Append("        Remove").Append(apiName).AppendLine("_NoLock(entity);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public bool TryGet").Append(apiName).Append("(in global::Xeno.Entity entity, out ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) { component = default; return false; }");
            sb.Append("        if (!HasGeneratedMask(entity, in ").Append(maskFieldName).AppendLine(")) { component = default; return false; }");
            sb.Append("        return ").Append(helperName).AppendLine(".TryCopy(this, entity.Id, out component);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public ref ").Append(typeName).Append(" Ref").Append(apiName).AppendLine("(in global::Xeno.Entity entity) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) throw new global::System.InvalidOperationException();");
            sb.Append("        if (!HasGeneratedMask(entity, in ").Append(maskFieldName).Append(")) __xeno_ThrowMissingComponent(\"").Append(displayName).AppendLine("\");");
            sb.Append("        return ref ").Append(helperName).AppendLine(".Ref(this, entity.Id);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public bool Has").Append(apiName).AppendLine("(in global::Xeno.Entity entity) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return false;");
            sb.Append("        return HasGeneratedMask(entity, in ").Append(maskFieldName).AppendLine(");");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public int Count").Append(apiName).AppendLine("() {");
            sb.Append("        return __xeno_CountMask(in ").Append(maskFieldName).AppendLine(");");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void AppendRequestedApiMethods(
        StringBuilder sb,
        List<RequestedApiMethod> requestedApiMethods,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets)
    {
        var hasMaterializedQueries = componentSets.Any(s => s.MaterializedQuery);
        foreach (var method in requestedApiMethods)
        {
            var componentInfos = method.ComponentTypes
                .Select(type => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, type)))
                .ToArray();
            var set = FindComponentSet(componentSets, componentInfos);
            var parameterNames = method.ParameterNames != null && method.ParameterNames.Length == componentInfos.Length
                ? method.ParameterNames
                : componentInfos.Select((component, index) => BuildParameterName(component.Type, index)).ToArray();
            var partialModifier = method.DeclaredAsPartial ? " partial" : string.Empty;

            if (method.Kind == RequestedApiMethodKind.Count)
            {
                sb.Append("    public").Append(partialModifier).Append(" int ").Append(method.MethodName).AppendLine("() {");
            }
            else if (method.Kind == RequestedApiMethodKind.HasAll || method.Kind == RequestedApiMethodKind.HasAny)
            {
                sb.Append("    public").Append(partialModifier).Append(" ")
                    .Append("bool ")
                    .Append(method.MethodName)
                    .AppendLine("(in global::Xeno.Entity entity) {");
            }
            else if (method.Kind == RequestedApiMethodKind.Remove)
            {
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    internal void ").Append(method.MethodName).Append("_NoLock(in global::Xeno.Entity entity) {").AppendLine();
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
                sb.AppendLine("        var __xeno_eid = entity.Id;");
                sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
                sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
                sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
                for (var i = 0; i < componentInfos.Length; i++)
                    sb.Append("        __xeno_ClearPage_").Append(componentInfos[i].Index.ToString("X2")).AppendLine("(ref __xeno_page, __xeno_slot);");
                sb.Append("        RemoveGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(set.MaskFieldName).Append(", ")
                    .Append(set.TransitionKey)
                    .Append(", ref ").Append(set.RemoveSourceCacheFieldName)
                    .Append(", ref ").Append(set.RemoveTargetCacheFieldName)
                    .AppendLine(");");
                if (hasMaterializedQueries)
                {
                    sb.AppendLine("        var __xeno_newMask = GetGeneratedEntityMask(__xeno_eid);");
                    sb.AppendLine("        __xeno_UpdateGeneratedQueries(__xeno_eid, in __xeno_newMask);");
                }
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    public").Append(partialModifier).Append(" void ")
                    .Append(method.MethodName)
                    .AppendLine("(in global::Xeno.Entity entity) {");
            }
            else if (method.Kind == RequestedApiMethodKind.CreateEntity)
            {
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    internal global::Xeno.Entity CreateEntity_NoLock(")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
                sb.Append("        var __xeno_entity = CreateEntityWithMask_NoLock(in ").Append(set.MaskFieldName)
                    .Append(", ref ").Append(set.ArchetypeCacheFieldName).AppendLine(");");
                sb.AppendLine("        var __xeno_eid = __xeno_entity.Id;");
                sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
                sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
                sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
                for (var i = 0; i < componentInfos.Length; i++)
                {
                    sb.Append("        __xeno_WritePage_").Append(componentInfos[i].Index.ToString("X2")).Append("(ref __xeno_page, __xeno_slot, in ")
                        .Append(parameterNames[i]).AppendLine(");");
                }
                if (HasKnownGeneratedQueryAdds(componentSets, set))
                {
                    AppendKnownGeneratedQueryAdds(sb, componentSets, set, "__xeno_eid");
                }
                sb.AppendLine("        return __xeno_entity;");
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    public").Append(partialModifier).Append(" global::Xeno.Entity CreateEntity(")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
            }
            else
            {
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    internal void Add_NoLock(in global::Xeno.Entity entity, ")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
                sb.AppendLine("        var __xeno_eid = entity.Id;");
                sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
                sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
                sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
                for (var i = 0; i < componentInfos.Length; i++)
                {
                    sb.Append("        __xeno_WritePage_").Append(componentInfos[i].Index.ToString("X2")).Append("(ref __xeno_page, __xeno_slot, in ")
                        .Append(parameterNames[i]).AppendLine(");");
                }
                sb.Append("        AddGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(set.MaskFieldName).Append(", ")
                    .Append(set.TransitionKey)
                    .Append(", ref ").Append(set.AddSourceCacheFieldName)
                    .Append(", ref ").Append(set.AddTargetCacheFieldName)
                    .AppendLine(");");
                if (hasMaterializedQueries)
                {
                    sb.AppendLine("        var __xeno_newMask = GetGeneratedEntityMask(__xeno_eid);");
                    sb.AppendLine("        __xeno_UpdateGeneratedQueries(__xeno_eid, in __xeno_newMask);");
                }
                sb.AppendLine("    }");
                sb.AppendLine();
                sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
                sb.Append("    public").Append(partialModifier).Append(" void Add(in global::Xeno.Entity entity, ")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
            }

            if (method.Kind == RequestedApiMethodKind.Count)
            {
                sb.Append("        return __xeno_CountMask(in ").Append(set.MaskFieldName).AppendLine(");");
                sb.AppendLine("    }");
                sb.AppendLine();
                continue;
            }

            if (method.Kind == RequestedApiMethodKind.HasAll)
            {
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return false;");
                sb.Append("        return HasGeneratedMask(entity, in ").Append(set.MaskFieldName).AppendLine(");");
                sb.AppendLine("    }");
                sb.AppendLine();
                continue;
            }

            if (method.Kind == RequestedApiMethodKind.HasAny)
            {
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return false;");
                sb.Append("        return ").Append(string.Join(" || ", componentInfos.Select(component => $"HasGeneratedMask(entity, in {FindComponentSet(componentSets, new[] { component }).MaskFieldName})"))).AppendLine(";");
                sb.AppendLine("    }");
                sb.AppendLine();
                continue;
            }

            if (method.Kind == RequestedApiMethodKind.CreateEntity)
            {
                sb.AppendLine("        AssertOwnerThread();");
                sb.Append("        return CreateEntity_NoLock(")
                    .Append(string.Join(", ", parameterNames.Select(name => $"in {name}")))
                    .AppendLine(");");
            }
            else if (method.Kind == RequestedApiMethodKind.Add)
            {
                sb.AppendLine("        AssertOwnerThread();");
                sb.Append("        Add_NoLock(entity, ")
                    .Append(string.Join(", ", parameterNames.Select(name => $"in {name}")))
                    .AppendLine(");");
            }
            else if (method.Kind == RequestedApiMethodKind.Remove)
            {
                sb.AppendLine("        AssertOwnerThread();");
                sb.Append("        ").Append(method.MethodName).AppendLine("_NoLock(entity);");
            }
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void AppendGeneratedCleanupOverride(
        StringBuilder sb,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        IReadOnlyDictionary<ComponentInfo, ComponentSetInfo> singleMasks)
    {
        var hasMaterializedQueries = componentSets.Any(s => s.MaterializedQuery);
        sb.AppendLine("    protected override void ClearGeneratedEntityData(in int entityId, in global::Xeno.BitSetReadOnly mask) {");
        sb.AppendLine("        var __xeno_pid = entityId >> __xeno_pageShift;");
        sb.AppendLine("        var __xeno_slot = entityId & __xeno_pageMask;");
        if (hasMaterializedQueries)
        {
            sb.AppendLine("        __xeno_RemoveGeneratedQueries(entityId);");
        }
        sb.AppendLine("        if (__xeno_pid >= __xeno_pages.Length) return;");
        sb.AppendLine("        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages), __xeno_pid);");
        foreach (var component in components)
        {
            sb.Append("        if (GeneratedMaskIncludes(in mask, in ")
                .Append(singleMasks[component].MaskFieldName).AppendLine(")) {");
            sb.Append("            __xeno_ClearPage_").Append(component.Index.ToString("X2")).AppendLine("(ref __xeno_page, __xeno_slot);");
            sb.AppendLine("        }");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    protected override void DisposeGeneratedData_Internal() {");
        sb.AppendLine("        if (__xeno_pages == null) return;");
        sb.AppendLine("        ref var __xeno_pagesRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages);");
        sb.AppendLine("        for (var __xeno_p = 0; __xeno_p < __xeno_pages.Length; __xeno_p++) {");
        sb.AppendLine("            ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_pagesRef, __xeno_p);");
        foreach (var component in components.Where(c => !c.Inline))
        {
            sb.Append("            __xeno_ReturnPage_").Append(component.Index.ToString("X2")).Append("(__xeno_page.").Append(component.PagesFieldName).AppendLine(");");
            sb.Append("            __xeno_page.").Append(component.PagesFieldName).AppendLine(" = null;");
        }
        sb.AppendLine("        }");
        sb.AppendLine("        __xeno_pages = null;");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendStart(StringBuilder sb, List<RegisteredSystem> systems)
    {
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    public override void Start() {");
        sb.AppendLine("        ResetTicks();");
        AppendStage(sb, systems, SystemMethodType.Startup, null, null, null, null);
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendTick(
        StringBuilder sb,
        List<RegisteredSystem> systems,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var calls = TickCalls(systems).ToArray();
        var usesComponentLoop = calls.Any(c => c.ComponentParameters.Length > 0);
        var usesChunkLoop = calls.Any(c => c.ComponentParameters.Length > 0 && !FindComponentSet(
            componentSets,
            c.ComponentParameters.Select(p => components.First(component => SymbolEqualityComparer.Default.Equals(component.Type, p.Type))).ToArray()
        ).MaterializedQuery);
        var usesMaterializedLoop = calls.Any(c => c.ComponentParameters.Length > 0 && FindComponentSet(
            componentSets,
            c.ComponentParameters.Select(p => components.First(component => SymbolEqualityComparer.Default.Equals(component.Type, p.Type))).ToArray()
        ).MaterializedQuery);
        var usesDelta = calls.Any(c => MethodUsesDeltaParameter(c.Method, uniformAttributeType));
        var usesEntity = calls.Any(c => c.Method.Parameters.Any(p => IsEntityParameter(p, entityType)));
        var usedComponents = calls
            .SelectMany(c => c.ComponentParameters)
            .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
            .Distinct()
            .OrderBy(c => c.Index)
            .ToArray();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    public override void Tick(float f) {");
        AppendTickLocals(sb, usesComponentLoop, usesChunkLoop, usesMaterializedLoop, usesDelta, usesEntity, usedComponents, entityType);
        AppendStage(sb, systems, SystemMethodType.PreUpdate, components, componentSets, entityType, uniformAttributeType);
        AppendStage(sb, systems, SystemMethodType.Update, components, componentSets, entityType, uniformAttributeType);
        AppendStage(sb, systems, SystemMethodType.PostUpdate, components, componentSets, entityType, uniformAttributeType);
        sb.AppendLine("        IncrementTicks();");
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static IEnumerable<SystemCall> TickCalls(List<RegisteredSystem> systems)
    {
        return systems
            .SelectMany(s => s.Calls)
            .Where(c => c.Type is SystemMethodType.PreUpdate or SystemMethodType.Update or SystemMethodType.PostUpdate);
    }

    private static void AppendTickLocals(
        StringBuilder sb,
        bool usesComponentLoop,
        bool usesChunkLoop,
        bool usesMaterializedLoop,
        bool usesDelta,
        bool usesEntity,
        ComponentInfo[] usedComponents,
        INamedTypeSymbol entityType)
    {
        if (usesComponentLoop)
        {
            sb.AppendLine("        var __xeno_i = 0;");
            sb.AppendLine("        var __xeno_count = 0;");
            if (usesChunkLoop || usesEntity)
                sb.AppendLine("        int __xeno_eid = 0;");
            if (usesChunkLoop || usesMaterializedLoop || usesEntity)
                sb.AppendLine("        int __xeno_pid = 0;");
            sb.AppendLine("        int __xeno_slot = 0;");
            if (usesMaterializedLoop)
            {
                sb.AppendLine("        ulong __xeno_slots = 0;");
                sb.AppendLine("        ulong __xeno_pageStates = 0;");
                sb.AppendLine("        ulong __xeno_pageState = 0;");
            }
            if (usesChunkLoop)
            {
                sb.AppendLine("        var __xeno_chunkCount = 0;");
                sb.AppendLine("        var __xeno_chunk = 0;");
                sb.AppendLine("        int[] __xeno_buf = null;");
            }
        }
        if (usesEntity)
            sb.Append("        ").Append(TypeName(entityType)).AppendLine("[] __xeno_entities = null;");
        if (usesDelta)
            sb.AppendLine("        var __xeno_delta = f;");
        foreach (var component in usedComponents.Where(c => !c.Inline))
            sb.Append("        ").Append(TypeName(component.Type)).Append("[] ").Append(ComponentPageFieldName(component)).AppendLine(" = null;");
    }

    private static void AppendStop(StringBuilder sb, List<RegisteredSystem> systems)
    {
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    public override void Stop() {");
        AppendStage(sb, systems, SystemMethodType.Shutdown, null, null, null, null);
        sb.AppendLine("    }");
        sb.AppendLine();
    }

    private static void AppendStage(
        StringBuilder sb,
        List<RegisteredSystem> systems,
        SystemMethodType stage,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var calls = OrderedStageCalls(systems, stage).ToArray();
        for (var i = 0; i < calls.Length;) {
            var call = calls[i];
            if (call.ComponentParameters.Length == 0) {
                AppendDirectCall(sb, call, entityType, uniformAttributeType);
                i++;
                continue;
            }

            if (!call.Pure) {
                AppendComponentLoop(sb, new[] { call }, components, componentSets, entityType, uniformAttributeType);
                i++;
                continue;
            }

            var setKey = ComponentSetKey(GetCallComponentInfos(call, components));
            var count = 1;
            while (i + count < calls.Length) {
                var next = calls[i + count];
                if (!next.Pure || next.ComponentParameters.Length == 0)
                    break;
                if (ComponentSetKey(GetCallComponentInfos(next, components)) != setKey)
                    break;
                count++;
            }

            AppendComponentLoop(sb, calls.Skip(i).Take(count).ToArray(), components, componentSets, entityType, uniformAttributeType);
            i += count;
        }
    }

    private static void AppendDirectCall(
        StringBuilder sb,
        SystemCall call,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        AppendInvocation(sb, "        ", call, null, entityType, uniformAttributeType);
    }

    private static void AppendComponentLoop(
        StringBuilder sb,
        IReadOnlyList<SystemCall> calls,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var firstCall = calls[0];
        var componentInfos = firstCall.ComponentParameters
            .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
            .ToArray();
        var hasEntityParameter = calls.Any(call => call.Method.Parameters.Any(p => IsEntityParameter(p, entityType)));

        var set = FindComponentSet(componentSets, componentInfos);
        if (set.MaterializedQuery)
        {
            AppendMaterializedComponentLoop(sb, calls, componentInfos, set, hasEntityParameter, entityType, uniformAttributeType);
            return;
        }

        sb.Append("        __xeno_chunkCount = MatchGeneratedChunks(in ").Append(set.MaskFieldName).AppendLine(", ref __xeno_chunks, ref __xeno_counts);");
        if (hasEntityParameter)
            sb.AppendLine("        __xeno_entities = entities;");
        sb.AppendLine("        if (__xeno_chunkCount != 0) {");
        sb.AppendLine("            ref var __xeno_chunksRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_chunks);");
        sb.AppendLine("            ref var __xeno_countsRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_counts);");
        sb.AppendLine("            ref var __xeno_pagesRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages);");
        sb.AppendLine("            for (__xeno_chunk = 0; __xeno_chunk < __xeno_chunkCount; __xeno_chunk++) {");
        sb.AppendLine("                __xeno_buf = global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_chunksRef, __xeno_chunk);");
        sb.AppendLine("                __xeno_count = global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_countsRef, __xeno_chunk);");
        sb.AppendLine("                ref var __xeno_bufRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_buf);");
        sb.AppendLine("                for (__xeno_i = 0; __xeno_i < __xeno_count; __xeno_i++) {");
        sb.AppendLine("                    __xeno_eid = global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_bufRef, __xeno_i);");
        sb.AppendLine("                    __xeno_pid = __xeno_eid >> __xeno_pageShift;");
        sb.AppendLine("                    __xeno_slot = __xeno_eid & __xeno_pageMask;");
        sb.AppendLine("                    ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_pagesRef, __xeno_pid);");
        foreach (var component in componentInfos.Where(c => c.Inline && FixedBufferTypeName(c.Type) == null))
        {
            sb.Append("                    ref var ").Append(ComponentPageFieldName(component))
                .Append(" = ref __xeno_page.")
                .Append(component.PagesFieldName)
                .AppendLine(".__xeno_00;");
        }
        foreach (var component in componentInfos.Where(c => !c.Inline))
        {
            sb.Append("                    ").Append(ComponentPageFieldName(component))
                .Append(" = __xeno_page.")
                .Append(component.PagesFieldName)
                .AppendLine(";");
        }
        foreach (var call in calls)
            AppendInvocation(sb, "                    ", call, componentInfos.ToList(), entityType, uniformAttributeType);
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static void AppendMaterializedComponentLoop(
        StringBuilder sb,
        IReadOnlyList<SystemCall> calls,
        ComponentInfo[] componentInfos,
        ComponentSetInfo set,
        bool hasEntityParameter,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        void EmitPageReferences(string indent)
        {
            foreach (var component in componentInfos.Where(c => c.Inline && FixedBufferTypeName(c.Type) == null))
            {
                sb.Append(indent).Append("ref var ").Append(ComponentPageFieldName(component))
                    .Append(" = ref __xeno_page.")
                    .Append(component.PagesFieldName)
                    .AppendLine(".__xeno_00;");
            }
            foreach (var component in componentInfos.Where(c => !c.Inline))
            {
                sb.Append(indent).Append(ComponentPageFieldName(component))
                    .Append(" = __xeno_page.")
                    .Append(component.PagesFieldName)
                    .AppendLine(";");
            }
        }

        void EmitFullPageLoop(string indent)
        {
            sb.Append(indent).AppendLine("for (__xeno_slot = 0; __xeno_slot < __xeno_pageCap; __xeno_slot++) {");
            if (hasEntityParameter)
                sb.Append(indent).AppendLine("    __xeno_eid = (__xeno_pid << __xeno_pageShift) | __xeno_slot;");
            foreach (var call in calls)
                AppendInvocation(sb, indent + "    ", call, componentInfos.ToList(), entityType, uniformAttributeType);
            sb.Append(indent).AppendLine("}");
        }

        void EmitPartialPageLoop(string indent)
        {
            sb.Append(indent).AppendLine("__xeno_slot = 0;");
            sb.Append(indent).AppendLine("while (__xeno_slots != 0) {");
            sb.Append(indent).AppendLine("    while ((__xeno_slots & 1ul) == 0) {");
            sb.Append(indent).AppendLine("        __xeno_slot++;");
            sb.Append(indent).AppendLine("        __xeno_slots >>= 1;");
            sb.Append(indent).AppendLine("    }");
            sb.Append(indent).AppendLine("    do {");
            if (hasEntityParameter)
                sb.Append(indent).AppendLine("        __xeno_eid = (__xeno_pid << __xeno_pageShift) | __xeno_slot;");
            foreach (var call in calls)
                AppendInvocation(sb, indent + "        ", call, componentInfos.ToList(), entityType, uniformAttributeType);
            sb.Append(indent).AppendLine("        __xeno_slot++;");
            sb.Append(indent).AppendLine("        __xeno_slots >>= 1;");
            sb.Append(indent).AppendLine("    } while ((__xeno_slots & 1ul) != 0);");
            sb.Append(indent).AppendLine("}");
        }

        if (hasEntityParameter)
            sb.AppendLine("        __xeno_entities = entities;");

        sb.Append("        __xeno_count = ").Append(set.QueryCountFieldName).AppendLine(";");
        sb.AppendLine("        if (__xeno_count != 0) {");
        sb.Append("            ref var __xeno_pageStatesRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(").Append(set.QueryPageStatesFieldName).AppendLine(");");
        sb.Append("            ref var __xeno_querySlotsRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(").Append(set.QuerySlotsFieldName).AppendLine(");");
        sb.AppendLine("            ref var __xeno_pagesRef = ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_pages);");
        sb.Append("            __xeno_count = ").Append(set.QueryPageStatesFieldName).AppendLine(".Length;");
        sb.AppendLine("            for (__xeno_i = 0; __xeno_i < __xeno_count; __xeno_i++) {");
        sb.AppendLine("                __xeno_pageStates = global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_pageStatesRef, __xeno_i);");
        sb.AppendLine("                if (__xeno_pageStates == 0ul) continue;");
        sb.AppendLine("                __xeno_pid = __xeno_i << 5;");
        sb.AppendLine("                while (__xeno_pageStates != 0ul) {");
        sb.AppendLine("                    __xeno_pageState = __xeno_pageStates & 3ul;");
        sb.AppendLine("                    if (__xeno_pageState != 0ul) {");
        sb.AppendLine("                        __xeno_slots = global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_querySlotsRef, __xeno_pid);");
        sb.AppendLine("                        ref var __xeno_page = ref global::System.Runtime.CompilerServices.Unsafe.Add(ref __xeno_pagesRef, __xeno_pid);");
        EmitPageReferences("                        ");
        sb.AppendLine("                        if (__xeno_pageState == 3ul) {");
        EmitFullPageLoop("                            ");
        sb.AppendLine("                        } else {");
        EmitPartialPageLoop("                            ");
        sb.AppendLine("                        }");
        sb.AppendLine("                    }");
        sb.AppendLine("                    __xeno_pid++;");
        sb.AppendLine("                    __xeno_pageStates >>= 2;");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    private static ComponentSetInfo FindComponentSet(List<ComponentSetInfo> componentSets, ComponentInfo[] componentInfos)
    {
        var ordered = componentInfos.OrderBy(c => c.Index).ToArray();
        return componentSets.First(s => s.Components.Length == ordered.Length
                                        && s.Components.Zip(ordered, (a, b) => a.Index == b.Index).All(v => v));
    }

    private static ComponentInfo[] GetCallComponentInfos(SystemCall call, List<ComponentInfo> components)
    {
        return call.ComponentParameters
            .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
            .ToArray();
    }

    private static string ComponentSetKey(ComponentInfo[] componentInfos)
    {
        return string.Join(",", componentInfos.OrderBy(c => c.Index).Select(c => c.Index));
    }

    private static string QuerySuffix(ComponentSetInfo set)
    {
        return string.Join("_", set.Components.Select(c => c.Index.ToString("X2")));
    }

    private static IEnumerable<SystemCall> OrderedStageCalls(List<RegisteredSystem> systems, SystemMethodType stage)
    {
        return systems
            .SelectMany(s => s.Calls)
            .Where(c => c.Type == stage)
            .OrderBy(c => c.Order)
            .ThenBy(c => c.System.Order)
            .ThenBy(c => c.System.Index)
            .ThenBy(c => c.Index);
    }

    private static string SystemInvocationTarget(SystemCall call)
    {
        return call.Method.IsStatic
            ? $"{TypeName(call.System.Type)}.{call.Method.Name}"
            : $"{call.System.FieldName}.{call.Method.Name}";
    }

    private static void AppendInvocation(
        StringBuilder sb,
        string indent,
        SystemCall call,
        List<ComponentInfo> components,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType,
        string slotExpression = "__xeno_slot")
    {
        var target = SystemInvocationTarget(call);
        var arguments = string.Join(", ", call.Method.Parameters.Select(p => ArgumentExpression(p, components, entityType, uniformAttributeType, slotExpression)));

        if (RequiresUnsafeInvocation(call, components, entityType, uniformAttributeType))
            sb.Append(indent).Append("unsafe { ").Append(target).Append('(').Append(arguments).AppendLine("); }");
        else
            sb.Append(indent).Append(target).Append('(').Append(arguments).AppendLine(");");
    }

    private static bool RequiresUnsafeInvocation(
        SystemCall call,
        List<ComponentInfo> components,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        if (components == null)
            return false;

        foreach (var parameter in call.Method.Parameters)
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type, entityType))
                continue;
            if (IsUniformParameter(parameter, uniformAttributeType, out _, out _))
                continue;

            var component = components.FirstOrDefault(c => SymbolEqualityComparer.Default.Equals(c.Type, parameter.Type));
            if (component is { Inline: true } && FixedBufferTypeName(component.Type) != null)
                return true;
        }

        return false;
    }

    private static string ComponentPageFieldName(ComponentInfo component) => $"{component.PagesFieldName}_page";

    private static string PageStorageTypeName(ComponentInfo component)
    {
        return component.Inline ? component.InlinePageName : $"{TypeName(component.Type)}[]";
    }

    private static string FixedBufferTypeName(ITypeSymbol type)
    {
        return type.SpecialType switch {
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Byte => "byte",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Char => "char",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            _ => null,
        };
    }

    private static string ArgumentExpression(
        IParameterSymbol parameter,
        List<ComponentInfo> components,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType,
        string slotExpression = "__xeno_slot")
    {
        var prefix = parameter.RefKind switch {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            _ => string.Empty,
        };

        if (SymbolEqualityComparer.Default.Equals(parameter.Type, entityType) && parameter.RefKind == RefKind.In)
            return "global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_entities), __xeno_eid)";

        if (IsUniformParameter(parameter, uniformAttributeType, out var kind, out var name))
        {
            return kind switch {
                UniformKind.Delta => $"{prefix}__xeno_delta",
                UniformKind.Named => $"{prefix}{name}",
                _ => $"{prefix}default",
            };
        }

        if (components != null)
        {
            var component = components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, parameter.Type));
            if (component.Inline)
            {
                if (FixedBufferTypeName(component.Type) != null)
                    return $"ref __xeno_page.{component.PagesFieldName}.__xeno_values[{slotExpression}]";
                else
                    return $"ref global::System.Runtime.CompilerServices.Unsafe.Add(ref {ComponentPageFieldName(component)}, {slotExpression})";
            }
            return $"ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference({ComponentPageFieldName(component)}), {slotExpression})";
        }

        return $"{prefix}default";
    }

    private static bool MethodUsesDeltaParameter(IMethodSymbol method, INamedTypeSymbol uniformAttributeType)
    {
        foreach (var parameter in method.Parameters)
        {
            if (IsUniformParameter(parameter, uniformAttributeType, out var kind, out _) && kind == UniformKind.Delta)
                return true;
        }

        return false;
    }

    private static bool TryGetSystemMethod(IMethodSymbol method, INamedTypeSymbol systemMethodAttributeType, out SystemMethodType type, out int order, out bool pure)
    {
        type = default;
        order = 0;
        pure = false;

        var attribute = method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, systemMethodAttributeType));
        if (attribute == null || attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Value == null)
            return false;

        type = (SystemMethodType)(int)attribute.ConstructorArguments[0].Value;
        order = attribute.ConstructorArguments.Length > 1 && attribute.ConstructorArguments[1].Value is int value ? value : 0;
        pure = TryGetNamedBool(attribute, "Pure")
               || (attribute.ConstructorArguments.Length > 2
                   && attribute.ConstructorArguments[2].Value is bool positionalPure
                   && positionalPure);
        return true;
    }

    private static bool IsSupportedSystemMethod(
        IMethodSymbol method,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        if (!method.ReturnsVoid)
            return false;

        var parameters = method.Parameters;
        if (parameters.Length == 0)
            return true;

        var componentCount = parameters.Count(p => IsComponentParameter(p, entityType, uniformAttributeType));
        if (componentCount == 0)
            return parameters.All(p => IsUniformParameter(p, uniformAttributeType, out _, out _));

        return parameters.All(p => IsComponentParameter(p, entityType, uniformAttributeType)
                                   || IsEntityParameter(p, entityType)
                                   || IsUniformParameter(p, uniformAttributeType, out _, out _));
    }

    private static bool IsComponentParameter(IParameterSymbol parameter, INamedTypeSymbol entityType, INamedTypeSymbol uniformAttributeType)
    {
        return parameter.RefKind == RefKind.Ref
               && !IsEntityParameter(parameter, entityType)
               && !IsUniformParameter(parameter, uniformAttributeType, out _, out _);
    }

    private static bool IsEntityParameter(IParameterSymbol parameter, INamedTypeSymbol entityType)
    {
        return parameter.RefKind == RefKind.In
               && SymbolEqualityComparer.Default.Equals(parameter.Type, entityType);
    }

    private static bool IsUniformParameter(IParameterSymbol parameter, INamedTypeSymbol uniformAttributeType, out UniformKind kind, out string name)
    {
        kind = default;
        name = null;

        if (parameter.RefKind != RefKind.In && parameter.RefKind != RefKind.Ref)
            return false;

        var attribute = parameter.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, uniformAttributeType));
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
            default:
                return false;
        }
    }

    private static bool TryGetNamedBool(AttributeData attribute, string name)
    {
        foreach (var argument in attribute.NamedArguments)
        {
            if (argument.Key == name && argument.Value.Value is bool value)
                return value;
        }

        return false;
    }

    private static bool TryParseNamedRequestedApiMethod(string methodName, out RequestedApiMethodKind kind, out string[] apiNames)
    {
        kind = default;
        apiNames = Array.Empty<string>();

        if (methodName.StartsWith("Count", StringComparison.Ordinal))
        {
            kind = RequestedApiMethodKind.Count;
            return TrySplitComponentApiNames(methodName.Substring("Count".Length), "And", out apiNames);
        }

        if (methodName.StartsWith("HasAny", StringComparison.Ordinal))
        {
            kind = RequestedApiMethodKind.HasAny;
            return TrySplitComponentApiNames(methodName.Substring("HasAny".Length), "Or", out apiNames);
        }

        if (methodName.StartsWith("Has", StringComparison.Ordinal))
        {
            kind = RequestedApiMethodKind.HasAll;
            return TrySplitComponentApiNames(methodName.Substring("Has".Length), "And", out apiNames);
        }

        if (methodName.StartsWith("Remove", StringComparison.Ordinal))
        {
            kind = RequestedApiMethodKind.Remove;
            return TrySplitComponentApiNames(methodName.Substring("Remove".Length), "And", out apiNames);
        }

        return false;
    }

    private static bool TrySplitComponentApiNames(string value, string separator, out string[] apiNames)
    {
        apiNames = Array.Empty<string>();
        if (string.IsNullOrEmpty(value))
            return false;

        var parts = value.Split(new[] { separator }, StringSplitOptions.None);
        if (parts.Length < 2 || parts.Any(string.IsNullOrEmpty))
            return false;

        apiNames = parts;
        return true;
    }

    private static bool TryResolveComponentApiNames(
        IReadOnlyDictionary<string, INamedTypeSymbol> knownComponentsByApiName,
        IReadOnlyList<string> apiNames,
        out INamedTypeSymbol[] componentTypes)
    {
        componentTypes = new INamedTypeSymbol[apiNames.Count];
        for (var i = 0; i < apiNames.Count; i++)
        {
            if (!knownComponentsByApiName.TryGetValue(apiNames[i], out var componentType))
                return false;

            componentTypes[i] = componentType;
        }

        return true;
    }

    private static string BuildApiName(INamedTypeSymbol type)
    {
        var raw = type.Name;
        var tick = raw.IndexOf('`');
        if (tick >= 0)
            raw = raw.Substring(0, tick);

        var sb = new StringBuilder(raw.Length + 8);
        foreach (var ch in raw)
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
                sb.Append(ch);
        }

        if (sb.Length == 0 || !char.IsLetter(sb[0]) && sb[0] != '_')
            sb.Insert(0, "C");

        if (sb.Length == 0)
            sb.Append("Component");
        return sb.ToString();
    }

    private static string BuildParameterName(INamedTypeSymbol type, int index)
    {
        var apiName = BuildApiName(type);
        if (string.IsNullOrEmpty(apiName))
            return $"component{index + 1}";

        return char.ToLowerInvariant(apiName[0]) + apiName.Substring(1);
    }

    private static bool IsSameOrDerived(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        return SymbolEqualityComparer.Default.Equals(type, baseType) || IsDerivedFrom(type, baseType);
    }

    private static bool IsDerivedFrom(INamedTypeSymbol type, INamedTypeSymbol baseType)
    {
        for (var current = type.BaseType; current != null; current = current.BaseType)
            if (SymbolEqualityComparer.Default.Equals(current, baseType))
                return true;

        return false;
    }

    private static string TypeName(ITypeSymbol symbol)
    {
        return symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
