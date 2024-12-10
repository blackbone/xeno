using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xeno.SourceGenerator.SyntaxReceivers;
using NotImplementedException = System.NotImplementedException;

namespace Xeno.SourceGenerator;

using static SyntaxFactory;

internal static class WorldGenerator {
    public static StatementSyntax DisposeCheck { get; } = ParseStatement("Xeno.WorldDisposedException.ThrowIf(isDisposed, this.Describe());");

    public static void Generate(GeneratorInfo info) {
        if (!Ensure.IsEcsAssembly(info.Compilation)) return;

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

        IEnumerable<MemberDeclarationSyntax> GetMembers() {
            // fields
            yield return Helpers.PublicReadOnlyField("string", "Name");
            yield return Helpers.PublicReadOnlyField("ushort", "Id");
            yield return Helpers.PrivateField("bool", "isDisposed")
                .WithTrailingTrivia(Comment("\n"));

            var passInSystemInstanceArguments = string.Join(", ", info.RegisteredSystemGroups.Where(s => s.RequiresExternalInstance).Select(s => $"in {s.TypeFullName} {s.FieldName}"));
            var passSystemInstanceArguments = string.Join(", ", info.RegisteredSystemGroups.Where(s => s.RequiresExternalInstance).Select(s => $"{s.FieldName}"));

            // constructor
            yield return Helpers.PublicConstructor("World", $"in string name, in ushort id {(string.IsNullOrEmpty(passInSystemInstanceArguments) ? "" : $", {passInSystemInstanceArguments}")}")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    ParseStatement("Id = id;"),
                    ParseStatement("Name = name;")
                        .WithTrailingTrivia(Comment("\n")),
                    // TODO: make this constants configurable
                    ParseStatement("InitializeEntities(4096);"),
                    ParseStatement("InitializeArchetypes(256);"),
                    ParseStatement("InitializeStores(4096, 128);"),
                    ParseStatement($"InitializeSystems({passSystemInstanceArguments});")
                ));

            // disposer
            yield return Helpers.PublicVoidMethod("Dispose")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    DisposeCheck,
                    ParseStatement("DisposeSystems();"),
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
            yield return Helpers.PartialVoidMethod("InitializeSystems", passInSystemInstanceArguments).WithTrailingTrivia(Comment(";"));

            // partial dispose methods
            yield return Helpers.PartialVoidMethod("DisposeStores").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("DisposeArchetypes").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("DisposeEntities").WithTrailingTrivia(Comment(";"));
            yield return Helpers.PartialVoidMethod("DisposeSystems").WithTrailingTrivia(Comment(";"));
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

            // grow method
            yield return Helpers.PrivateVoidMethod("GrowCapacity", "in uint capacity")
                .AddAttributeLists(Helpers.AggressiveInlining)
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
                    ParseStatement("freeIdsCount = (uint)freeIdsCount_int;"),

