using System.Runtime.CompilerServices;

namespace Xeno.Tests;

[TestFixture]
public class FreeListAndBufferResizingTests {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EntityReuseAlwaysReturnsLowestAvailableID() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e2.Destroy(); // Free an entity ID
        var e4 = world.CreateEntity(); // Should reuse e2's ID

        Assert.That(e4.Id, Is.EqualTo(e2.Id));

        e1.Destroy();
        e3.Destroy();
        e4.Destroy();
    }

    [Test]
    public void ResizingFreeIdsDoesNotCorruptExistingFreeIds() {
        Worlds.TryGet("world", out var world);

        var initialCapacity = world.freeIds.Length;
        Span<Entity> entities = stackalloc Entity[initialCapacity + 10];
        for (var i = 0; i < initialCapacity + 10; i++) {
            entities[i] = world.CreateEntity();
        }

        Assert.That(world.freeIds.Length, Is.GreaterThanOrEqualTo(initialCapacity));

        foreach (var e in entities) {
            var re = e;
            re.Destroy();
        }

        Assert.That(world.freeIds.Length, Is.GreaterThanOrEqualTo(initialCapacity));
    }

    [Test]
    public void FreeIdsMaintainReverseOrderForCorrectReuse() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e1.Destroy();
        e2.Destroy();
        e3.Destroy();

        var e4 = world.CreateEntity();
        var e5 = world.CreateEntity();
        var e6 = world.CreateEntity();

        Assert.That(e4.Id, Is.EqualTo(e3.Id));
        Assert.That(e5.Id, Is.EqualTo(e2.Id));
        Assert.That(e6.Id, Is.EqualTo(e1.Id));
    }

    [Test]
    public void InitEmptyEntitiesInternalCorrectlyFillsFreeIdsInReverseOrder() {
        Worlds.TryGet("world", out var world);

        var prev = world.CreateEntity();
        for (int i = 0; i < 32; i++) {
            var e = world.CreateEntity();
            Assert.That(e.Id, Is.GreaterThan(prev.Id));
            prev = e;
        }
    }

    [Test]
    public void ResizingWhenEntityCountExceedsInitialCapacity() {
        Worlds.TryGet("world", out var world);

        var initialCapacity = world.entities.Length;

        for (int i = 0; i < initialCapacity + 10; i++) {
            world.CreateEntity();
        }

        Assert.That(world.entities.Length, Is.GreaterThan(initialCapacity));
    }

    [Test]
    public void AddingRemovingEntitiesDoesNotCauseIndexFragmentation() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e2.Destroy();
        var e4 = world.CreateEntity();
        var e5 = world.CreateEntity();

        Assert.That(e4.Id, Is.EqualTo(e2.Id));
        Assert.That(e5.Id, Is.EqualTo(3)); // Next available ID
    }

    [Test]
    public void EntitiesAreReusedInLIFOOrder() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var e2 = world.CreateEntity();
        var e3 = world.CreateEntity();

        e3.Destroy();
        e2.Destroy();
        e1.Destroy();

        var e4 = world.CreateEntity();
        var e5 = world.CreateEntity();
        var e6 = world.CreateEntity();

        Assert.That(e4.Id, Is.EqualTo(e1.Id));
        Assert.That(e5.Id, Is.EqualTo(e2.Id));
        Assert.That(e6.Id, Is.EqualTo(e3.Id));
    }

    [Test]
    public void DeletingEntityAndImmediatelyReaddingDoesNotCreateDuplicates() {
        Worlds.TryGet("world", out var world);

        var e1 = world.CreateEntity();
        var id = e1.Id;

        e1.Destroy();
        var e2 = world.CreateEntity();

        Assert.That(e2.Id, Is.EqualTo(id)); // Should reuse the same ID
    }

    [Test]
    public void DeletingAllEntitiesAndRecreatingFillsGapsCorrectly() {
        Worlds.TryGet("world", out var world);

        var entities = new List<Entity>();

        for (int i = 0; i < 100; i++) {
            entities.Add(world.CreateEntity());
        }

        foreach (var e in entities) {
            var re = e;
            re.Destroy();
        }

        for (int i = 0; i < 100; i++) {
            var e = world.CreateEntity();
            Assert.That(e.Id, Is.EqualTo((uint)(99 - i))); // Should be reusing IDs in reverse order
        }
    }

    [Test]
    public void ResizingBehaviorWhenComponentsExceedBitmaskLimits() {
        Worlds.TryGet("world", out var world);

        var e = world.CreateEntity();
        e.AddComponents(default(Component_1));
        e.AddComponents(default(Component_2));
        e.AddComponents(default(Component_3));
        e.AddComponents(default(Component_4));
        e.AddComponents(default(Component_5));
        e.AddComponents(default(Component_6));
        e.AddComponents(default(Component_7));
        e.AddComponents(default(Component_8));
        e.AddComponents(default(Component_9));
        e.AddComponents(default(Component_10));
        e.AddComponents(default(Component_11));
        e.AddComponents(default(Component_12));
        e.AddComponents(default(Component_13));
        e.AddComponents(default(Component_14));
        e.AddComponents(default(Component_15));
        e.AddComponents(default(Component_16));
        e.AddComponents(default(Component_17));
        e.AddComponents(default(Component_18));
        e.AddComponents(default(Component_19));
        e.AddComponents(default(Component_20));
        e.AddComponents(default(Component_21));
        e.AddComponents(default(Component_22));
        e.AddComponents(default(Component_23));
        e.AddComponents(default(Component_24));
        e.AddComponents(default(Component_25));
        e.AddComponents(default(Component_26));
        e.AddComponents(default(Component_27));
        e.AddComponents(default(Component_28));
        e.AddComponents(default(Component_29));
        e.AddComponents(default(Component_30));
        e.AddComponents(default(Component_31));
        e.AddComponents(default(Component_32));
        e.AddComponents(default(Component_33));
        e.AddComponents(default(Component_34));
        e.AddComponents(default(Component_35));
        e.AddComponents(default(Component_36));
        e.AddComponents(default(Component_37));
        e.AddComponents(default(Component_38));
        e.AddComponents(default(Component_39));
        e.AddComponents(default(Component_40));
        e.AddComponents(default(Component_41));
        e.AddComponents(default(Component_42));
        e.AddComponents(default(Component_43));
        e.AddComponents(default(Component_44));
        e.AddComponents(default(Component_45));
        e.AddComponents(default(Component_46));
        e.AddComponents(default(Component_47));
        e.AddComponents(default(Component_48));
        e.AddComponents(default(Component_49));
        e.AddComponents(default(Component_50));
        e.AddComponents(default(Component_51));
        e.AddComponents(default(Component_52));
        e.AddComponents(default(Component_53));
        e.AddComponents(default(Component_54));
        e.AddComponents(default(Component_55));
        e.AddComponents(default(Component_56));
        e.AddComponents(default(Component_57));
        e.AddComponents(default(Component_58));
        e.AddComponents(default(Component_59));
        e.AddComponents(default(Component_60));
        e.AddComponents(default(Component_61));
        e.AddComponents(default(Component_62));
        e.AddComponents(default(Component_63));
        e.AddComponents(default(Component_64));
        e.AddComponents(default(Component_65));
        e.AddComponents(default(Component_66));
        e.AddComponents(default(Component_67));
        e.AddComponents(default(Component_68));
        e.AddComponents(default(Component_69));
        e.AddComponents(default(Component_70));
        e.AddComponents(default(Component_71));
        e.AddComponents(default(Component_72));
        e.AddComponents(default(Component_73));
        e.AddComponents(default(Component_74));
        e.AddComponents(default(Component_75));
        e.AddComponents(default(Component_76));
        e.AddComponents(default(Component_77));
        e.AddComponents(default(Component_78));
        e.AddComponents(default(Component_79));
        e.AddComponents(default(Component_80));
        e.AddComponents(default(Component_81));
        e.AddComponents(default(Component_82));
        e.AddComponents(default(Component_83));
        e.AddComponents(default(Component_84));
        e.AddComponents(default(Component_85));
        e.AddComponents(default(Component_86));
        e.AddComponents(default(Component_87));
        e.AddComponents(default(Component_88));
        e.AddComponents(default(Component_89));
        e.AddComponents(default(Component_90));
        e.AddComponents(default(Component_91));
        e.AddComponents(default(Component_92));
        e.AddComponents(default(Component_93));
        e.AddComponents(default(Component_94));
        e.AddComponents(default(Component_95));
        e.AddComponents(default(Component_96));
        e.AddComponents(default(Component_97));
        e.AddComponents(default(Component_98));
        e.AddComponents(default(Component_99));
        e.AddComponents(default(Component_100));

        var archetype = world.entityArchetypes[e.Id];
        Assert.That(archetype.mask.GetIndices().Count(), Is.EqualTo(100));

        e.Destroy();
    }
}
