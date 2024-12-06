using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator
{
    [Generator]
    public partial class SystemSourceGenerator : ISourceGenerator
    {
        internal static INamedTypeSymbol componentInterfaceType;
        internal static INamedTypeSymbol entityType;
        internal static INamedTypeSymbol systemAttributeType;
        internal static INamedTypeSymbol systemMethodAttributeType;
        internal static INamedTypeSymbol uniformAttributeType;

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each compilation
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Retrieve the populated instance of the syntax receiver
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;

            // Retrieve the compilation that represents the entire solution
            var compilation = context.Compilation;

            // Find the type symbol for Xeno.UpdateSystem
            if (!Ensure.Type(compilation, "Xeno.Entity", out entityType)) return;
            if (!Ensure.Type(compilation, "Xeno.IComponent", out componentInterfaceType)) return;
            if (!Ensure.Type(compilation, "Xeno.SystemAttribute", out systemAttributeType)) return;
            if (!Ensure.Type(compilation, "Xeno.SystemMethodAttribute", out systemMethodAttributeType)) return;
            if (!Ensure.Type(compilation, "Xeno.UniformAttribute", out uniformAttributeType)) return;

            foreach (var classSyntax in receiver.CandidateClasses)
            {
                var classSemanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
                var classSymbol = classSemanticModel.GetDeclaredSymbol(classSyntax) as INamedTypeSymbol;
                if (classSymbol == null) continue;
                
                var classAttributes = classSymbol.GetAttributes();
                if (classAttributes.Length == 0) continue;
                
                if (classAttributes.All(a => !(a.AttributeClass?.Equals(systemAttributeType, SymbolEqualityComparer.Default) ?? false)))
                    continue;

                var systemMethods = new Dictionary<int, List<(IMethodSymbol method, int order)>>();
                foreach (var member in classSymbol.GetMembers())
                {
                    if (member is not IMethodSymbol method) continue;

                    var attributes = method.GetAttributes();
                    if (attributes.Length == 0) continue;
                    
                    var systemMethodAttribute = attributes.FirstOrDefault(attribute => attribute.AttributeClass?.Equals(systemMethodAttributeType, SymbolEqualityComparer.Default) ?? false);
                    if (systemMethodAttribute == null) continue;

                    if (systemMethodAttribute.ConstructorArguments.Length == 0) continue;
                    if (systemMethodAttribute.ConstructorArguments[0].Value == null) continue;
                    if (systemMethodAttribute.ConstructorArguments[1].Value == null) continue;
                    
                    var invocationGroup = (int)systemMethodAttribute.ConstructorArguments[0].Value;
                    if (!systemMethods.TryGetValue(invocationGroup, out var list))
                        list = systemMethods[invocationGroup] = new List<(IMethodSymbol, int)>();
                    
                    var order = (int)systemMethodAttribute.ConstructorArguments[1].Value;
                    list.Add((method, order));
                }
                
                var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
                var genericParameters = classSymbol.TypeParameters.Select(tp => tp.Name).ToArray();

                // Generate the partial class implementation
                var sourceText = SystemTemplate(namespaceName, classSymbol.Name, genericParameters, systemMethods);
                
                // Add the generated source to the compilation
                context.AddSource($"{classSymbol.Name}.g.cs", sourceText);
            }
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // Add class declarations with a base type to the candidates list
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Any(a => a.Attributes.Any(a2 => a2.Name.ToString() == "System")))
                    CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}
