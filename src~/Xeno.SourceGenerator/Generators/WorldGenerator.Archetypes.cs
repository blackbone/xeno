using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator;
using Xeno.SourceGenerator.Utils;

namespace Xeno.Generators;

using static SyntaxHelpers;
using static SyntaxFactory;

internal static partial class WorldGenerator {
    private static void GenerateArchetypes(Generation generation) {
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
                .Fields(
                    Field("Archetypes", "archetypes", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                    Field("Archetype[]", "entityArchetypes", null, SyntaxKind.PrivateKeyword),
                    Field("uint[]", "inArchetypeLocalIndices", null, SyntaxKind.PrivateKeyword)
                        .Line()
                    )
                .Fields(DirectlyUsedArchetypeFields(generation).ToArray())
                .Fields(
                    Field("Archetype", "zeroArchetype", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
                        .Line()
                )
                .Inner(
                    ArchetypeClass,
                    ArchetypesClass
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

        generation.Add(root, "World.Archetypes");
    }

    private static IEnumerable<string> InitializeArchetypesStatements(Generation generation) {
        yield return "archetypes = new Archetypes(this, capacity);";
        yield return "zeroArchetype = archetypes.AddPermanent(SetReadOnly.Zero);";
        foreach (var statement in GetInitializeStatements(generation))
            yield return statement;
        yield return "entityArchetypes = new Archetype[capacity];";
        yield return "inArchetypeLocalIndices = new uint[capacity];";
    }

    private static readonly ClassDeclarationSyntax ArchetypeClass =
        Class("Archetype", SyntaxKind.PrivateKeyword, SyntaxKind.SealedKeyword)
            .Attributes(StructLayoutSequential)
            .Fields(
                Field("World", "world", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("bool", "floating", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
                    .Line(),
                Field("SetReadOnly", "mask", null, SyntaxKind.InternalKeyword),
                Field("uint[]", "entities", null, SyntaxKind.InternalKeyword),
                Field("uint", "entitiesCount", null, SyntaxKind.InternalKeyword),
                Field("Archetype", "prev", null, SyntaxKind.InternalKeyword),
                Field("Archetype", "next", null, SyntaxKind.InternalKeyword)
                    .Line()
                )
            .Constructors(
                Constructor("Archetype",  "in bool floating, in World world, in uint capacity", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "this.world = world;",
                        "this.floating = floating;",
                        "entities = new uint[capacity];",
                        "entitiesCount = 0;"
                        )
                )
            .Methods(
                Method("void", "Clear", null, SyntaxKind.PublicKeyword)
                .Attributes(AggressiveInlining)
                .Body(
                    "entitiesCount = 0;",
                    "Array.Clear(entities, 0, entities.Length);",
                    "prev = null;",
                    "next = null;"
                    )
                );


    private static readonly ClassDeclarationSyntax ArchetypesClass =
        Class("Archetypes", SyntaxKind.PrivateKeyword, SyntaxKind.SealedKeyword)
            .Attributes(StructLayoutSequential)
            .Fields(
                Field("uint", "capacity", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword),
                Field("World", "world", null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword)
                    .Line(),
                Field("Archetype[]", "freeArchetypes", null, SyntaxKind.PrivateKeyword),
                Field("uint", "freeArchetypesCount", null, SyntaxKind.PrivateKeyword),
                Field("Archetype", "head", null, SyntaxKind.PrivateKeyword)
                    .Line()
                )
            .Constructors(
                Constructor("Archetypes", "in World world, in uint capacity", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "this.world = world;",
                        "this.capacity = capacity;",
                        "freeArchetypes = new Archetype[16];",
                        "freeArchetypesCount = 16;",
                        "for (var i = 0; i < freeArchetypesCount; i++)",
                        "freeArchetypes[i] = new Archetype(true, world, capacity);",
                        "head = null;"
                        )
                )
            .Methods(
                Method("Archetype", "AddPermanent", "in SetReadOnly mask", SyntaxKind.PublicKeyword)
                    .Attributes(AggressiveInlining)
                    .Body(
                        "var node = new Archetype(false, world, capacity) { mask = mask, next = head };",
                        "if (head != null) head.prev = node;",
                        "head = node;",
                        "return node;"
                        )
                );

    private static IEnumerable<FieldDeclarationSyntax> DirectlyUsedArchetypeFields(Generation generation) {
        if (!generation.InvocationsByName.TryGetValue("Create", out var invocations))
            yield break;

        foreach (var mask in invocations.Select(i => i.Mask).Distinct(Mask.Comparer)) {
            yield return Field("Archetype", mask.GetArchetypeFieldName(), null, SyntaxKind.PrivateKeyword, SyntaxKind.ReadOnlyKeyword);
        }
    }

    private static IEnumerable<string> GetInitializeStatements(Generation generation) {
        if (!generation.InvocationsByName.TryGetValue("Create", out var invocations))
            yield break;

        foreach (var mask in invocations.Select(i => i.Mask).Distinct(Mask.Comparer)) {
            yield return $"{mask.GetArchetypeFieldName()} = archetypes.AddPermanent({mask.ReadOnlyInitializer});";
        }
    }
}
