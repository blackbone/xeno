using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace Xeno.SourceGenerator;

internal static class Ensure
{
    public static void Type(Compilation compilation, TypeSyntax typeSyntax, out INamedTypeSymbol type) {
        type = compilation.GetSemanticModel(typeSyntax.SyntaxTree).GetSymbolInfo(typeSyntax).Symbol as INamedTypeSymbol;
        if (type == null) throw new TypeLoadException(typeSyntax.ToFullString());
    }

    public static void Type(Compilation compilation, string typeFullName, out INamedTypeSymbol type)
    {
        type = compilation.GetTypeByMetadataName(typeFullName);
        if (type == null) throw new TypeLoadException(typeFullName);
    }

    public static void EcsAssemblyAttribute(Compilation compilation, out INamedTypeSymbol type)
        => Type(compilation, "Xeno.EcsAssemblyAttribute", out type);

    public static void RegisterComponentAttribute(Compilation compilation, out INamedTypeSymbol type)
        => Type(compilation, "Xeno.RegisterComponentAttribute", out type);

    public static void RegisterSystemGroupAttribute(Compilation compilation, out INamedTypeSymbol type)
        => Type(compilation, "Xeno.RegisterSystemGroupAttribute", out type);

    public static bool IsEcsAssembly(Compilation compilation) {
        return false;
    }
}
