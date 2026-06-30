using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class ArchetypeTransitionTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EntityStartsInEmptyArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void AddingComponentMovesEntityToNewArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        var oldArchetype = world.entityArchetypes[e.Id];

        world.Add(e, new ComponentA());

        var newArchetype = world.entityArchetypes[e.Id];

        Assert.That(newArchetype, Is.Not.EqualTo(oldArchetype));
        Assert.That(oldArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(newArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void RemovingComponentMovesEntityBackToEmptyArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        var newArchetype = world.entityArchetypes[e.Id];

        world.RemoveComponentA(e);

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(newArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void AddingMultipleComponentsCreatesCorrectArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(world.HasComponentAAndComponentB(e), Is.True);
        Assert.That(archetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void RemovingOneComponentPreservesOtherComponents() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());
        var archetypeBefore = world.entityArchetypes[e.Id];

        world.RemoveComponentA(e);
        var archetypeAfter = world.entityArchetypes[e.Id];

        Assert.That(archetypeBefore, Is.Not.EqualTo(archetypeAfter));
        Assert.That(world.HasComponentA(e), Is.False);
        Assert.That(world.HasComponentB(e), Is.True);
        Assert.That(archetypeAfter.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void EntityCanMoveBetweenMultipleArchetypes() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();

        world.Add(e, new ComponentA());
        var archetypeA = world.entityArchetypes[e.Id];

        world.Add(e, new ComponentB());
        var archetypeAB = world.entityArchetypes[e.Id];

        world.RemoveComponentA(e);
        var archetypeB = world.entityArchetypes[e.Id];

        world.RemoveComponentB(e);
        var archetypeEmpty = world.entityArchetypes[e.Id];

        Assert.That(archetypeA, Is.Not.EqualTo(archetypeAB));
        Assert.That(archetypeAB, Is.Not.EqualTo(archetypeB));
        Assert.That(archetypeB, Is.Not.EqualTo(archetypeEmpty));
        Assert.That(archetypeEmpty, Is.EqualTo(world.zeroArchetype));

        e.Destroy();
    }

    [Test]
    public void MovingEntityBetweenArchetypesMaintainsCorrectIndex() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();

        world.Add(e1, new ComponentA());
        world.Tick(0f);

        Assert.That(world.inArchetypeLocalIndices[e1.Id], Is.EqualTo(0));
        Assert.That(world.inArchetypeLocalIndices[e2.Id], Is.EqualTo(0));

        e1.Destroy();
        e2.Destroy();
    }

    [Test]
    public void RemovingAllComponentsMovesEntityToDefaultArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());

        world.RemoveComponentA(e);
        world.RemoveComponentB(e);

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }
}
