using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xeno.Generators;
using Xeno.SourceGenerator.Collectors;

namespace Xeno.SourceGenerator;

internal class AssemblyInfo {
    public readonly Generation Generation;
    public readonly IAssemblySymbol Assembly;
    public readonly string AssemblyName;
    public readonly bool IsEcsAssembly;

    public readonly ImmutableArray<AssemblyInfo> ReferencedAssemblies;
    public readonly ImmutableArray<Component> RegisteredComponents;
    public readonly ImmutableArray<Component> ImplicitComponents;
    public readonly ImmutableArray<SystemGroup> RegisteredSystemGroups;
    public readonly INamedTypeSymbol EntityType;
    public readonly INamedTypeSymbol WorldType;
    public readonly INamedTypeSymbol ApiType;

    public readonly ImmutableArray<Component> Components;
    public readonly ImmutableArray<SystemGroup> SystemGroups;
    public readonly ImmutableArray<System> SystemCalls;

    public AssemblyInfo(Generation generation, IAssemblySymbol assembly, ImmutableArray<InvocationCandidate> invocationCandidates = default) {
        Generation = generation;
        Assembly = assembly;
        AssemblyName = assembly.Name;
        IsEcsAssembly = assembly.IsEcsAssembly(generation);
        ReferencedAssemblies = GetReferencedAssemblies(generation, assembly);
        RegisteredComponents = GetRegisteredComponents(generation, assembly);
        ImplicitComponents = GetImplicitComponents(invocationCandidates);
        Components = RegisteredComponents.AddRange(ImplicitComponents).Distinct(Component.Comparer).ToImmutableArray().Sort((a, b) => a.Order.CompareTo(b.Order));
        RegisteredSystemGroups = GetRegisteredSystemGroups(generation, assembly, Components);
        SystemGroups = RegisteredSystemGroups.AddRange(ReferencedAssemblies.SelectMany(a => a.SystemGroups)).Distinct(SystemGroup.Comparer).ToImmutableArray();
        SystemCalls = RegisteredSystemGroups.SelectMany(g => g.Systems).ToImmutableArray();
        ReferencedAssemblies = ReferencedAssemblies.Where(a => a.Components.Length > 0 || a.SystemGroups.Length > 0).ToImmutableArray();

        EntityType = FindType("Entity");
        WorldType = FindType("World");
        ApiType = FindType("Api");
    }

    public void Init() {
        InitComponents();
        InitSystems();
    }

    private void InitComponents() {
        for (ushort i = 1; i <= Components.Length; i++) {
            Components[i - 1].Init(Generation, i);
        }
    }

    private void InitSystems() {
        foreach (var system in SystemCalls) {
            system.Method.GetWithAttributeValues(Generation.Compilation, out var withTypes);
            system.Method.GetWithoutAttributeValues(Generation.Compilation, out var withoutTypes);
            system.Init(Generation, withTypes, withoutTypes);
        }
    }

    private ImmutableArray<Component> GetRegisteredComponents(Generation generation, IAssemblySymbol assembly) {
        var thisAssemblyComponents = assembly.GetAttributes()
            .Select(a => a.IsRegisterComponent(generation, out var component) ? component : null)
            .Where(t => t != null)
            .ToImmutableArray();

        return thisAssemblyComponents.AddRange(ReferencedAssemblies.SelectMany(a => a.RegisteredComponents)).Distinct(Component.Comparer).ToImmutableArray();
    }

    private ImmutableArray<Component> GetImplicitComponents(ImmutableArray<InvocationCandidate> invocationCandidates) {
        if (invocationCandidates == default)
            return ImmutableArray<Component>.Empty;

        var thisAssemblyComponents = invocationCandidates
            .SelectMany(ic => ic.Args.Select(a => a.Type))
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)
            .Where(ct => RegisteredComponents.All(c => !c.Type.Equals(ct, SymbolEqualityComparer.Default)))
            .Select(ct => new Component(ct, int.MaxValue, null))
            .ToImmutableArray();

        return thisAssemblyComponents.AddRange(ReferencedAssemblies.SelectMany(a => a.ImplicitComponents)).Distinct(Component.Comparer).ToImmutableArray();
    }

    private static ImmutableArray<SystemGroup> GetRegisteredSystemGroups(Generation generation, IAssemblySymbol assembly, ImmutableArray<Component> components)
        => assembly.GetAttributes()
            .Select(a => a.IsRegisteredSystemGroup(generation, components, out var systemGroup) ? systemGroup : null)
            .Where(t => t != null)
            .ToImmutableArray();

    private static ImmutableArray<AssemblyInfo> GetReferencedAssemblies(Generation generation, IAssemblySymbol assembly) {
        if (!assembly.Equals(generation.Compilation.Assembly, SymbolEqualityComparer.Default))
            return ImmutableArray<AssemblyInfo>.Empty;

        return generation.Compilation.References.Select(generation.Compilation.GetAssemblyOrModuleSymbol)
            .OfType<IAssemblySymbol>()
            .Select(a => new AssemblyInfo(generation, a))
            .ToImmutableArray();
    }

    private INamedTypeSymbol FindType(string interfaceName)
        => Assembly.GetTypeByMetadataName(Assembly.Name + "." + interfaceName);

    public static void GenerateCommon(Generation generation) {
    }

    public static void GenerateEcs(Generation generation) {
        UtilsGenerator.Generate(generation);
        EntityGenerator.GenerateEcsEntity(generation);
        WorldGenerator.GenerateEcsWorld(generation);
        // ApiGenerator.GenerateEcsApi(generation);
    }

    public void GeneratePlugin(Generation generation) {
        EntityGenerator.GeneratePluginEntity(generation);
        WorldGenerator.GeneratePluginWorld(generation);
        ApiGenerator.GeneratePluginApi(generation);
    }
}
