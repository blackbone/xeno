using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NotImplementedException = System.NotImplementedException;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

public static class WorldGenerator {
    public static StatementSyntax DisposeCheck { get; } = ParseStatement("Xeno.WorldDisposedException.ThrowIf(isDisposed, this.Describe());");

    public static void Generate(GeneratorInfo info) {
        GenerateBase(info);
        GenerateEntities(info);
        GenerateArchetypes(info);
        GenerateStores(info);
        GenerateSystems(info);
        GenerateInternalExtensions(info);
    }

    private static void GenerateBase(GeneratorInfo info) {
        var root = WorldClassWithMembers(info, GetMembers());
        info.Context.Add("Xeno/World.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers() {
            // fields
            yield return Helpers.PublicReadOnlyField("string", "Name");
            yield return Helpers.PublicReadOnlyField("ushort", "Id");
            yield return Helpers.PrivateField("bool", "isDisposed")
                .WithTrailingTrivia(Comment("\n"));

            // constructor
            yield return Helpers.PublicConstructor("World", "in string name, in ushort id")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("Id = id;"),
                    ParseStatement("Name = name;")
                        .WithTrailingTrivia(Comment("\n")),
                    // TODO: make this constants configurable
                    ParseStatement("InitializeEntities(4096);"),
                    ParseStatement("InitializeArchetypes(256);"),
                    ParseStatement("InitializeStores(4096, 128);")
                ));

