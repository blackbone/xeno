using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class CapacityAndStorageTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EnsureCapacity_PreallocatesEntityStorageWithoutChangingLivingEntities() {
        var world = TestWorlds.Get("world");

        world.EnsureCapacity(4096);

        Assert.That(world.entities.Length, Is.GreaterThanOrEqualTo(4096));
        Assert.That(world.EntityCount, Is.Zero);

        var entities = new Entity[130];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity(new ComponentA { Value = i }, new ComponentB { Value = i * 2 });

        world.EnsureCapacity(8192);

        Assert.That(world.entities.Length, Is.GreaterThanOrEqualTo(8192));
        Assert.That(world.EntityCount, Is.EqualTo(entities.Length));
        Assert.That(world.CountComponentAAndComponentB(), Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++) {
            Assert.That(world.TryGetComponentA(entities[i], out var a), Is.True);
            Assert.That(world.TryGetComponentB(entities[i], out var b), Is.True);
            Assert.That(a.Value, Is.EqualTo(i));
            Assert.That(b.Value, Is.EqualTo(i * 2));
        }
    }

    [Test]
    public void DenseEntityIds_PreserveDistinctComponentSlotsAcrossStorePages() {
        var world = TestWorlds.Get("world");

        var entities = new Entity[130];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity(new ComponentA { Value = i + 10 });

        for (var i = 0; i < entities.Length; i++) {
            Assert.That(world.TryGetComponentA(entities[i], out var a), Is.True);
            Assert.That(a.Value, Is.EqualTo(i + 10));
        }
    }

    [Test]
    public void ReferenceComponents_AreAcceptedByRuntimeWorldStorage() {
        var world = TestWorlds.Get("world");

        var component = new RuntimeReferenceComponent { Value = 17 };
        var entity = world.CreateEntity(component);

        Assert.That(world.CountRuntimeReferenceComponent(), Is.EqualTo(1));
        Assert.That(world.HasRuntimeReferenceComponent(entity), Is.True);
        Assert.That(world.RefRuntimeReferenceComponent(entity), Is.SameAs(component));

        world.RefRuntimeReferenceComponent(entity).Value = 23;

        Assert.That(component.Value, Is.EqualTo(23));
    }
}

public sealed class RuntimeReferenceComponent {
    public int Value;
}
