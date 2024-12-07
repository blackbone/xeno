using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xeno.SourceGenerator.SyntaxReceivers;

namespace Xeno.SourceGenerator;

internal class GeneratorInfo {
    public readonly SourceProductionContext Context;
    public readonly Compilation Compilation;

    public readonly ImmutableArray<SystemGroup> RegisteredSystemGroups;
    public readonly ImmutableArray<Component> RegisteredComponents;

    public readonly ImmutableDictionary<SystemMethodType, ImmutableArray<System>> SystemInvocations;

    public readonly ImmutableArray<UserApiCall> UserApiCalls;


    public GeneratorInfo(
        SourceProductionContext context,
        Compilation compilation,
        ImmutableArray<SystemGroup> systemGroups,
        ImmutableArray<Component> components,
        ImmutableArray<UserApiCall> apiCalls)
    {
        Context = context;
        Compilation = compilation;

        // declared
        RegisteredSystemGroups = systemGroups;
        RegisteredComponents = PrepareComponents(compilation, components);
        UserApiCalls = apiCalls;

        // computed
        SystemInvocations = ComputeSystemInvocations();

        var componentTypesUsedInSystems = ExtractComponentsFromSystems(compilation, systemGroups);
        var componentTypesUsedInApiCalls = ExtractComponentsFromSystems(compilation, systemGroups);

    }

    private ImmutableDictionary<SystemMethodType,ImmutableArray<System>> ComputeSystemInvocations() {
        return RegisteredSystemGroups
            .SelectMany(s => s.Systems)
            .GroupBy(s => s.Type)
            .ToImmutableDictionary(g => g.Key, g => g.OrderBy(sm => sm.Order).ToImmutableArray());
    }

    private static ImmutableArray<Component> PrepareComponents(Compilation compilation, ImmutableArray<Component> components) {
        components = components.OrderBy(c => c.Priority ?? int.MaxValue).ToImmutableArray();
        for (var i = 0; i < components.Length; i++) {
            components[i].Index = i;
            components[i].PersistentId = components[i].Type.GetPersistentHashCode(compilation);
        }

        return components;
    }

    private static IEnumerable<ITypeSymbol> ExtractComponentsFromSystems(Compilation compilation, ImmutableArray<SystemGroup> systems) {
        return systems.SelectMany(s => s.Systems)
            .SelectMany(s => s.Parameters)
            .Where(p => !p.IsValidEntityParameter() && !p.IsValidUniformParameter(compilation))
            .Select(p => p.Type);
    }

    private static IEnumerable<ITypeSymbol> ExtractComponentsFromCalls(Compilation compilation, ImmutableArray<UserApiCall> userApiCalls) {
        // return userApiCalls.SelectMany(c => c.MethodSyntax.Parameters)
        //     .Where(p => !p.IsValidEntityParameter(compilation) && !p.IsValidUniformParameter(compilation))
        //     .Select(p => p.Type);
        yield break;
    }
}
