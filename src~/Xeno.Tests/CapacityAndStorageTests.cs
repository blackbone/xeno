namespace Xeno.Tests;

[TestFixture]
public class CapacityAndStorageTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EnsureCapacity_PreallocatesEntityStorageWithoutChangingLivingEntities() {
        Worlds.TryGet("world", out var world);

        world.EnsureCapacity(4096);

        Assert.That(world.entities.Length, Is.GreaterThanOrEqualTo(4096));
        Assert.That(world.EntityCount, Is.Zero);

        var entities = new Entity[130];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity(new ComponentA { Value = i }, new ComponentB { Value = i * 2 });

        world.EnsureCapacity(8192);

        Assert.That(world.entities.Length, Is.GreaterThanOrEqualTo(8192));
        Assert.That(world.EntityCount, Is.EqualTo(entities.Length));
        Assert.That(world.Count<ComponentA, ComponentB>(), Is.EqualTo(entities.Length));

        for (var i = 0; i < entities.Length; i++) {
            var a = default(ComponentA);
            var b = default(ComponentB);

            Assert.That(world.RefComponentsAll(entities[i], ref a, ref b), Is.True);
            Assert.That(a.Value, Is.EqualTo(i));
            Assert.That(b.Value, Is.EqualTo(i * 2));
        }
    }

    [Test]
    public void DenseEntityIds_PreserveDistinctComponentSlotsAcrossStorePages() {
        Worlds.TryGet("world", out var world);

        var entities = new Entity[130];
        for (var i = 0; i < entities.Length; i++)
            entities[i] = world.CreateEntity(new ComponentA { Value = i + 10 });

        for (var i = 0; i < entities.Length; i++) {
            var a = default(ComponentA);

            Assert.That(entities[i].RefComponents(ref a), Is.True);
            Assert.That(a.Value, Is.EqualTo(i + 10));
        }
    }

    [Test]
    public void ReferenceComponents_AreAcceptedByRuntimeWorldStorage() {
        Worlds.TryGet("world", out var world);

        var component = new RuntimeReferenceComponent { Value = 17 };
        var entity = world.CreateEntity(component);

        Assert.That(world.Count<RuntimeReferenceComponent>(), Is.EqualTo(1));
        Assert.That(world.HasComponent<RuntimeReferenceComponent>(entity), Is.True);
        Assert.That(world.Ref<RuntimeReferenceComponent>(entity), Is.SameAs(component));

        world.Ref<RuntimeReferenceComponent>(entity).Value = 23;

        Assert.That(component.Value, Is.EqualTo(23));
    }
}

public sealed class RuntimeReferenceComponent {
    public int Value;
}
