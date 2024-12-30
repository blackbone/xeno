using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

internal class GeneratorInfo {
    public readonly SourceProductionContext Context;
    public readonly Compilation Compilation;
    public readonly ImmutableArray<SystemGroup> RegisteredSystemGroups;
    public readonly ImmutableArray<Component> RegisteredComponents;
    public readonly ImmutableDictionary<SystemType, ImmutableArray<System>> SystemInvocations;

    public GeneratorInfo(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<SystemGroup> systemGroups,
        ImmutableArray<Component> components)
    {
        Context = context;
        Compilation = compilation;

        // declared
        RegisteredSystemGroups = systemGroups;
        RegisteredComponents = PrepareComponents(compilation, components);

        // computed
        SystemInvocations = ComputeSystemInvocations();
        PrepareSystems();
    }

    private ImmutableDictionary<SystemType, ImmutableArray<System>> ComputeSystemInvocations() {
        return RegisteredSystemGroups
            .SelectMany(s => s.Systems)
            .GroupBy(s => s.Type)
            .ToImmutableDictionary(g => g.Key, g => g.OrderBy(sm => sm.Order).ToImmutableArray());
    }

    private static ImmutableArray<Component> PrepareComponents(Compilation compilation, ImmutableArray<Component> components) {
        components = components.OrderBy(c => c.Order).ToImmutableArray();
        // for (var i = 0; i < components.Length; i++) {
        //     components[i].Index = i;
        //     components[i].PersistentId = components[i].Type.GetPersistentHashCode(compilation);
        // }

        return components;
    }

    private static IEnumerable<ITypeSymbol> ExtractComponentsFromSystems(Compilation compilation, ImmutableArray<SystemGroup> systems) {
        return systems.SelectMany(s => s.Systems)
            .SelectMany(s => s.Parameters)
            .Where(p => !p.IsValidEntityParameter() && !p.IsValidUniformParameter(compilation, out _, out _))
            .Select(p => p.Type);
    }

    private void PrepareSystems() {
        var filters = new HashSet<Filter>(EqualityComparer<Filter>.Default);
        // foreach (var system in SystemInvocations.Values.SelectMany(s => s)) {
        //     system.Method.GetWithAttributeValues(Compilation, out var withTypes);
        //     system.Method.GetWithoutAttributeValues(Compilation, out var withoutTypes);
        //     system.InitFilter(this, withTypes, withoutTypes);
        //     filters.Add(system.Filter);
        // }
    }
}
