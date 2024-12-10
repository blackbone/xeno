using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

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

        var info = new GeneratorInfo(context, compilation,
            systems.Where(v => v != null).ToImmutableArray(),
            components.Where(v => v != null).ToImmutableArray(),
            userApiCalls.Where(v => v != null).ToImmutableArray());

        try {
            UtilsGenerator.Generate(info);
            EntityGenerator.Generate(info);
            ArchetypeGenerator.Generate(info);
            FiltersGenerator.Generate(info);

            ComponentGenerator.Generate(info);
            StoresGenerator.Generate(info);
            WorldGenerator.Generate(info);

            UserApiGenerator.Generate(info);
        } catch (Exception e){
            context.AddSource("Xeno/_Log.txt", SourceText.From(e.Message + "\n" + e.StackTrace, Encoding.UTF8));
        }
    }
}
