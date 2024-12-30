using System;
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
    private static void GenerateEntities(Generation generation) {
        var classSyntax = Class("World", SyntaxKind.PublicKeyword, SyntaxKind.SealedKeyword, SyntaxKind.PartialKeyword)
                .Fields(
                    Field("uint", "AllocatedMask", "0b10000000_00000000_00000000_00000000U", SyntaxKind.PrivateKeyword, SyntaxKind.ConstKeyword),
                    Field("uint", "NonAllocatedMask", "~AllocatedMask", SyntaxKind.PrivateKeyword, SyntaxKind.ConstKeyword)
                        .Line(),
                    Field("uint", "entityCount", null, SyntaxKind.PrivateKeyword),
                    Field("RWEntity[]", "entities", null, SyntaxKind.PrivateKeyword),
                    Field("uint", "freeIdsCount", null, SyntaxKind.PrivateKeyword),
                    Field("uint[]", "freeIds", null, SyntaxKind.PrivateKeyword)
                        .Line(),
                    Field("uint", "__size", "0", SyntaxKind.PrivateKeyword)
                        .Line()
                )
                .Methods(
                    InitializeEntitiesMethod,
                    GrowEntitiesCapacityMethod,
                    DisposeEntitiesMethod,

                    // entityMethods
                    CreateMethod(generation),
                    IsValidEntityMethod,
                    DestroyEntityMethod
                )
                .Methods(GetDynamicInvocationMethods(generation).ToArray())
                .Inner(RWEntityStruct)
                .AddMembers()
            ;

        var root = CompilationUnit()
            .Usings(
                "System",
                "System.Runtime.CompilerServices")
            .AddMembers(Namespace(generation.AssemblyName)
                .AddMembers(classSyntax));

        generation.Add(root, "World.Entities");
    }

    private static IEnumerable<MethodDeclarationSyntax> GetDynamicInvocationMethods(Generation generation) {
        foreach (var invocation in generation.Invocations) {
            if (!invocation.Target.Name.Equals("World")) // rly dumb check but workds
                continue;

            if (generation.AssemblyInfo.IsEcsAssembly)
                switch (invocation.Name) {
                    case "Create": yield return CreateMethod(generation, invocation); break;
                    case "Add": yield return AddMethod(generation, invocation); break;
                }
            else
                yield return GenerateWorldExtension(generation, invocation);
        }
    }


    private static MethodDeclarationSyntax GenerateWorldExtension(Generation generation, Invocation invocation) {
        generation.Log($"generating dynamic invocation: {invocation.Name}");
        MethodDeclarationSyntax method = null;
        switch (invocation.Name) {
            // case "Create":
            //     method = Method("Entity", "Create", AsComponentInList(generation, invocation.Args))
            //         .Attributes(AggressiveInlining);
            //     break;
            // case "Add":
            //     method = Method("Entity", "Add", $"in Entity entity {AsComponentInList(generation, invocation.Args)}")
            //         .Attributes(AggressiveInlining);
            //     break;
            // case "Remove":
            //     method = Method("Entity", "Remove", $"in Entity entity {AsComponentOutList(generation, invocation.Args)}")
            //         .Attributes(AggressiveInlining);
            //     break;
            default:
                throw new IndexOutOfRangeException();
        }

        return method.Attributes(AggressiveInlining);
    }

    private static MethodDeclarationSyntax GenerateWorldInvocation(Generation generation, Invocation invocation) {
        MethodDeclarationSyntax method;
        switch (invocation.Name) {
            // case "Create":
            //     method = Method("Entity", "Create", AsComponentInList(generation, invocation.Args))
            //         .Body(Array.Empty<string>());
            //     break;
            // case "Add":
            //     method = Method("Entity", "Add", $"in Entity entity {AsComponentInList(generation, invocation.Args)}")
            //         .Body(Array.Empty<string>());
            //     break;
            // case "Remove":
            //     method = Method("Entity", "Remove", $"in Entity entity {AsComponentOutList(generation, invocation.Args)}")
            //         .Body(Array.Empty<string>());
            //     break;
            default:
                throw new IndexOutOfRangeException();
        }

        generation.Log(method.ToFullString());
        return method.Attributes(AggressiveInlining).AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    private static string AsComponentInList(Generation generation, IEnumerable<Component> args) {
        generation.Log(string.Join(", ", args));
        return string.Join(", ", args.Select(a => $"in {a.TypeFullName} {a.ArgName}"));
    }

    private static string AsComponentOutList(Generation generation, IEnumerable<Component> args) {
        generation.Log(string.Join(", ", args));
        return string.Join(", ", args.Select(a => $"out {a.TypeFullName} {a.ArgName}"));
    }

    private static readonly MethodDeclarationSyntax InitializeEntitiesMethod =
        Method("void", "InitializeEntities", "in uint capacity", SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .WithBody(Block(
                ParseStatement("entityCount = 0;"),
                ParseStatement("entities = new RWEntity[capacity];"),
                ParseStatement("freeIdsCount = 0;"),
                ParseStatement("freeIds = Array.Empty<uint>();"),

                ParseStatement("// initializing new entities"),
                ParseStatement("var span_entities = entities.AsSpan((int)entityCount, (int)capacity);"),
                ParseStatement("var size = freeIds.Length == 0 ? 1 : freeIds.Length;"),
                ParseStatement("while (size < freeIdsCount + capacity) size <<= 1;"),
                ParseStatement("Utils.Resize(ref freeIds, (uint)size);"),
                ParseStatement("var span_freeIds_c = freeIds.Length - (int)freeIdsCount;"),
                ParseStatement("var span_freeIds = freeIds.AsSpan((int)freeIdsCount, span_freeIds_c);"),
                ParseStatement("var freeIdsCount_int = (int)freeIdsCount;"),
                ParseStatement("for (var i = 0; i < span_entities.Length; i++)"),
                Block(
                    ParseStatement("ref var e = ref span_entities[i];"),
                    ParseStatement("var id = entityCount + (uint)i;"),
                    ParseStatement("e.Id = id;"),
                    ParseStatement("e.World = this;"),
                    ParseStatement("e.Version = 0;"),
                    ParseStatement("span_freeIds[--span_freeIds_c] = id;"),
                    ParseStatement("freeIdsCount_int++;")
                ),
                ParseStatement("freeIdsCount = (uint)freeIdsCount_int;")
            ));

    private static readonly MethodDeclarationSyntax GrowEntitiesCapacityMethod =
        Method("void", "GrowEntitiesCapacity", "in uint capacity", SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .WithBody(Block(
                ParseStatement("// entities"),
                ParseStatement("Utils.Resize(ref entities, capacity);"),

                ParseStatement("// initializing new entities"),
                ParseStatement("var count = capacity - entityCount;"),
                ParseStatement("var span_entities = entities.AsSpan((int)entityCount, (int)count);"),
                ParseStatement("var size = freeIds.Length == 0 ? 1 : freeIds.Length;"),
                ParseStatement("while (size < freeIdsCount + count) size <<= 1;"),
                ParseStatement("Utils.Resize(ref freeIds, (uint)size);"),
                ParseStatement("var span_freeIds_c = freeIds.Length - (int)freeIdsCount;"),
                ParseStatement("var span_freeIds = freeIds.AsSpan((int)freeIdsCount, span_freeIds_c);"),
                ParseStatement("var freeIdsCount_int = (int)freeIdsCount;"),
                ParseStatement("for (var i = 0; i < span_entities.Length; i++)"),
                Block(
                    ParseStatement("ref var e = ref span_entities[i];"),
                    ParseStatement("var id = entityCount + (uint)i;"),
                    ParseStatement("e.Id = id;"),
                    ParseStatement("e.World = this;"),
                    ParseStatement("e.Version = 0;"),
                    ParseStatement("span_freeIds[--span_freeIds_c] = id;"),
                    ParseStatement("freeIdsCount_int++;")
                ),
                ParseStatement("freeIdsCount = (uint)freeIdsCount_int;")
            ));

    private static readonly MethodDeclarationSyntax DisposeEntitiesMethod =
        Method("void", "DisposeEntities", null, SyntaxKind.PrivateKeyword)
            .Attributes(AggressiveInlining)
            .Body(
                "entityCount = 0;",
                "entities = default;",
                "freeIdsCount = 0;",
                "freeIds = default;"
                );

    private static readonly MethodDeclarationSyntax IsValidEntityMethod
        = Method("bool", "IsValid", "in Entity entity", SyntaxKind.PublicKeyword)
            .Attributes(AggressiveInlining)
            .Body(DisposeCheck)
            .ExtendBody("return entity.Id < entities.Length && entities[entity.Id].Version == entity.Version;");

    private static readonly MethodDeclarationSyntax DestroyEntityMethod
        = Method("bool", "Delete", "ref Entity entity", SyntaxKind.PublicKeyword)
            .Attributes(AggressiveInlining)
            .Body(DisposeCheck)
            .ExtendBody(
                "if (!IsValid(entity)) return false;",
                "ref var e = ref entities[entity.Id];",
                "e.Version &= NonAllocatedMask;",
                "e.Version++;",
                "entityCount--;",
                "if (freeIdsCount == freeIds.Length)",
                "Utils.Resize(ref freeIds, freeIdsCount << 1);",
                "freeIds[freeIdsCount++] = e.Id;"
                )
            // TODO add clean up reference components from stores
            .ExtendBody("return true;");

    private static readonly StructDeclarationSyntax RWEntityStruct =
        Struct("RWEntity", SyntaxKind.PrivateKeyword)
            .Fields(
                Field("uint", "Id", null, SyntaxKind.PublicKeyword),
                Field("uint", "Version", null, SyntaxKind.PublicKeyword),
                Field("World", "World", null, SyntaxKind.PublicKeyword)
            );

    private static MethodDeclarationSyntax CreateMethod(Generation generation, Invocation invocation = null) {
        var method = Method("Entity", "Create", invocation != null
                ? AsComponentInList(generation, invocation.Components.Select(a => a.Item2))
                : null,
            SyntaxKind.PublicKeyword);

        var archetype = invocation?.Mask.GetArchetypeFieldName() ?? "zeroArchetype";
        method = method
            .AddAttributeLists(Helpers.AggressiveInlining)
            .Body(DisposeCheck)
            .ExtendBody(
                "if (entityCount == entities.Length)",
                "GrowCapacity(entityCount << 1);",
                "var e_id = entityCount;",
                "if (freeIdsCount > 0)")
            .ExtendBody(Block(
                "freeIdsCount--;",
                "e_id = freeIds[freeIdsCount];"
            ))
            .ExtendBody(
                "entities[e_id].Version |= AllocatedMask;",
                "var entity = Unsafe.As<RWEntity, Entity>(ref entities[e_id]);",
                "entityCount++;",
                "// add to archetype",
                $"if ({archetype}.entitiesCount == {archetype}.entities.Length)",
                $"Utils.Resize(ref {archetype}.entities, (uint){archetype}.entities.Length << 1);",
                $"{archetype}.entities[{archetype}.entitiesCount] = entity.Id;",
                $"inArchetypeLocalIndices[entity.Id] = {archetype}.entitiesCount;",
                $"{archetype}.entitiesCount++;",
                $"entityArchetypes[entity.Id] = {archetype};"
            );

        if (invocation != null) {
            method = method.ExtendBody(invocation.Mask.ComponentArgs.SelectMany(c => c.GetAddStatements("e_id")).ToArray());
        }

        return method.ExtendBody("return entity;");
    }


    private static MethodDeclarationSyntax AddMethod(Generation generation, Invocation invocation) {
        var method = Method("Entity", "Add", AsComponentInList(generation, invocation.Components.Select(a => a.Item2)), SyntaxKind.PublicKeyword);

        var archetype = invocation.Mask.GetArchetypeFieldName();
        method = method
            .AddAttributeLists(Helpers.AggressiveInlining)
            .Body(DisposeCheck)
            .ExtendBody(
                "if (entityCount == entities.Length)",
                "GrowCapacity(entityCount << 1);",
                "var e_id = entityCount;",
                "if (freeIdsCount > 0)")
            .ExtendBody(Block(
                "freeIdsCount--;",
                "e_id = freeIds[freeIdsCount];"
            ))
            .ExtendBody(
                "entities[e_id].Version |= AllocatedMask;",
                "var entity = Unsafe.As<RWEntity, Entity>(ref entities[e_id]);",
                "entityCount++;",
                "// add to archetype",
                $"if ({archetype}.entitiesCount == {archetype}.entities.Length)",
                $"Utils.Resize(ref {archetype}.entities, (uint){archetype}.entities.Length << 1);",
                $"{archetype}.entities[{archetype}.entitiesCount] = entity.Id;",
                $"inArchetypeLocalIndices[entity.Id] = {archetype}.entitiesCount;",
                $"{archetype}.entitiesCount++;",
                $"entityArchetypes[entity.Id] = {archetype};"
            );

        if (invocation != null) {
            method = method.ExtendBody(invocation.Mask.ComponentArgs.SelectMany(c => c.GetAddStatements("e_id")).ToArray());
        }

        return method.ExtendBody("return entity;");
    }
}
