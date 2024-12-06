namespace Xeno.Tests;

[TestFixture]
public class ArchetypeTransitionTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EntityStartsInEmptyArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void AddingComponentMovesEntityToNewArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        var oldArchetype = world.entityArchetypes[e.Id];

        world.AddComponents(e, new ComponentA());

        var newArchetype = world.entityArchetypes[e.Id];

        Assert.That(newArchetype, Is.Not.EqualTo(oldArchetype));
        Assert.That(oldArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(newArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void RemovingComponentMovesEntityBackToEmptyArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        var newArchetype = world.entityArchetypes[e.Id];

        world.RemoveComponents<ComponentA>(e);

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(newArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void AddingMultipleComponentsCreatesCorrectArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        world.AddComponents(e, new ComponentB());

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype.mask.Get(CI<ComponentA>.Index), Is.True);
        Assert.That(archetype.mask.Get(CI<ComponentB>.Index), Is.True);
        Assert.That(archetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void RemovingOneComponentPreservesOtherComponents() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        world.AddComponents(e, new ComponentB());
        var archetypeBefore = world.entityArchetypes[e.Id];

        world.RemoveComponents<ComponentA>(e);
        var archetypeAfter = world.entityArchetypes[e.Id];

        Assert.That(archetypeBefore, Is.Not.EqualTo(archetypeAfter));
        Assert.That(archetypeAfter.mask.Get(CI<ComponentA>.Index), Is.False);
        Assert.That(archetypeAfter.mask.Get(CI<ComponentB>.Index), Is.True);
        Assert.That(archetypeAfter.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }

    [Test]
    public void EntityCanMoveBetweenMultipleArchetypes() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();

        world.AddComponents(e, new ComponentA());
        var archetypeA = world.entityArchetypes[e.Id];

        world.AddComponents(e, new ComponentB());
        var archetypeAB = world.entityArchetypes[e.Id];

        world.RemoveComponents<ComponentA>(e);
        var archetypeB = world.entityArchetypes[e.Id];

        world.RemoveComponents<ComponentB>(e);
        var archetypeEmpty = world.entityArchetypes[e.Id];

        Assert.That(archetypeA, Is.Not.EqualTo(archetypeAB));
        Assert.That(archetypeAB, Is.Not.EqualTo(archetypeB));
        Assert.That(archetypeB, Is.Not.EqualTo(archetypeEmpty));
        Assert.That(archetypeEmpty, Is.EqualTo(world.zeroArchetype));

        e.Destroy();
    }

    [Test]
    public void MovingEntityBetweenArchetypesMaintainsCorrectIndex() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();

        world.AddComponents(e1, new ComponentA());

        Assert.That(world.inArchetypeLocalIndices[e1.Id], Is.EqualTo(0));
        Assert.That(world.inArchetypeLocalIndices[e2.Id], Is.EqualTo(0));

        e1.Destroy();
        e2.Destroy();
    }

    [Test]
    public void RemovingAllComponentsMovesEntityToDefaultArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        world.AddComponents(e, new ComponentB());

        world.RemoveComponents<ComponentA>(e);
        world.RemoveComponents<ComponentB>(e);

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));

        e.Destroy();
    }
}
