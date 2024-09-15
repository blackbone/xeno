namespace Xeno.Tests;

[TestFixture]
public class WorldApiCreationTests {
    [Test]
    public void World_Create() {
        var world = Worlds.Create("world");
        world.Dispose();
    }
}

[TestFixture]
public class WorldEntityCreationTests {
    [SetUp]
    public void OneTimeSetUp() => Worlds.Create("world");

    [TearDown]
    public void OneTimeTearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void CreateEntity() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity();

        Assert.That(e.IsValid());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_1() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity(new Component1 { Value = 1 });

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(e.HasComponent<Component1>());

        e.RemoveComponents<Component1>();

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!e.HasComponent<Component1>());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_2() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 }
            );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(e.HasAllComponents<Component1, Component2>());

        e.RemoveComponents<Component1>();

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!e.HasAllComponents<Component1, Component2>());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_3() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 },
            new Component3 { Value = 3 }
        );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(e.HasAllComponents<Component1, Component2, Component3>());

        e.RemoveComponents<Component1>();

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!e.HasAllComponents<Component1, Component2, Component3>());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_4() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 },
            new Component3 { Value = 3 },
            new Component4 { Value = 4 }
        );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(e.HasAllComponents<Component1, Component2, Component3, Component4>());

        e.RemoveComponents<Component1>();

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!e.HasAllComponents<Component1, Component2, Component3, Component4>());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }
}