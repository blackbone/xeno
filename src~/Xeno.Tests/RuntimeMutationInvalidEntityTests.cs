namespace Xeno.Tests;

[TestFixture]
public class RuntimeMutationInvalidEntityTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void StaleEntityCannotAddComponentsToReusedEntitySlot() {
        Worlds.TryGet("world", out var world);

        var stale = world.CreateEntity();
        stale.Destroy();
        var replacement = world.CreateEntity();

        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(world.IsEntityValid(stale), Is.False);

        world.AddComponents(stale, new ComponentA { Value = 10 });

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.HasComponent<ComponentA>(replacement), Is.False);

        replacement.Destroy();
    }

    [Test]
    public void StaleEntityCannotRemoveComponentsFromReusedEntitySlot() {
        Worlds.TryGet("world", out var world);

        var stale = world.CreateEntity();
        stale.Destroy();
        var replacement = world.CreateEntity(new ComponentA { Value = 10 });

        Assert.That(replacement.Id, Is.EqualTo(stale.Id));
        Assert.That(world.IsEntityValid(stale), Is.False);

        world.RemoveComponents<ComponentA>(stale);

        Assert.That(world.IsEntityValid(replacement), Is.True);
        Assert.That(world.HasComponent<ComponentA>(replacement), Is.True);
        Assert.That(world.Ref<ComponentA>(replacement).Value, Is.EqualTo(10));

        replacement.Destroy();
    }

    [Test]
    public void StaleEntityCannotDestroyReusedEntitySlot() {
        Worlds.TryGet("world", out var world);

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
        Worlds.TryGet("world", out var world);
        var invalid = new Entity {
            Id = uint.MaxValue,
            Version = 1,
            WorldId = world.Id,
        };

        Assert.That(world.IsEntityValid(invalid), Is.False);
        Assert.That(world.HasComponent<ComponentA>(invalid), Is.False);
        Assert.That(world.HasAllComponents<ComponentA, ComponentB>(invalid), Is.False);
        Assert.That(world.HasAnyComponents<ComponentA, ComponentB>(invalid), Is.False);
    }

    [Test]
    public void StaleEntityHasNoComponents() {
        Worlds.TryGet("world", out var world);

        var stale = world.CreateEntity(new ComponentA(), new ComponentB());
        stale.Destroy();
        var replacement = world.CreateEntity(new ComponentA());

        Assert.That(world.IsEntityValid(stale), Is.False);
        Assert.That(world.HasComponent<ComponentA>(stale), Is.False);
        Assert.That(world.HasAllComponents<ComponentA, ComponentB>(stale), Is.False);
        Assert.That(world.HasAnyComponents<ComponentA, ComponentB>(stale), Is.False);

        replacement.Destroy();
    }
}
