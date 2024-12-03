using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

public class GeneratorInfo {
    public readonly SourceProductionContext Context;
    public readonly Compilation Compilation;

    public readonly ImmutableArray<SystemGroup> RegisteredSystems;
    public readonly ImmutableArray<Component> RegisteredComponents;
    public readonly ImmutableArray<UserApiCall> FoundApiCalls;

    public readonly ImmutableArray<Component> AllComponents;

    public GeneratorInfo(SourceProductionContext context, Compilation compilation, ImmutableArray<SystemGroup> systems, ImmutableArray<Component> components, ImmutableArray<UserApiCall> apiCalls) {
        Context = context;
        Compilation = compilation;
        RegisteredSystems = systems;
        RegisteredComponents = PrepareComponents(compilation, components);
        FoundApiCalls = apiCalls;

        var componentTypesUsedInSystems = ExtractComponentsFromSystems(compilation, systems);
        var componentTypesUsedInApiCalls = ExtractComponentsFromSystems(compilation, systems);

        AllComponents = RegisteredComponents;
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