                    ParseStatement("// archetypes"),
                    ParseStatement("Utils.Resize(ref entityArchetypes, capacity);"),
                    ParseStatement("Utils.Resize(ref inArchetypeLocalIndices, capacity);")
                ))
                .AddBodyStatements(ParseStatement("// components"))
                .AddBodyStatements(info.RegisteredComponents.Select(c
                    => ParseStatement($"Utils.Resize(ref {c.StoreName}.sparse, capacity);")).ToArray());
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
                    ParseStatement("Utils.Resize(ref zeroArchetype.entities, (uint)zeroArchetype.entities.Length << 1);"),
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
                    ParseStatement("Utils.Resize(ref freeIds, freeIdsCount << 1);"),
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
        // pre-init systems
        var ns = 0;
        foreach (var systemGroup in info.RegisteredSystemGroups) {
            foreach (var system in systemGroup.Systems) {
                system.Index = ns++;
            }
        }

        var root = WorldClassWithMembers(info, GetMembers(info));
        info.Context.Add("Xeno/World.Systems.g.cs", root);
        return;

        static IEnumerable<MemberDeclarationSyntax> GetMembers(GeneratorInfo info) {
            // iteration cached fields
            yield return Helpers.PrivateField("uint[]", "iterationBuffer");
            yield return Helpers.PrivateField("Archetype", "iterationCurrent");
            yield return Helpers.PrivateField("uint", "iterationCount");

            // system instances (if needed)
            var systems = info.RegisteredSystemGroups.Where(s => s.RequiresInstance).ToImmutableArray();
            for (var i = 0; i < systems.Length; i++) {
                var system = systems[i];
                yield return Helpers.PrivateField(system.TypeFullName, system.FieldName)
                    .WithTrailingTrivia(Comment(i == systems.Length - 1 ? "\n" : string.Empty));
            }

            // unnamed uniforms
            var uniques = new HashSet<(ITypeSymbol, string)>();
            foreach (var system in info.SystemInvocations.Values.SelectMany(s => s)) {
                foreach (var uniform in system.Parameters.Where(p =>
                             p.IsValidUniformParameter(info.Compilation, out var kind, out var name)
                             && kind == UniformKind.Named
                             && !system.Group.SystemGroupGroupType.HasMatchingField(name, p))) {
                    uniform.IsValidUniformParameter(info.Compilation, out _, out var name);
                    uniques.Add((uniform.Type, name));
                }
            }
            var uniquesArray = uniques.ToArray();
            for (var i = 0; i < uniquesArray.Length; i++) {
                var (type, name) = uniquesArray[i];
                yield return Helpers.PrivateField(type.ToDisplayString(), $"{name}_{type.ToDisplayString().Replace(".", "_")}")
                    .WithTrailingTrivia(Comment(i == uniquesArray.Length - 1 ? "\n" : string.Empty));
            }

            // cachedFilters
            foreach (var system in info.SystemInvocations.Values.SelectMany(s => s)) {
                yield return Helpers.PrivateStaticField("FilterReadOnly", system.FilterName, "default");
            }

            // system instances initialization
            yield return Helpers.PartialVoidMethod("InitializeSystems", string.Join(", ", info.RegisteredSystemGroups.Where(s => s.RequiresExternalInstance).Select(s => $"in {s.TypeFullName} {s.FieldName}")))
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(
                    info.RegisteredSystemGroups.Where(s => s.RequiresInstance).Select(s
                        => ParseStatement(s.RequiresExternalInstance
                            ? $"this.{s.FieldName} = {s.FieldName};"
                            : $"this.{s.FieldName} = new {s.TypeFullName}();"))
                    ))
                .WithLeadingTrivia(Comment(" "));

            yield return Helpers.PublicVoidMethod("Startup")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(GetSimpleStatements(info, SystemMethodType.Startup)));
            yield return Helpers.PublicVoidMethod("PreUpdate", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("Update", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(GetSimpleStatements(info, SystemMethodType.Update)));
            yield return Helpers.PublicVoidMethod("PostUpdate", "in float delta")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block());
            yield return Helpers.PublicVoidMethod("Shutdown")
                .AddAttributeLists(Helpers.AggressiveInlining)
                .WithBody(Block(GetSimpleStatements(info, SystemMethodType.Shutdown)));
        }
    }

    private static IEnumerable<StatementSyntax> GetSimpleStatements(GeneratorInfo info, SystemMethodType type) {
        if (!info.SystemInvocations.TryGetValue(type, out var systems))
            yield break;

        foreach (var system in systems) {
            var invocation = GenerateSystemInvocation(system, info);
            if (invocation != null)
                foreach (var statement in invocation)
                    yield return statement;
        }
    }

    private static IEnumerable<StatementSyntax> GenerateSystemInvocation(System system, GeneratorInfo info) {
        switch (system.Type) {
            case SystemMethodType.Startup:
            case SystemMethodType.Shutdown:
                return GenerateStartupShutdownSystemInvocation(system, info);
            case SystemMethodType.PreUpdate:
            case SystemMethodType.Update:
            case SystemMethodType.PostUpdate:
                try {
                    return GenerateUpdateSystemInvocation(system, info);
                } catch (Exception e) {
                    return [EmptyStatement().WithTrailingTrivia(Comment($"/* {e.Message} {e.StackTrace} */"))];
                }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static  IEnumerable<StatementSyntax> GenerateStartupShutdownSystemInvocation(System system, GeneratorInfo info) {
        if (system.Type is not SystemMethodType.Startup and not SystemMethodType.Shutdown)
            throw new InvalidOperationException();

        if (!system.Method.Parameters.All(p => p.IsValidUniformParameter(info.Compilation, out var kind, out _) && kind != UniformKind.Delta)) {
            throw new InvalidOperationException();
        }

        return SimpleInvocation(system, info);
    }

    private static IEnumerable<StatementSyntax> GenerateUpdateSystemInvocation(System system, GeneratorInfo info) {
        if (system.Type is not SystemMethodType.PreUpdate and not SystemMethodType.Update and not SystemMethodType.PostUpdate)
            throw new InvalidOperationException();

        var entityParameterCount = system.Method.Parameters.Count(p => p.IsValidEntityParameter());
        var uniformParameterCount = system.Method.Parameters.Count(p => p.IsValidUniformParameter(info.Compilation, out _, out _));
        var componentParameterCount = system.Method.Parameters.Count(p => p.IsValidComponentParameter());

        if (entityParameterCount + uniformParameterCount + componentParameterCount != system.Method.Parameters.Length)
            throw new InvalidOperationException();

        // simple invocation
        if (entityParameterCount == 0 && componentParameterCount == 0)
            return SimpleInvocation(system, info);

        // entity or component invocations requires iteration
        return IterateInvocation(system, info);
    }

    private static IEnumerable<StatementSyntax> SimpleInvocation(System system, GeneratorInfo info) {
        var parametersString = "";
        foreach (var parameter in system.Method.Parameters) {
            parameter.IsValidUniformParameter(info.Compilation, out var kind, out var name);
            parametersString += parameter.RefKind.ToParameterPrefix();
            switch (kind) {
                case UniformKind.Named:
                    if (system.Group.SystemGroupGroupType.HasMatchingField(name, parameter))
                        parametersString += $"{system.Group.FieldName}.{name}";
                    else
                        parametersString += $"{name}_{parameter.Type.ToDisplayString().Replace(".", "_")}";
                    break;
                case UniformKind.None: break;
            }
        }

        yield return ParseStatement($"{system.Invocation()}({parametersString});");
    }

    private static IEnumerable<StatementSyntax> IterateInvocation(System system, GeneratorInfo info) {
        return [
            ParseStatement("iterationCount = 0;").WithLeadingTrivia(Comment($"// start of {system.Method.ToDisplayString()}")),
            ParseStatement("iterationCurrent = archetypes.head;"),
            ParseStatement("while (iterationCurrent != null)"),
            Block(
                ParseStatement($"if (!{system.FilterName}.Match(iterationCurrent.mask)) continue;"),
                ParseStatement("Array.Copy(iterationCurrent.entities, 0, iterationBuffer, iterationCount, iterationCurrent.entitiesCount);"),
                ParseStatement("iterationCount += iterationCurrent.entitiesCount;")
                ).WithTrailingTrivia(Comment($"// end of {system.Method.ToDisplayString()}\n"))
        ];
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
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)))
                    .WithMembers(List(members))
                ));
    }
}
