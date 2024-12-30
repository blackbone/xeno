using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Xeno.SourceGenerator.Collectors;

namespace Xeno.SourceGenerator;

[Generator]
public class Generator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var invocationCandidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: InvocationCollector.Filter,
                transform: InvocationCollector.Transform
            )
            .Where(data => data != null)
            .Collect();

        // Combine invocation data with the compilation
        var combined = context.CompilationProvider.Combine(invocationCandidates);

        // Pass combined data to the Generation class
        context.RegisterSourceOutput(combined, (spc, combinedData) =>
        {
            var (compilation, invocations) = combinedData;

            Generation gen = null;
            try {
                // Generate source with collected semantics and compilation
                gen = new Generation(spc, compilation, invocations);
            } catch (Exception e) {
                spc.AddSource(
                    "Xeno/_exception.cs",
                    SourceText.From($"/*\n{e.Message}\n{e.StackTrace}\n*/", Encoding.UTF8));
            } finally {
                gen?.Dispose();
            }
        });
    }
}
