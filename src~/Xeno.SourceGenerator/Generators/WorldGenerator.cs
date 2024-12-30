using System;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxHelpers;
using static SyntaxFactory;

internal static partial class WorldGenerator {

    public static void GeneratePluginWorld(Generation generation) {
        // just placeholder type
        var classSyntax = Interface("World", SyntaxKind.PublicKeyword);
        var root = CompilationUnit().AddMembers(Namespace(generation.AssemblyName).AddMembers(classSyntax));
        generation.Add(root, "World");
    }

    public static void GenerateEcsWorld(Generation generation) {
        GenerateMain(generation);
        GenerateLifetime(generation);
        GenerateEntities(generation);
        GenerateArchetypes(generation);
        GenerateComponents(generation);
        GenerateFilters(generation);
        GenerateSystems(generation);
    }

    private static void GenerateMain(Generation generation) {
        // just placeholder type
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
            .Constructors(
                Constructor("World", "in uint capacity = 1024", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(InitializeArchetypesStatements(generation).ToArray())
            )
            .Methods(
                Method("(string, ushort)", "Describe", null, SyntaxKind.PublicKeyword)
                    .Body("return (\"\", 0);"),
                Method("void", "GrowCapacity", "in uint capacity", SyntaxKind.PrivateKeyword)
                    .Body(Array.Empty<string>())
                )
            ;

        var root = CompilationUnit()
            .Usings("System")
            .Usings("System.Runtime.CompilerServices")
            .AddMembers(Namespace(generation.AssemblyName).AddMembers(classSyntax));
        generation.Add(root, "World");
    }
}
