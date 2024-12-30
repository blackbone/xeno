using Microsoft.CodeAnalysis.CSharp;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxFactory;
using static SyntaxHelpers;

internal static class EntityGenerator {

    public static void GenerateEcsEntity(Generation generation) {
        var structSyntax = Struct("Entity", SyntaxKind.PublicKeyword, SyntaxKind.PartialKeyword)
            .Attributes(StructLayoutSequential)
            .Fields(
                Field("uint", "Id", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("uint", "Version", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("World", "World", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword)
            );

        var root = CompilationUnit()
            .Usings("System.Runtime.InteropServices")
            .AddMembers(Namespace(generation.AssemblyName).AddMembers(structSyntax));

        generation.Add(root, "Entity");
    }

    public static void GeneratePluginEntity(Generation generation) {
        var structSyntax = Struct("Entity", SyntaxKind.PublicKeyword)
            .Attributes(StructLayoutSequential)
            .Fields(
                Field("uint", "Id", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("uint", "Version", null, SyntaxKind.PublicKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("World", "World", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
            );

        var root = CompilationUnit()
            .Usings("System.Runtime.InteropServices")
            .AddMembers(Namespace(generation.AssemblyName).AddMembers(structSyntax));

        generation.Add(root, "Entity");
    }
}
