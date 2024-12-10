using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Xeno.SourceGenerator;

internal static class Ensure
{
    public static bool Type(Compilation compilation, TypeSyntax typeSyntax, out INamedTypeSymbol type) {
        type = compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol;
        return type != null;
    }

    public static bool Type(Compilation compilation, string typeFullName, out INamedTypeSymbol type)
    {
        type = compilation.GetTypeByMetadataName(typeFullName);
        return type != null;
    }

    public static bool IsEcsAssembly(Compilation compilation) {
        Type(compilation, "Xeno.EcsAssemblyAttribute", out var ecsAssemblyAttribute);
        return compilation.Assembly.GetAttributes().Any(a => a.AttributeClass.Equals(ecsAssemblyAttribute, SymbolEqualityComparer.Default));
    }

}
