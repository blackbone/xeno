using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Xeno.SourceGenerator;

[Generator]
public class Generator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        // must be registered
        var componentsProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate : ComponentCollector.Check,
            transform : ComponentCollector.Transform
        );

        // must be registered
        var systemsProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate : SystemCollector.Check,
            transform : SystemCollector.Transform
            );

        // must not be registeresd
        var userApiProvider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate : UserApiCollector.Check,
            transform : UserApiCollector.Transform
        );

        var combined = context.CompilationProvider
            .Combine(systemsProvider.Collect())
            .Combine(componentsProvider.Collect())
            .Combine(userApiProvider.Collect());

        context.RegisterSourceOutput(combined, Generate);
    }
    private static void Generate(SourceProductionContext context, (((Compilation, ImmutableArray<SystemGroup>), ImmutableArray<Component>), ImmutableArray<UserApiCall>) tuple) {
        var (((compilation, systems), components), userApiCalls) = tuple;

        if (!Ensure.IsEcsAssembly(compilation)) return;

        var info = new GeneratorInfo(context, compilation,
            systems.Where(v => v != null).ToImmutableArray(),
            components.Where(v => v != null).ToImmutableArray(),
            userApiCalls.Where(v => v != null).ToImmutableArray());

        EntityGenerator.Generate(info);
        ArchetypeGenerator.Generate(info);
        FiltersGenerator.Generate(info);

        ComponentGenerator.Generate(info);
        StoresGenerator.Generate(info);
        WorldGenerator.Generate(info);
    }
}
