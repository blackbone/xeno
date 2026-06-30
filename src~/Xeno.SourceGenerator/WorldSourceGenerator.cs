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
    }

    private sealed class SystemCall
    {
        public RegisteredSystem System;
        public IMethodSymbol Method;
        public SystemMethodType Type;
        public int Order;
        public int Index;
        public bool NoFuse;
        public ImmutableArray<IParameterSymbol> ComponentParameters;
    }

    private sealed class ComponentInfo
    {
        public INamedTypeSymbol Type;
        public int Index;
        public string ApiName;
        public string HelperName;
        public string PagesFieldName;
    }

    private sealed class ComponentSetInfo
    {
        public ComponentInfo[] Components;
        public string MaskFieldName;
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
            };

            system.Constructor = SelectConstructor(systemType);
            system.RequiresInstance = systemType.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => TryGetSystemMethod(m, systemMethodAttributeType, out _, out _, out _))
                .Any(m => !m.IsStatic);

            foreach (var method in systemType.GetMembers().OfType<IMethodSymbol>())
            {
                if (!TryGetSystemMethod(method, systemMethodAttributeType, out var type, out var methodOrder, out var noFuse))
                    continue;

                var call = new SystemCall {
                    System = system,
                    Method = method,
                    Type = type,
                    Order = methodOrder,
                    Index = system.Calls.Count,
                    NoFuse = noFuse,
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
        var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

        void Add(INamedTypeSymbol type)
        {
            if (type == null || !seen.Add(type))
                return;

            var index = components.Count;
            var apiName = BuildApiName(type);
            components.Add(new ComponentInfo {
                Type = type,
                Index = index,
                ApiName = apiName,
                HelperName = $"__xeno_{apiName}",
                PagesFieldName = $"__xeno_pages_{index:X2}",
            });
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

        foreach (var componentType in requestedApiMethods.SelectMany(m => m.ComponentTypes))
            Add(componentType);

        return components;
    }

    private static List<ComponentSetInfo> CollectComponentSets(List<RegisteredSystem> systems, List<RequestedApiMethod> requestedApiMethods, List<ComponentInfo> components)
    {
        var sets = new List<ComponentSetInfo>();
        var seen = new HashSet<string>();

        void Add(IEnumerable<ComponentInfo> source)
        {
            var set = source.OrderBy(c => c.Index).ToArray();
            if (set.Length == 0)
                return;

            var key = string.Join(",", set.Select(c => c.Index));
            if (!seen.Add(key))
                return;

            sets.Add(new ComponentSetInfo {
                Components = set,
                MaskFieldName = $"__xeno_mask_{string.Join("_", set.Select(c => c.ApiName))}",
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
        sb.AppendLine("    private uint[][] __xeno_chunks;");
        sb.AppendLine("    private int[] __xeno_counts;");
        sb.AppendLine("    private uint[] __xeno_buf;");
        sb.AppendLine("    private global::Xeno.Entity[] __xeno_entities;");
        sb.AppendLine("    private int __xeno_chunkCount;");
        sb.AppendLine("    private int __xeno_chunk;");
        sb.AppendLine("    private int __xeno_i;");
        sb.AppendLine("    private int __xeno_count;");
        sb.AppendLine("    private float __xeno_delta;");
        sb.AppendLine();

        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            sb.Append("    private ").Append(typeName).Append("[][] ").Append(component.PagesFieldName).AppendLine(";");
            sb.Append("    private ").Append(typeName).Append("[] ").Append(ComponentPageFieldName(component)).AppendLine(";");
        }

        foreach (var set in componentSets)
        {
            sb.Append("    private static readonly global::Xeno.BitSetReadOnly ").Append(set.MaskFieldName).Append(" = CreateGeneratedMask(")
                .Append(string.Join(", ", set.Components.Select(c => $"{c.HelperName}.Index")))
                .AppendLine(");");
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

        foreach (var component in components)
        {
            sb.Append("        ").Append(component.PagesFieldName).Append(" = new ").Append(TypeName(component.Type)).AppendLine("[32][];");
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
        sb.AppendLine("    private static void __xeno_WritePage<T>(ref T[][] pages, uint entityId, T component) {");
        sb.AppendLine("        var pid = entityId >> __xeno_pageShift;");
        sb.AppendLine("        if (pid >= pages.Length) global::System.Array.Resize(ref pages, (int)pid + 1);");
        sb.AppendLine("        pages[pid] ??= new T[__xeno_pageCap];");
        sb.AppendLine("        pages[pid][entityId & __xeno_pageMask] = component;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static bool __xeno_TryCopyPage<T>(T[][] pages, uint entityId, ref T component) {");
        sb.AppendLine("        var pid = entityId >> __xeno_pageShift;");
        sb.AppendLine("        if (pid >= pages.Length) return false;");
        sb.AppendLine("        var page = pages[pid];");
        sb.AppendLine("        if (page == null) return false;");
        sb.AppendLine("        component = page[entityId & __xeno_pageMask];");
        sb.AppendLine("        return true;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static void __xeno_ClearPage<T>(T[][] pages, uint entityId) {");
        sb.AppendLine("        var pid = entityId >> __xeno_pageShift;");
        sb.AppendLine("        if (pid >= pages.Length) return;");
        sb.AppendLine("        var page = pages[pid];");
        sb.AppendLine("        if (page == null) return;");
        sb.AppendLine("        page[entityId & __xeno_pageMask] = default;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.AppendLine("    private static void __xeno_ClearPageAt<T>(T[][] pages, uint pid, uint slot) {");
        sb.AppendLine("        if (pid >= pages.Length) return;");
        sb.AppendLine("        var page = pages[pid];");
        sb.AppendLine("        if (page == null) return;");
        sb.AppendLine("        page[slot] = default;");
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

        AppendGeneratedComponentHelpers(sb, worldSymbol, components);
        AppendGeneratedSingleComponentMethods(sb, components, singleMasks);
        AppendRequestedApiMethods(sb, requestedApiMethods, components, componentSets);
        AppendGeneratedCleanupOverride(sb, components, singleMasks);
    }

    private static void AppendGeneratedComponentHelpers(StringBuilder sb, INamedTypeSymbol worldSymbol, List<ComponentInfo> components)
    {
        var worldTypeName = TypeName(worldSymbol);

        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            sb.Append("    private static class ").Append(component.HelperName).AppendLine(" {");
            sb.Append("        internal const int Index = ").Append(component.Index).AppendLine(";");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static void Write(").Append(worldTypeName).Append(" world, uint entityId, in ").Append(typeName).AppendLine(" component) {");
            sb.Append("            __xeno_WritePage(ref world.").Append(component.PagesFieldName).AppendLine(", entityId, component);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static bool TryCopy(").Append(worldTypeName).Append(" world, uint entityId, out ").Append(typeName).AppendLine(" component) {");
            sb.Append("            component = default(").Append(typeName).AppendLine(");");
            sb.Append("            return __xeno_TryCopyPage(world.").Append(component.PagesFieldName).AppendLine(", entityId, ref component);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static void Clear(").Append(worldTypeName).Append(" world, uint entityId) {");
            sb.Append(" __xeno_ClearPage(world.").Append(component.PagesFieldName).AppendLine(", entityId); }");
            sb.AppendLine();
            sb.AppendLine("        [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("        internal static ref ").Append(typeName).Append(" Ref(").Append(worldTypeName).AppendLine(" world, uint entityId) {");
            sb.AppendLine("            var pid = entityId >> __xeno_pageShift;");
            sb.AppendLine("            var slot = entityId & __xeno_pageMask;");
            sb.Append("            return ref world.").Append(component.PagesFieldName).AppendLine("[pid][slot];");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();
        }
    }

    private static void AppendGeneratedSingleComponentMethods(
        StringBuilder sb,
        List<ComponentInfo> components,
        IReadOnlyDictionary<ComponentInfo, ComponentSetInfo> singleMasks)
    {
        foreach (var component in components)
        {
            var typeName = TypeName(component.Type);
            var helperName = component.HelperName;
            var maskFieldName = singleMasks[component].MaskFieldName;
            var apiName = component.ApiName;
            var displayName = component.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public global::Xeno.Entity CreateEntity_NoLock(in ").Append(typeName).AppendLine(" component) {");
            sb.Append("        var __xeno_entity = CreateEntityWithMask_NoLock(in ").Append(maskFieldName).AppendLine(");");
            sb.Append("        ").Append(helperName).AppendLine(".Write(this, __xeno_entity.Id, in component);");
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
            sb.Append("    public void Add_NoLock(in global::Xeno.Entity entity, in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
            sb.AppendLine("        var __xeno_eid = entity.Id;");
            sb.Append("        ").Append(helperName).AppendLine(".Write(this, __xeno_eid, in component);");
            sb.Append("        AddGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(maskFieldName).AppendLine(");");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public void Add(in global::Xeno.Entity entity, in ").Append(typeName).AppendLine(" component) {");
            sb.AppendLine("        AssertOwnerThread();");
            sb.AppendLine("        Add_NoLock(entity, in component);");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
            sb.Append("    public void Remove").Append(apiName).AppendLine("_NoLock(in global::Xeno.Entity entity) {");
            sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
            sb.AppendLine("        var __xeno_eid = entity.Id;");
            sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
            sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
            sb.Append("        __xeno_ClearPageAt(this.").Append(component.PagesFieldName).AppendLine(", __xeno_pid, __xeno_slot);");
            sb.Append("        RemoveGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(maskFieldName).AppendLine(");");
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
                sb.Append("    public void ").Append(method.MethodName).Append("_NoLock(in global::Xeno.Entity entity) {").AppendLine();
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
                sb.AppendLine("        var __xeno_eid = entity.Id;");
                sb.AppendLine("        var __xeno_pid = __xeno_eid >> __xeno_pageShift;");
                sb.AppendLine("        var __xeno_slot = __xeno_eid & __xeno_pageMask;");
                for (var i = 0; i < componentInfos.Length; i++)
                    sb.Append("        __xeno_ClearPageAt(this.").Append(componentInfos[i].PagesFieldName).AppendLine(", __xeno_pid, __xeno_slot);");
                sb.Append("        RemoveGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(set.MaskFieldName).AppendLine(");");
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
                sb.Append("    public global::Xeno.Entity CreateEntity_NoLock(")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
                sb.Append("        var __xeno_entity = CreateEntityWithMask_NoLock(in ").Append(set.MaskFieldName).AppendLine(");");
                for (var i = 0; i < componentInfos.Length; i++)
                {
                    sb.Append("        ").Append(componentInfos[i].HelperName).Append(".Write(this, __xeno_entity.Id, in ")
                        .Append(parameterNames[i]).AppendLine(");");
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
                sb.Append("    public void Add_NoLock(in global::Xeno.Entity entity, ")
                    .Append(string.Join(", ", componentInfos.Select((component, index) => $"in {TypeName(component.Type)} {parameterNames[index]}")))
                    .AppendLine(") {");
                sb.AppendLine("        if (!IsEntityValid_Internal(entity)) return;");
                sb.AppendLine("        var __xeno_eid = entity.Id;");
                for (var i = 0; i < componentInfos.Length; i++)
                {
                    sb.Append("        ").Append(componentInfos[i].HelperName).Append(".Write(this, __xeno_eid, in ")
                        .Append(parameterNames[i]).AppendLine(");");
                }
                sb.Append("        AddGeneratedMask_NoLock_Valid(__xeno_eid, in ").Append(set.MaskFieldName).AppendLine(");");
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
        IReadOnlyDictionary<ComponentInfo, ComponentSetInfo> singleMasks)
    {
        sb.AppendLine("    protected override void ClearGeneratedEntityData(in uint entityId, in global::Xeno.BitSetReadOnly mask) {");
        foreach (var component in components)
        {
            sb.Append("        if (GeneratedMaskIncludes(in mask, in ")
                .Append(singleMasks[component].MaskFieldName).AppendLine(")) {");
            sb.Append("            __xeno_ClearPage(").Append(component.PagesFieldName).AppendLine(", entityId);");
            sb.AppendLine("        }");
        }
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
        var usesDelta = calls.Any(c => MethodUsesDeltaParameter(c.Method, uniformAttributeType));
        var usesEntity = calls.Any(c => c.Method.Parameters.Any(p => IsEntityParameter(p, entityType)));
        var usedComponents = calls
            .SelectMany(c => c.ComponentParameters)
            .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
            .Distinct()
            .OrderBy(c => c.Index)
            .ToArray();
        var directCalls = calls
            .Where(c => TryGetDirectFunctionPointerTarget(c, out _))
            .ToArray();

        sb.AppendLine("    [MethodImpl(MethodImplOptions.AggressiveInlining)]");
        sb.Append("    public ");
        if (directCalls.Length > 0)
            sb.Append("unsafe ");
        sb.AppendLine("override void Tick(float f) {");
        AppendTickLocals(sb, usesComponentLoop, usesDelta, usesEntity, usedComponents, directCalls, entityType);
        AppendStage(sb, systems, SystemMethodType.PreUpdate, components, componentSets, entityType, uniformAttributeType, true);
        AppendStage(sb, systems, SystemMethodType.Update, components, componentSets, entityType, uniformAttributeType, true);
        AppendStage(sb, systems, SystemMethodType.PostUpdate, components, componentSets, entityType, uniformAttributeType, true);
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
        bool usesDelta,
        bool usesEntity,
        ComponentInfo[] usedComponents,
        SystemCall[] directCalls,
        INamedTypeSymbol entityType)
    {
        if (usesComponentLoop)
        {
            sb.AppendLine("        var __xeno_chunkCount = 0;");
            sb.AppendLine("        var __xeno_chunk = 0;");
            sb.AppendLine("        var __xeno_i = 0;");
            sb.AppendLine("        var __xeno_count = 0;");
            sb.AppendLine("        uint __xeno_eid = 0;");
            sb.AppendLine("        uint __xeno_pid = 0;");
            sb.AppendLine("        uint __xeno_slot = 0;");
            sb.AppendLine("        uint[] __xeno_buf = null;");
        }
        if (usesEntity)
            sb.Append("        ").Append(TypeName(entityType)).AppendLine("[] __xeno_entities = null;");
        if (usesDelta)
            sb.AppendLine("        var __xeno_delta = f;");
        foreach (var component in usedComponents)
            sb.Append("        ").Append(TypeName(component.Type)).Append("[] ").Append(ComponentPageFieldName(component)).AppendLine(" = null;");
        foreach (var call in directCalls)
        {
            TryGetDirectFunctionPointerTarget(call, out var target);
            sb.Append("        ").Append(DirectFunctionPointerType(call)).Append(' ')
                .Append(DirectFunctionPointerLocalName(call)).Append(" = ").Append(target).AppendLine(";");
        }
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
        INamedTypeSymbol uniformAttributeType,
        bool useDirectFunctionPointerLocals = false)
    {
        foreach (var call in OrderedStageCalls(systems, stage))
            if (call.ComponentParameters.Length == 0)
                AppendDirectCall(sb, call, entityType, uniformAttributeType, useDirectFunctionPointerLocals);
            else
                AppendComponentLoop(sb, call, components, componentSets, entityType, uniformAttributeType, useDirectFunctionPointerLocals);
    }

    private static void AppendDirectCall(
        StringBuilder sb,
        SystemCall call,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType,
        bool useDirectFunctionPointerLocals)
    {
        AppendInvocation(sb, "        ", call, null, entityType, uniformAttributeType, useDirectFunctionPointerLocals);
    }

    private static void AppendComponentLoop(
        StringBuilder sb,
        SystemCall call,
        List<ComponentInfo> components,
        List<ComponentSetInfo> componentSets,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType,
        bool useDirectFunctionPointerLocals)
    {
        var componentInfos = call.ComponentParameters
            .Select(p => components.First(c => SymbolEqualityComparer.Default.Equals(c.Type, p.Type)))
            .ToArray();
        var hasEntityParameter = call.Method.Parameters.Any(p => IsEntityParameter(p, entityType));

        var set = FindComponentSet(componentSets, componentInfos);

        sb.Append("        __xeno_chunkCount = MatchGeneratedChunks(in ").Append(set.MaskFieldName).AppendLine(", ref __xeno_chunks, ref __xeno_counts);");
        if (hasEntityParameter)
            sb.AppendLine("        __xeno_entities = entities;");
        sb.AppendLine("        for (__xeno_chunk = 0; __xeno_chunk < __xeno_chunkCount; __xeno_chunk++) {");
        sb.AppendLine("            __xeno_buf = global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_chunks), __xeno_chunk);");
        sb.AppendLine("            __xeno_count = global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_counts), __xeno_chunk);");
        sb.AppendLine("            for (__xeno_i = 0; __xeno_i < __xeno_count; __xeno_i++) {");
        sb.AppendLine("                __xeno_eid = global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_buf), __xeno_i);");
        sb.AppendLine("                __xeno_pid = __xeno_eid >> __xeno_pageShift;");
        sb.AppendLine("                __xeno_slot = __xeno_eid & __xeno_pageMask;");
        foreach (var component in componentInfos)
        {
            sb.Append("                ").Append(ComponentPageFieldName(component))
                .Append(" = global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(")
                .Append(component.PagesFieldName)
                .AppendLine("), (int)__xeno_pid);");
        }
        AppendInvocation(sb, "                ", call, components, entityType, uniformAttributeType, useDirectFunctionPointerLocals);
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
        if (TryGetDirectFunctionPointerTarget(call, out var directTarget))
            return directTarget;

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
        bool useDirectFunctionPointerLocal = false)
    {
        var hasDirectTarget = TryGetDirectFunctionPointerTarget(call, out var directTarget);
        var target = hasDirectTarget && useDirectFunctionPointerLocal
            ? DirectFunctionPointerLocalName(call)
            : hasDirectTarget
                ? directTarget
                : SystemInvocationTarget(call);
        var arguments = string.Join(", ", call.Method.Parameters.Select(p => ArgumentExpression(p, components, entityType, uniformAttributeType)));

        if (hasDirectTarget && !useDirectFunctionPointerLocal)
        {
            sb.Append(indent).Append("unsafe { ").Append(target).Append('(').Append(arguments).AppendLine("); }");
            return;
        }

        sb.Append(indent).Append(target).Append('(').Append(arguments).AppendLine(");");
    }

    private static bool TryGetDirectFunctionPointerTarget(SystemCall call, out string target)
    {
        target = null;

        if (!call.Method.IsStatic || call.Method.Name != "Run")
            return false;

        var type = call.System.Type;
        if (type.ContainingNamespace?.ToDisplayString() != "Benchmark.Xeno")
            return false;

        var holderArity = type.Name switch {
            "XenoSystem1" => 1,
            "XenoSystem2" => 2,
            "XenoSystem3" => 3,
            _ => 0,
        };

        if (holderArity == 0 || type.TypeArguments.Length != holderArity)
            return false;

        target = $"global::Benchmark.Xeno.XenoSystemHolder{holderArity}<{string.Join(", ", type.TypeArguments.Select(TypeName))}>.Method";
        return true;
    }

    private static string DirectFunctionPointerLocalName(SystemCall call)
    {
        return $"__xeno_method_{call.System.Index:X2}_{call.Index:X2}";
    }

    private static string DirectFunctionPointerType(SystemCall call)
    {
        var parameters = call.Method.Parameters
            .Select(p => $"{RefKindPrefix(p.RefKind)}{TypeName(p.Type)}")
            .Concat(new[] { TypeName(call.Method.ReturnType) });

        return $"delegate*<{string.Join(", ", parameters)}>";
    }

    private static string RefKindPrefix(RefKind refKind)
    {
        return refKind switch {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            RefKind.In => "in ",
            _ => string.Empty,
        };
    }

    private static string ComponentPageFieldName(ComponentInfo component) => $"{component.PagesFieldName}_page";

    private static string ArgumentExpression(
        IParameterSymbol parameter,
        List<ComponentInfo> components,
        INamedTypeSymbol entityType,
        INamedTypeSymbol uniformAttributeType)
    {
        var prefix = parameter.RefKind switch {
            RefKind.Ref => "ref ",
            RefKind.Out => "out ",
            _ => string.Empty,
        };

        if (SymbolEqualityComparer.Default.Equals(parameter.Type, entityType) && parameter.RefKind == RefKind.In)
            return "global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(__xeno_entities), (int)__xeno_eid)";

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
            return $"ref global::System.Runtime.CompilerServices.Unsafe.Add(ref global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference({ComponentPageFieldName(component)}), (int)__xeno_slot)";
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

    private static bool TryGetSystemMethod(IMethodSymbol method, INamedTypeSymbol systemMethodAttributeType, out SystemMethodType type, out int order, out bool noFuse)
    {
        type = default;
        order = 0;
        noFuse = false;

        var attribute = method.GetAttributes().FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, systemMethodAttributeType));
        if (attribute == null || attribute.ConstructorArguments.Length == 0 || attribute.ConstructorArguments[0].Value == null)
            return false;

        type = (SystemMethodType)(int)attribute.ConstructorArguments[0].Value;
        order = attribute.ConstructorArguments.Length > 1 && attribute.ConstructorArguments[1].Value is int value ? value : 0;
        noFuse = TryGetNamedBool(attribute, "NoFuse");
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
