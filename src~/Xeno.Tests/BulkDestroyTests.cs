using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class BulkDestroyTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
        if (Worlds.TryGet("foreign", out var foreign))
            foreign.Dispose();
    }

    [Test]
    public void DestroyEntities_InvalidatesAndRecyclesIdsWithoutLettingStaleHandlesDeleteReusedSlots() {
        var world = TestWorlds.Get("world");
        var stale = world.CreateEntity();

        world.DestroyEntities(new[] { stale });

        Assert.That(world.IsEntityValid(stale), Is.False);
        Assert.That(world.EntityCount, Is.Zero);

        var replacement = world.CreateEntity();
        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(replacement.Version, Is.Not.EqualTo(stale.Version));

        world.DestroyEntities(new[] { stale });

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.EntityCount, Is.EqualTo(1));
    }

    [Test]
    public void DestroyEntities_IgnoresDuplicateStaleAndForeignEntities() {
        var world = TestWorlds.Get("world");
        var foreignWorld = TestWorlds.Create("foreign");
        var duplicate = world.CreateEntity();
        var stale = world.CreateEntity();

        world.DestroyEntity(stale);
        var replacement = world.CreateEntity();
        var foreign = foreignWorld.CreateEntity();

        world.DestroyEntities(new[] { stale, duplicate, duplicate, foreign });

        Assert.That(world.IsEntityValid(duplicate), Is.False);
        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(foreignWorld.IsEntityValid(foreign), Is.True);
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(foreignWorld.EntityCount, Is.EqualTo(1));
    }

    [Test]
    public void DestroyEntities_DeletesEmptyEntitiesFromZeroArchetype() {
        var world = TestWorlds.Get("world");
        var entities = new Entity[16];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity();

        world.DestroyEntities(entities);

        Assert.That(world.EntityCount, Is.Zero);
        Assert.That(world.zeroArchetype.entitiesCount, Is.Zero);
        for (var i = 0; i < entities.Length; i++)
            Assert.That(world.IsEntityValid(entities[i]), Is.False);
    }

    [Test]
    public void DestroyEntities_ClearsComponentDataAndCountsForOneToFourComponentArchetypes() {
        var world = TestWorlds.Get("world");
        var e1 = world.CreateEntity(new ComponentA { Value = 11 });
        var e2 = world.CreateEntity(new ComponentA { Value = 21 }, new ComponentB { Value = 22 });
        var e3 = world.CreateEntity(new Component_1(), new Component_2(), new Component_3());
        var e4 = world.CreateEntity(new Component_4(), new Component_5(), new Component_6(), new Component_7());

        world.DestroyEntities(new[] { e1, e2, e3, e4 });

        Assert.That(world.EntityCount, Is.Zero);
        Assert.That(world.CountComponentA(), Is.Zero);
        Assert.That(world.CountComponentAAndComponentB(), Is.Zero);
        Assert.That(world.CountComponent_1AndComponent_2AndComponent_3(), Is.Zero);
        Assert.That(world.CountComponent_4AndComponent_5AndComponent_6AndComponent_7(), Is.Zero);
        var r1 = world.CreateEntity(new ComponentA { Value = 101 });
        var r2 = world.CreateEntity(new ComponentA { Value = 102 });
        var r3 = world.CreateEntity(new ComponentA { Value = 103 });
        var r4 = world.CreateEntity(new ComponentA { Value = 104 });

        Assert.That(r3.Id, Is.EqualTo(e2.Id));
        Assert.That(r4.Id, Is.EqualTo(e1.Id));
        Assert.That(world.RefComponentA(r3).Value, Is.EqualTo(103));
        Assert.That(world.RefComponentA(r4).Value, Is.EqualTo(104));
    }

    [Test]
    public void DestroyEntities_PreservesSwappedEntityLocalIndexWhenDeletingMiddleEntity() {
        var world = TestWorlds.Get("world");
        var entities = new Entity[5];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity(new ComponentA { Value = i });

        var archetype = world.entityArchetypes[entities[0].Id];
        var removedIndex = world.inArchetypeLocalIndices[entities[2].Id];
        var lastId = entities[^1].Id;

        world.DestroyEntities(new[] { entities[2] });

        Assert.That(world.EntityCount, Is.EqualTo(4));
        Assert.That(archetype.entitiesCount, Is.EqualTo(4));
        Assert.That(world.inArchetypeLocalIndices[lastId], Is.EqualTo(removedIndex));
        Assert.That(archetype.entities[removedIndex], Is.EqualTo(lastId));
    }

    [Test]
    public void DestroyEntities_ReturnsEmptyFloatingArchetypeForReuse() {
        var world = TestWorlds.Get("world");
        var entity = world.CreateEntity(new ComponentA { Value = 1 });
        var archetype = world.entityArchetypes[entity.Id];

        world.DestroyEntities(new[] { entity });

        Assert.That(archetype.entitiesCount, Is.Zero);
        Assert.That(world.CountComponentA(), Is.Zero);

        var replacement = world.CreateEntity(new ComponentA { Value = 2 });

        Assert.That(ReferenceEquals(world.entityArchetypes[replacement.Id], archetype), Is.True);
        Assert.That(world.CountComponentA(), Is.EqualTo(1));
    }
}
