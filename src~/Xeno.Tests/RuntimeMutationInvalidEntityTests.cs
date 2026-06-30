using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class RuntimeMutationInvalidEntityTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void StaleEntityCannotAddComponentsToReusedEntitySlot() {
        var world = TestWorlds.Get("world");

        var stale = world.CreateEntity();
        stale.Destroy();
        var replacement = world.CreateEntity();

        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(world.IsEntityValid(stale), Is.False);

        world.Add(stale, new ComponentA { Value = 10 });

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.HasComponentA(replacement), Is.False);

        replacement.Destroy();
    }

    [Test]
    public void StaleEntityCannotRemoveComponentsFromReusedEntitySlot() {
        var world = TestWorlds.Get("world");

        var stale = world.CreateEntity();
        stale.Destroy();
        var replacement = world.CreateEntity(new ComponentA { Value = 10 });

        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(world.IsEntityValid(stale), Is.False);

        world.RemoveComponentA(stale);

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.HasComponentA(replacement), Is.True);
        Assert.That(world.RefComponentA(replacement).Value, Is.EqualTo(10));

        replacement.Destroy();
    }

    [Test]
    public void StaleEntityCannotDestroyReusedEntitySlot() {
        var world = TestWorlds.Get("world");

        var stale = world.CreateEntity();
        stale.Destroy();
        var replacement = world.CreateEntity();

        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(world.IsEntityValid(stale), Is.False);

        world.DestroyEntity(stale);

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.EntityCount, Is.EqualTo(1));

        replacement.Destroy();
    }

    [Test]
    public void OutOfRangeEntityIsInvalidAndSafeForQueries() {
        var world = TestWorlds.Get("world");
        var invalid = new Entity {
            Id = uint.MaxValue,
            Version = 1,
            WorldId = world.Id,
        };

        Assert.That(world.IsEntityValid(invalid), Is.False);
        Assert.That(world.HasComponentA(invalid), Is.False);
        Assert.That(world.HasComponentAAndComponentB(invalid), Is.False);
        Assert.That(world.HasAnyComponentAOrComponentB(invalid), Is.False);
    }

    [Test]
    public void StaleEntityHasNoComponents() {
        var world = TestWorlds.Get("world");

        var stale = world.CreateEntity(new ComponentA(), new ComponentB());
        stale.Destroy();
        var replacement = world.CreateEntity(new ComponentA());

        Assert.That(world.IsEntityValid(stale), Is.False);
        Assert.That(world.HasComponentA(stale), Is.False);
        Assert.That(world.HasComponentAAndComponentB(stale), Is.False);
        Assert.That(world.HasAnyComponentAOrComponentB(stale), Is.False);

        replacement.Destroy();
    }
}
