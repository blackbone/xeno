namespace Xeno.Tests;

[TestFixture]
public class IntegrityChecksOnRemovalsAndSwapsTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EntityDeletionDoesNotBreakArchetypeIntegrity() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());

        var archetypeBefore = world.entityArchetypes[e.Id];

        e.Destroy();
        var archetypeAfter = world.entityArchetypes[e.Id];

        Assert.That(archetypeBefore, Is.Not.EqualTo(archetypeAfter));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void LastEntitySwapWorksCorrectlyWhenRemovingEntities() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());

        var archetype = world.entityArchetypes[e1.Id];

        var initialCount = archetype.entitiesCount;
        e1.Destroy();
        var afterRemovalCount = archetype.entitiesCount;

        Assert.That(afterRemovalCount, Is.EqualTo(initialCount - 1));
    }

    [Test]
    public void InArchetypeLocalIndicesRemainsValidWhenSwapping() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());

        var archetype = world.entityArchetypes[e1.Id];

        var indexBefore = world.inArchetypeLocalIndices[e2.Id];
        e1.Destroy();
        var indexAfter = world.inArchetypeLocalIndices[e2.Id];

        Assert.That(indexAfter, Is.EqualTo(0)); // The entity index should remain consistent
    }

    [Test]
    public void RemovingLastEntityInArchetypeDoesNotCauseOutOfBoundsAccess() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());

        var archetype = world.entityArchetypes[e.Id];
        Assert.DoesNotThrow(() => e.Destroy());

        Assert.That(world.entityArchetypes[e.Id], Is.Null);
        Assert.That(archetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void EntityOrderCorrectnessWhenSwappingLastEntity() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());

        var archetype = world.entityArchetypes[e1.Id];

        var lastEntityId = archetype.entities[archetype.entitiesCount - 1];
        e1.Destroy();

        Assert.That(archetype.entities[0], Is.EqualTo(lastEntityId)); // Ensure last entity took the removed entity's place
    }

    [Test]
    public void DeletingAllEntitiesInAnArchetypeResetsEntityCountToZero() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());

        var archetype1 = world.entityArchetypes[e1.Id];
        var archetype2 = world.entityArchetypes[e1.Id];

        Assert.That(archetype1, Is.Not.Null);
        Assert.That(archetype2, Is.Not.Null);
        Assert.That(archetype1, Is.EqualTo(archetype2));
        Assert.That(archetype1.entitiesCount, Is.EqualTo(2));

        e1.Destroy();
        e2.Destroy();

        Assert.That(archetype1.entitiesCount, Is.EqualTo(0));
        Assert.That(world.entityArchetypes[e1.Id], Is.Null);
        Assert.That(world.entityArchetypes[e2.Id], Is.Null);
    }

    [Test]
    public void RemovingEntityInMiddleDoesNotShiftUnrelatedEntityIndices() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());
        world.AddComponents(e3, new ComponentA());

        var archetype = world.entityArchetypes[e1.Id];
        var e2IndexBefore = world.inArchetypeLocalIndices[e2.Id];

        e1.Destroy();

        var e2IndexAfter = world.inArchetypeLocalIndices[e2.Id];
        Assert.That(e2IndexAfter, Is.EqualTo(e2IndexBefore));
    }

    [Test]
    public void EntityRemovalsDoNotCauseMemoryLeaks() {
        Worlds.TryGet("world", out var world);

        var initialEntityCount = world.zeroArchetype.entitiesCount;
        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());

        e.Destroy();
        var afterRemovalCount = world.zeroArchetype.entitiesCount;

        Assert.That(afterRemovalCount, Is.EqualTo(initialEntityCount));
    }

    [Test]
    public void EntitySwappingDoesNotCorruptArchetypeStorage() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        world.AddComponents(e1, new ComponentA());
        world.AddComponents(e2, new ComponentA());

        var archetype = world.entityArchetypes[e1.Id];

        Span<uint> entitiesBefore = archetype.entities[..(int)archetype.entitiesCount];
        e1.Destroy();
        Span<uint> entitiesAfter = archetype.entities[..(int)archetype.entitiesCount];

        Assert.That(entitiesBefore.Length, Is.GreaterThan(entitiesAfter.Length)); // Should reduce size
    }

    [Test]
    public void EmptyArchetypeGetsProperlyRemovedWhenNoEntitiesRemain() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        e.Destroy();

        Assert.That(world.entityArchetypes[e.Id], Is.Null);
    }
}
