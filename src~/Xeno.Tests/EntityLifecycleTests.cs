using System.Collections.Generic;
using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class EntityLifecycleTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void CreateSingleEntity() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entities[0], Is.EqualTo(e.Id));

        e.Destroy();
    }

    [Test]
    public void CreateMultipleEntities() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        Assert.That(e1.Id, Is.Not.EqualTo(e2.Id));
        Assert.That(e2.Id, Is.Not.EqualTo(e3.Id));

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(3));
        Assert.That(world.inArchetypeLocalIndices[e1.Id], Is.EqualTo(0));
        Assert.That(world.inArchetypeLocalIndices[e2.Id], Is.EqualTo(1));
        Assert.That(world.inArchetypeLocalIndices[e3.Id], Is.EqualTo(2));

        e1.Destroy();
        e2.Destroy();
        e3.Destroy();
    }

    [Test]
    public void ReuseEntityIdsAfterDeletion() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e2.Destroy();
        var e4 = world.CreateEntity();

        Assert.That(e4.Id, Is.EqualTo(e2.Id)); // Should reuse the smallest free ID
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(3));

        e1.Destroy();
        e3.Destroy();
        e4.Destroy();
    }

    [Test]
    public void IncrementVersionOnReuse() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var version1 = e1.Version;

        e1.Destroy();

        var e2 = world.CreateEntity();
        var version2 = e2.Version;

        Assert.That(e2.Id, Is.EqualTo(e1.Id)); // Reused ID
        Assert.That(version2, Is.GreaterThan(version1)); // Version should increase

        e2.Destroy();
    }

    [Test]
    public void InvalidEntityDeletionIsIgnored() {
        var world = TestWorlds.Get("world");

        var e = new Entity(); // Non-existent entity

        Assert.DoesNotThrow(() => e.Destroy());
    }

    [Test]
    public void DeletingEntityRemovesItFromArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        e.Destroy();

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0)); // Should be reset
    }

    [Test]
    public void DeletingAllEntitiesAndRecreatingDoesNotCorruptIndices() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e3.Destroy();
        e2.Destroy();
        e1.Destroy();

        var e4 = world.CreateEntity();
        var e5 = world.CreateEntity();
        var e6 = world.CreateEntity();

        Assert.That(e4.Id, Is.EqualTo(e1.Id));
        Assert.That(e5.Id, Is.EqualTo(e2.Id));
        Assert.That(e6.Id, Is.EqualTo(e3.Id));

        e4.Destroy();
        e5.Destroy();
        e6.Destroy();
    }

    [Test]
    public void CreatingEntitiesBeyondInitialCapacityDoesNotCrash() {
        var world = TestWorlds.Get("world");

        var entities = new List<Entity>();

        for (int i = 0; i < 1000; i++) {
            entities.Add(world.CreateEntity());
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1000));

        for (var i = 0; i < entities.Count; i++) {
            var e = entities[i];
            e.Destroy();
        }
    }
}
