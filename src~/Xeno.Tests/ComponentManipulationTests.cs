using System.Linq;
using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class ComponentManipulationTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void AddingComponentToEntityUpdatesArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        var initialArchetype = world.entityArchetypes[e.Id];

        world.Add(e, new ComponentA());
        var newArchetype = world.entityArchetypes[e.Id];

        Assert.That(initialArchetype, Is.Not.EqualTo(newArchetype));
        Assert.That(world.HasComponentA(e), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingComponentFromEntityUpdatesArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        var archetypeWithA = world.entityArchetypes[e.Id];

        world.RemoveComponentA(e);
        var archetypeWithoutA = world.entityArchetypes[e.Id];

        Assert.That(archetypeWithA, Is.Not.EqualTo(archetypeWithoutA));
        Assert.That(world.HasComponentA(e), Is.False);

        e.Destroy();
    }

    [Test]
    public void AddingMultipleComponentsUpdatesMask() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(world.HasComponentAAndComponentB(e), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingOneComponentPreservesOther() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());

        world.RemoveComponentA(e);

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(world.HasComponentA(e), Is.False);
        Assert.That(world.HasComponentB(e), Is.True);

        e.Destroy();
    }

    [Test]
    public void RemovingAllComponentsMovesEntityToZeroArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        world.Add(e, new ComponentB());

        world.RemoveComponentA(e);
        world.RemoveComponentB(e);

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(archetype.mask.GetIndices(), Is.Empty);

        e.Destroy();
    }

    [Test]
    public void OverwritingComponentDoesNotCreateDuplicate() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA());
        var initialArchetype = world.entityArchetypes[e.Id];

        world.Add(e, new ComponentA()); // Adding the same component again

        var finalArchetype = world.entityArchetypes[e.Id];

        Assert.That(initialArchetype, Is.EqualTo(finalArchetype)); // Should not create a new archetype
        Assert.That(finalArchetype.mask.GetIndices().Count(), Is.EqualTo(1)); // No duplicate entries

        e.Destroy();
    }

    [Test]
    public void ModifyingComponentDoesNotChangeArchetype() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, new ComponentA { Value = 10 });
        var archetypeBefore = world.entityArchetypes[e.Id];

        ref var component = ref world.RefComponentA(e);
        component.Value = 20;

        var archetypeAfter = world.entityArchetypes[e.Id];

        Assert.That(archetypeBefore, Is.EqualTo(archetypeAfter)); // Archetype should not change
        Assert.That(world.RefComponentA(e).Value, Is.EqualTo(20));

        e.Destroy();
    }

    [Test]
    public void AddingAndRemovingComponentsMultipleTimesDoesNotCorruptSystem() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();

        for (int i = 0; i < 100; i++) {
            world.Add(e, new ComponentA());
            world.RemoveComponentA(e);
        }

        var archetype = world.entityArchetypes[e.Id];

        Assert.That(archetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(archetype.mask.GetIndices(), Is.Empty);

        e.Destroy();
    }
}
