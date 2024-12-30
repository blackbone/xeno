using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator.Utils;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;
using static SyntaxHelpers;

internal static class UtilsGenerator {
    public static void Generate(Generation generation) {
        var classSyntax = Class("Utils", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
                .Attributes(Serializable)
                .Methods(
                    ResizeExtensionMethod
                )
            ;
        var root = CompilationUnit()
            .Usings(
                "System",
                "System.Runtime.CompilerServices",
                "System.Runtime.InteropServices"
            )
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "Utils");
    }

    private static readonly MethodDeclarationSyntax ResizeExtensionMethod =
        Method("void", "Resize<T>", "ref T[] array, in uint size", SyntaxKind.PublicKeyword, SyntaxKind.StaticKeyword)
            .Attributes(AggressiveInlining)
            .Body("var tmp = new T[size];",
                "Array.Copy(array, 0, tmp, 0, Math.Min(size, array.Length));",
                "array = tmp;");
}
