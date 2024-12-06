using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal static class ArchetypeGenerator {
    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation)) return;

        GenerateArchetype(info);
        GenerateArchetypes(info);
    }

    private static void GenerateArchetype(GeneratorInfo info) {
        var root = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System")))
            .AddUsings(UsingDirective(ParseName("System.Runtime.InteropServices")))
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration("Archetype")
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
                    .WithMembers(List(GetMembers()))));

        info.Context.Add("Xeno/Archetype.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PrivateReadOnlyField("World", "world");
            yield return Helpers.PrivateReadOnlyField("bool", "floating")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.InternalField("SetReadOnly", "mask");
            yield return Helpers.InternalField("uint[]", "entities");
            yield return Helpers.InternalField("uint", "entitiesCount");
            yield return Helpers.InternalField("Archetype", "prev");
            yield return Helpers.InternalField("Archetype", "next")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicConstructor("Archetype", "in bool floating, in World world")
                .WithBody(Block(
                    ParseStatement("this.world = world;"),
                    ParseStatement("this.floating = floating;"),
                    ParseStatement("entities = new uint[128];"),
                    ParseStatement("entitiesCount = 0;")
                ));

            yield return Helpers.PublicVoidMethod("Clear")
                .WithBody(Block(
                    ParseStatement("entitiesCount = 0;"),
                    ParseStatement("Array.Clear(entities, 0, entities.Length);"),
                    ParseStatement("prev = null;"),
                    ParseStatement("next = null;")
                    ));
        }
    }

    private static void GenerateArchetypes(GeneratorInfo info) {
        var root = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System.Runtime.CompilerServices")))
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration("Archetypes")
                    .WithModifiers(TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.SealedKeyword)))
                    .WithMembers(List(GetMembers()))));

        info.Context.Add("Xeno/Archetypes.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PrivateReadOnlyField("World", "world");
            yield return Helpers.PrivateField("Archetype[]", "freeArchetypes");
            yield return Helpers.PrivateField("uint", "freeArchetypesCount");
            yield return Helpers.PrivateField("Archetype", "head")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PublicConstructor("Archetypes", "in World world")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("this.world = world;"),
                    ParseStatement("freeArchetypes = new Archetype[32];"),
                    ParseStatement("freeArchetypesCount = 32;"),
                    ParseStatement("for (var i = 0; i < freeArchetypesCount; i++)"),
                    ParseStatement("freeArchetypes[i] = new Archetype(true, world);"),
                    ParseStatement("head = null;")
                ));

            yield return Helpers.PublicMethod("Archetype", "AddPermanent", "ref SetReadOnly mask")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(@"
var node = new Archetype(false, world) {
    mask = mask,
    next = head
};

if (head != null) head.prev = node;
head = node;

return node;".Split("\n").Select(s => ParseStatement(s))
                    ));
        }
    }
}