            // disposer
            yield return Helpers.PublicVoidMethod("Dispose")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    DisposeCheck,
                    ParseStatement("DisposeStores();"),
                    ParseStatement("DisposeArchetypes();"),
                    ParseStatement("DisposeEntities();"),
                    ParseStatement("isDisposed = true;")
                ));

            // disposed accessor
            yield return Helpers.PublicMethod("bool", "IsDisposed")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("return isDisposed;")));

            yield return Helpers.PrivateMethod("(string Name, ushort Id)", "Describe")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("return (Name, Id);")
                ));

            // partial init methods
            yield return Helpers.PartialVoidMethod("InitializeEntities", "in uint capacity").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("InitializeArchetypes", "in uint capacity").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("InitializeStores", "in uint sparseCapacity, in uint denseCapacity").WithTrailingTrivia(Comment(";"));

            // partial dispose methods
            yield return Helpers.PartialVoidMethod("DisposeStores").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("DisposeArchetypes").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("DisposeEntities").WithTrailingTrivia(Comment(";"));
        }
    }

    private static void GenerateEntities(GeneratorInfo info) {
        var root = WorldClassWithMembers(info, GetMembers());
        info.Context.Add("Xeno/World.Entities.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PrivateConstant("uint", "AllocatedMask", "0b10000000_00000000_00000000_00000000U");
            yield return Helpers.PrivateConstant("uint", "NonAllocatedMask", "~AllocatedMask")
                .WithTrailingTrivia(Comment("\n"));

            // entities storage
            yield return Helpers.PrivateField("uint", "entityCount");
            yield return Helpers.PrivateField("RWEntity[]", "entities");
            yield return Helpers.PrivateField("uint", "freeIdsCount");
            yield return Helpers.PrivateField("uint[]", "freeIds")
                .WithTrailingTrivia(Comment("\n"));

            // initializers
            yield return Helpers.PartialVoidMethod("InitializeEntities", "in uint capacity")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("entityCount = 0;"),
                    ParseStatement("entities = new RWEntity[capacity];"),
                    ParseStatement("freeIdsCount = 0;"),
                    ParseStatement("freeIds = Array.Empty<uint>();"),
                    ParseStatement("// initializing new entities"),
                    ParseStatement("var span_entities = entities.AsSpan((int)entityCount, (int)capacity);"),
                    ParseStatement("var size = freeIds.Length == 0 ? 1 : freeIds.Length;"),
                    ParseStatement("while (size < freeIdsCount + capacity) size <<= 1;"),
                    ParseStatement("Array.Resize(ref freeIds, size);"),
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

            // grow method
            yield return Helpers.PrivateVoidMethod("GrowCapacity", "in uint capacity")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("// entities"),
                    ParseStatement("var arr = new RWEntity[capacity];"),
                    ParseStatement("Array.Copy(entities, arr, entities.Length);"),
                    ParseStatement("entities = arr;"),

                    ParseStatement("// initializing new entities"),
                    ParseStatement("var count = capacity - entityCount;"),
                    ParseStatement("var span_entities = entities.AsSpan((int)entityCount, (int)count);"),
                    ParseStatement("var size = freeIds.Length == 0 ? 1 : freeIds.Length;"),
                    ParseStatement("while (size < freeIdsCount + count) size <<= 1;"),
                    ParseStatement("Array.Resize(ref freeIds, size);"),
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
                    ParseStatement("freeIdsCount = (uint)freeIdsCount_int;"),

                    ParseStatement("// archetypes"),
                    ParseStatement("var at = new Archetype[capacity];"),
                    ParseStatement("Array.Copy(entityArchetypes, at, entityArchetypes.Length);"),
                    ParseStatement("entityArchetypes = at;"),
                    ParseStatement("var atiid = new uint[capacity];"),
                    ParseStatement("Array.Copy(inArchetypeLocalIndices, atiid, inArchetypeLocalIndices.Length);"),
                    ParseStatement("inArchetypeLocalIndices = atiid;")
                ))
                .AddBodyStatements(ParseStatement("// components"))
                .AddBodyStatements(
                    info.RegisteredComponents.Select(c => Block(
                        ParseStatement("var storeSparse = new uint[capacity];"),
                        ParseStatement($"Array.Copy({c.StoreName}.sparse, storeSparse, {c.StoreName}.sparse.Length);"),
                        ParseStatement($"{c.StoreName}.sparse = storeSparse;"))).ToArray<StatementSyntax>());

            // disposer
            yield return Helpers.PartialVoidMethod("DisposeEntities")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("entityCount = 0;"),
                    ParseStatement("entities = default;"),
                    ParseStatement("freeIdsCount = 0;"),
                    ParseStatement("freeIds = default;")
                ));

            // entities api
            yield return Helpers.PublicMethod("Entity", "CreateEmpty")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    DisposeCheck,
                    ParseStatement("if (entityCount == entities.Length)"),
                    ParseStatement("GrowCapacity(entityCount << 1);"),
                    ParseStatement("// create entity self"),
                    ParseStatement("var e_id = entityCount;"),
                    ParseStatement("if (freeIdsCount > 0)"),
                    Block(
                        ParseStatement("freeIdsCount--;"),
                        ParseStatement("e_id = freeIds[freeIdsCount];")
                        ),
                    ParseStatement("entities[e_id].Version |= AllocatedMask;"),
                    ParseStatement("var entity = Unsafe.As<RWEntity, Entity>(ref entities[e_id]);"),
                    ParseStatement("entityCount++;"),
                    ParseStatement("// add to zero archetype"),
                    ParseStatement("if (zeroArchetype.entitiesCount == zeroArchetype.entities.Length)"),
                    ParseStatement("Array.Resize(ref zeroArchetype.entities, zeroArchetype.entities.Length << 1);"),
                    ParseStatement("zeroArchetype.entities[zeroArchetype.entitiesCount] = entity.Id;"),
                    ParseStatement("inArchetypeLocalIndices[entity.Id] = zeroArchetype.entitiesCount;"),
                    ParseStatement("zeroArchetype.entitiesCount++;"),
                    ParseStatement("entityArchetypes[entity.Id] = zeroArchetype;"),
                    ParseStatement("return entity;")
                    ));

            yield return Helpers.PublicVoidMethod("Delete", "in Entity entity")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    DisposeCheck,
                    ParseStatement("ref var e = ref entities[entity.Id];"),
                    ParseStatement("e.Version &= NonAllocatedMask;"),
                    ParseStatement("e.Version++;"),
                    ParseStatement("entityCount--;"),
                    ParseStatement("if (freeIdsCount == freeIds.Length)"),
                    ParseStatement("Array.Resize(ref freeIds, (int)(freeIdsCount << 1));"),
                    ParseStatement("freeIds[freeIdsCount++] = e.Id;")
                ));

            yield return Helpers.PublicMethod("bool", "IsValid", "in Entity e")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    DisposeCheck,
                    ParseStatement("return e.Id < entities.Length && entities[e.Id].Version == e.Version;")
                ));

            foreach (var c in info.RegisteredComponents) {
                yield return Helpers.InternalVoidMethod("Add", $"in Entity e, in {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("// TODO")
                    ));

                yield return Helpers.InternalVoidMethod("Get", $"in Entity e, ref {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("// TODO")
                    ));

                yield return Helpers.InternalVoidMethod("Remove", $"in Entity e, {c.TypeFullName} _")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement("// TODO")
                    ));
            }

            // rw entity class
            yield return EntityGenerator.GetEntityDeclarationWithName("RWEntity", false)
                .WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));
        }
    }

    private static void GenerateArchetypes(GeneratorInfo info) {
        var root = WorldClassWithMembers(info, GetMembers());
        info.Context.Add("Xeno/World.Archetypes.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.PrivateField("Archetypes", "archetypes");
            yield return Helpers.PrivateField("Archetype", "zeroArchetype")
                .WithTrailingTrivia(Comment("\n"));

            yield return Helpers.PrivateField("Archetype[]", "entityArchetypes");
            yield return Helpers.PrivateField("uint[]", "inArchetypeLocalIndices")
                .WithTrailingTrivia(Comment("\n"));

            // initializer
            yield return Helpers.PartialVoidMethod("InitializeArchetypes", "in uint capacity")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("archetypes = new Archetypes(this);"),
                    ParseStatement("zeroArchetype = archetypes.AddPermanent(ref SetReadOnly.Zero);"),
                    ParseStatement("entityArchetypes = new Archetype[capacity];"),
                    ParseStatement("inArchetypeLocalIndices = new uint[capacity];")
                ));

            // disposer
            yield return Helpers.PartialVoidMethod("DisposeArchetypes")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("archetypes = default;"),
                    ParseStatement("zeroArchetype = default;"),
                    ParseStatement("entityArchetypes = default;"),
                    ParseStatement("inArchetypeLocalIndices = default;")
                ));
        }
    }

    private static void GenerateStores(GeneratorInfo info) {
        var root = WorldClassWithMembers(info, GetMembers(info));
        info.Context.Add("Xeno/World.Stores.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers(GeneratorInfo info) {
            // component storages
            for (var i = 0; i < info.RegisteredComponents.Length; i++) {
                var component = info.RegisteredComponents[i];
                yield return Helpers.PrivateField(component.StoreTypeName, component.StoreName)
                    .WithTrailingTrivia(Comment(i == info.RegisteredComponents.Length - 1 ? "\n" : string.Empty));
            }

            // initializer
            yield return Helpers.PartialVoidMethod("InitializeStores", "in uint sparseCapacity, in uint denseCapacity")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(info.RegisteredComponents.Select(component =>
                    ParseStatement($"s_{component.Index} = new {component.StoreTypeName}(sparseCapacity, denseCapacity);"))));

            // disposer
            yield return Helpers.PartialVoidMethod("DisposeStores")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(info.RegisteredComponents.Select(component =>
                    ParseStatement($"s_{component.Index} = default;"))));


            foreach (var c in info.RegisteredComponents) {
                // add component logic
                yield return Helpers.PrivateVoidMethod("Add", $"in uint entityId, in {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement($"var c = {c.StoreName}.count;"),
                        ParseStatement($"if (c == {c.StoreName}.dense.Length)"),
                        Block(
                            ParseStatement("var dense = new uint[c << 1];"),
                            ParseStatement($"var data = new {c.TypeFullName}[c << 1];"),
                            ParseStatement($"{c.StoreName}.dense.AsSpan().CopyTo(dense);"),
                            ParseStatement($"{c.StoreName}.data.AsSpan().CopyTo(data);"),
                            ParseStatement($"{c.StoreName}.dense = dense;"),
                            ParseStatement($"{c.StoreName}.data = data;")
                            ),
                        ParseStatement($"{c.StoreName}.data[c] = value;"),
                        ParseStatement($"{c.StoreName}.sparse[entityId] = c;"),
                        ParseStatement($"{c.StoreName}.dense[c] = entityId;"),
                        ParseStatement($"{c.StoreName}.count++;")
                        ));

                // has logic
                yield return Helpers.PrivateMethod("bool", "Has", $"in uint entityId, {c.TypeFullName} _")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement($"var d1 = {c.StoreName}.sparse[entityId];"),
                        ParseStatement($"if (d1 >= {c.StoreName}.count) return false;"),
                        ParseStatement($"var sp = {c.StoreName}.dense[d1];"),
                        ParseStatement("if (sp != entityId) return false;"),
                        ParseStatement("return true;")
                    ));

                // remove component logic
                yield return Helpers.PrivateMethod("bool", "Remove", $"in uint entityId, ref {c.TypeFullName} value")
                    .AddAttributeLists(Helpers.AggressiveInlining)
                    .WithBody(Block(
                        ParseStatement($"var d1 = {c.StoreName}.sparse[entityId];"),
                        ParseStatement($"if (d1 >= {c.StoreName}.count) return false;"),
                        ParseStatement($"var sp = {c.StoreName}.dense[d1];"),
                        ParseStatement("if (sp != entityId) return false;"),
                        ParseStatement($"var last = --{c.StoreName}.count;"),
                        ParseStatement($"value = {c.StoreName}.data[d1];"),
                        ParseStatement($"{c.StoreName}.data[d1] = {c.StoreName}.data[last];"),
                        ParseStatement($"var ld = {c.StoreName}.dense[last];"),
                        ParseStatement($"{c.StoreName}.dense[d1] = ld;"),
                        ParseStatement($"{c.StoreName}.sparse[ld] = d1;"),
                        ParseStatement("return true;")
                    ));
            }
        }
    }

    private static void GenerateSystems(GeneratorInfo info) {
        var root = WorldClassWithMembers(info, GetMembers(info));
        info.Context.Add("Xeno/World.Systems.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers(GeneratorInfo info) {
            // system instances (if needed)
            foreach (var system in info.RegisteredSystems) {
            }

            yield return Helpers.PublicVoidMethod("Setup")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("PreUpdate", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("Update", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("PostUpdate", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("Shutdown")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
        }
    }

    private static void GenerateInternalExtensions(GeneratorInfo info) {
        var root = Helpers.InternalExtensionsClass(GetMembers(), info);
        info.Context.Add("Xeno/World.InternalExtensions.g.cs", root);
        return;

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            yield return Helpers.ExtensionMethod("(string Name, ushort Id)", "World world", "Describe")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("return world != null ? (world.Name, world.Id) : (default, default);")
                ));
        }
    }

    private static CompilationUnitSyntax WorldClassWithMembers(GeneratorInfo info, IEnumerable<MemberDeclarationSyntax> members) {
        return CompilationUnit()
            .AddUsings(
                UsingDirective(ParseName("System")),
                UsingDirective(ParseName("System.Runtime.InteropServices")),
                UsingDirective(ParseName("System.Runtime.CompilerServices"))
                )
            .AddMembers(NamespaceDeclaration(ParseName(info.Compilation.AssemblyName ?? string.Empty))
                .AddMembers(ClassDeclaration("World")
                    .AddBaseListTypes(SimpleBaseType(ParseTypeName("Xeno.IWorld")))
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithMembers(List(members))
                ));
    }
}
