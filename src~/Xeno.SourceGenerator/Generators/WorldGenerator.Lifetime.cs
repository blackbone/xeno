using Microsoft.CodeAnalysis.CSharp;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxHelpers;

internal static partial class WorldGenerator {
    private static void GenerateLifetime(Generation generation) {
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
            .Fields(
                Field("bool", "isDisposed", "false", SyntaxKind.PrivateKeyword)
            )
            .Methods(
                // dispose method, fill body
                Method("void", "Dispose", null, SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body("isDisposed = true;"),
                Method("bool", "IsDisposed", null, SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body("return isDisposed;")
            );

        var root = SyntaxFactory.CompilationUnit()
            .Usings("System.Runtime.CompilerServices")
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "World.Lifetime");
    }
}
