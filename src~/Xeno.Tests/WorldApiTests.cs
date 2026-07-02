using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class WorldApiCreationTests {
    [Test]
    public void World_Create() {
        var world = TestWorlds.Create("world");
        world.Dispose();
    }
}

[TestFixture]
public class WorldEntityCreationTests {
    [SetUp]
    public void OneTimeSetUp() => TestWorlds.Create("world");

    [TearDown]
    public void OneTimeTearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void CreateEntity() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity();

        Assert.That(e.IsValid());

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_1() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity(new Component1 { Value = 1 });

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(world.HasComponent1(e));

        world.RemoveComponent1(e);

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!world.HasComponent1(e));

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_2() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 }
            );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(world.HasComponent1AndComponent2(e));

        world.RemoveComponent1(e);

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!world.HasComponent1AndComponent2(e));

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_3() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 },
            new Component3 { Value = 3 }
        );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(world.HasComponent1AndComponent2AndComponent3(e));

        world.RemoveComponent1(e);

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!world.HasComponent1AndComponent2AndComponent3(e));

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CreateEntity_4() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity(
            new Component1 { Value = 1 },
            new Component2 { Value = 2 },
            new Component3 { Value = 3 },
            new Component4 { Value = 4 }
        );

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(world.HasComponent1AndComponent2AndComponent3AndComponent4(e));

        world.RemoveComponent1(e);

        Assert.That(e.IsValid());
        Assert.That(world.EntityCount, Is.EqualTo(1));
        Assert.That(!world.HasComponent1AndComponent2AndComponent3AndComponent4(e));

        e.Destroy();

        Assert.That(!e.IsValid());
        Assert.That(world.EntityCount, Is.Zero);
    }

    [Test]
    public void CountQueriesReflectArchetypeChangesAfterCaching() {
        var world = TestWorlds.Get("world");

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1(), Is.EqualTo(0));

        var a = world.CreateEntity(new Component1 { Value = 1 });
        var b = world.CreateEntity(new Component2 { Value = 2 });

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));

        world.Add(b, new Component1 { Value = 3 });

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));

        world.RemoveComponent1(a);
        world.RemoveComponent1(b);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
    }
}
