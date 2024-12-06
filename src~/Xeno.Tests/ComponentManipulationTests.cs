namespace Xeno.Tests;

[TestFixture]
public class ComponentManipulationTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void AddingComponentToEntityUpdatesArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        var initialArchetype = world.entityArchetypes[e.Id];

        world.AddComponents(e, new ComponentA());
        var newArchetype = world.entityArchetypes[e.Id];

        Assert.That(initialArchetype, Is.Not.EqualTo(newArchetype));
        Assert.That(newArchetype.mask.Get(CI<ComponentA>.Index), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingComponentFromEntityUpdatesArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        var archetypeWithA = world.entityArchetypes[e.Id];

        world.RemoveComponents<ComponentA>(e);
        var archetypeWithoutA = world.entityArchetypes[e.Id];

        Assert.That(archetypeWithA, Is.Not.EqualTo(archetypeWithoutA));
        Assert.That(archetypeWithoutA.mask.Get(CI<ComponentA>.Index), Is.False);

        e.Destroy();
    }

    [Test]
    public void AddingMultipleComponentsUpdatesMask() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA(), new ComponentB());

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype.mask.Get(CI<ComponentA>.Index), Is.True);
        Assert.That(archetype.mask.Get(CI<ComponentB>.Index), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingOneComponentPreservesOther() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA(), new ComponentB());

        world.RemoveComponents<ComponentA>(e);

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype.mask.Get(CI<ComponentA>.Index), Is.False);
        Assert.That(archetype.mask.Get(CI<ComponentB>.Index), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingAllComponentsMovesEntityToZeroArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA(), new ComponentB());

        world.RemoveComponents<ComponentA>(e);
        world.RemoveComponents<ComponentB>(e);

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(archetype.mask.GetIndices(), Is.Empty);

        e.Destroy();
    }

    [Test]
    public void OverwritingComponentDoesNotCreateDuplicate() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA());
        var initialArchetype = world.entityArchetypes[e.Id];

        world.AddComponents(e, new ComponentA()); // Adding the same component again

        var finalArchetype = world.entityArchetypes[e.Id];

        Assert.That(initialArchetype, Is.EqualTo(finalArchetype)); // Should not create a new archetype
        Assert.That(finalArchetype.mask.GetIndices().Count(), Is.EqualTo(1)); // No duplicate entries

        e.Destroy();
    }

    [Test]
    public void ModifyingComponentDoesNotChangeArchetype() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        world.AddComponents(e, new ComponentA { Value = 10 });
        var archetypeBefore = world.entityArchetypes[e.Id];

        ref var component = ref world.Ref<ComponentA>(e);
        component.Value = 20;

        var archetypeAfter = world.entityArchetypes[e.Id];

        Assert.That(archetypeBefore, Is.EqualTo(archetypeAfter)); // Archetype should not change
        Assert.That(world.Ref<ComponentA>(e).Value, Is.EqualTo(20));

        e.Destroy();
    }

    [Test]
    public void AddingAndRemovingComponentsMultipleTimesDoesNotCorruptSystem() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();

        for (int i = 0; i < 100; i++) {
            world.AddComponents(e, new ComponentA());
            world.RemoveComponents<ComponentA>(e);
        }

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(archetype.mask.GetIndices(), Is.Empty);

        e.Destroy();
    }
}
