using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class FreeListAndBufferResizingTests {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void EntityReuseAlwaysReturnsLowestAvailableID() {
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

        var prev = world.CreateEntity();
        for (int i = 0; i < 32; i++) {
            var e = world.CreateEntity();
            Assert.That(e.Id, Is.GreaterThan(prev.Id));
            prev = e;
        }
    }

    [Test]
    public void ResizingWhenEntityCountExceedsInitialCapacity() {
        var world = TestWorlds.Get("world");

        var initialCapacity = world.entities.Length;

        for (int i = 0; i < initialCapacity + 10; i++) {
            world.CreateEntity();
        }

        Assert.That(world.entities.Length, Is.GreaterThan(initialCapacity));
    }

    [Test]
    public void AddingRemovingEntitiesDoesNotCauseIndexFragmentation() {
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();
        var id = e1.Id;

        e1.Destroy();
        var e2 = world.CreateEntity();

        Assert.That(e2.Id, Is.EqualTo(id)); // Should reuse the same ID
    }

    [Test]
    public void DeletingAllEntitiesAndRecreatingFillsGapsCorrectly() {
        var world = TestWorlds.Get("world");

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
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();
        world.Add(e, default(Component_1));
        world.Add(e, default(Component_2));
        world.Add(e, default(Component_3));
        world.Add(e, default(Component_4));
        world.Add(e, default(Component_5));
        world.Add(e, default(Component_6));
        world.Add(e, default(Component_7));
        world.Add(e, default(Component_8));
        world.Add(e, default(Component_9));
        world.Add(e, default(Component_10));
        world.Add(e, default(Component_11));
        world.Add(e, default(Component_12));
        world.Add(e, default(Component_13));
        world.Add(e, default(Component_14));
        world.Add(e, default(Component_15));
        world.Add(e, default(Component_16));
        world.Add(e, default(Component_17));
        world.Add(e, default(Component_18));
        world.Add(e, default(Component_19));
        world.Add(e, default(Component_20));
        world.Add(e, default(Component_21));
        world.Add(e, default(Component_22));
        world.Add(e, default(Component_23));
        world.Add(e, default(Component_24));
        world.Add(e, default(Component_25));
        world.Add(e, default(Component_26));
        world.Add(e, default(Component_27));
        world.Add(e, default(Component_28));
        world.Add(e, default(Component_29));
        world.Add(e, default(Component_30));
        world.Add(e, default(Component_31));
        world.Add(e, default(Component_32));
        world.Add(e, default(Component_33));
        world.Add(e, default(Component_34));
        world.Add(e, default(Component_35));
        world.Add(e, default(Component_36));
        world.Add(e, default(Component_37));
        world.Add(e, default(Component_38));
        world.Add(e, default(Component_39));
        world.Add(e, default(Component_40));
        world.Add(e, default(Component_41));
        world.Add(e, default(Component_42));
        world.Add(e, default(Component_43));
        world.Add(e, default(Component_44));
        world.Add(e, default(Component_45));
        world.Add(e, default(Component_46));
        world.Add(e, default(Component_47));
        world.Add(e, default(Component_48));
        world.Add(e, default(Component_49));
        world.Add(e, default(Component_50));
        world.Add(e, default(Component_51));
        world.Add(e, default(Component_52));
        world.Add(e, default(Component_53));
        world.Add(e, default(Component_54));
        world.Add(e, default(Component_55));
        world.Add(e, default(Component_56));
        world.Add(e, default(Component_57));
        world.Add(e, default(Component_58));
        world.Add(e, default(Component_59));
        world.Add(e, default(Component_60));
        world.Add(e, default(Component_61));
        world.Add(e, default(Component_62));
        world.Add(e, default(Component_63));
        world.Add(e, default(Component_64));
        world.Add(e, default(Component_65));
        world.Add(e, default(Component_66));
        world.Add(e, default(Component_67));
        world.Add(e, default(Component_68));
        world.Add(e, default(Component_69));
        world.Add(e, default(Component_70));
        world.Add(e, default(Component_71));
        world.Add(e, default(Component_72));
        world.Add(e, default(Component_73));
        world.Add(e, default(Component_74));
        world.Add(e, default(Component_75));
        world.Add(e, default(Component_76));
        world.Add(e, default(Component_77));
        world.Add(e, default(Component_78));
        world.Add(e, default(Component_79));
        world.Add(e, default(Component_80));
        world.Add(e, default(Component_81));
        world.Add(e, default(Component_82));
        world.Add(e, default(Component_83));
        world.Add(e, default(Component_84));
        world.Add(e, default(Component_85));
        world.Add(e, default(Component_86));
        world.Add(e, default(Component_87));
        world.Add(e, default(Component_88));
        world.Add(e, default(Component_89));
        world.Add(e, default(Component_90));
        world.Add(e, default(Component_91));
        world.Add(e, default(Component_92));
        world.Add(e, default(Component_93));
        world.Add(e, default(Component_94));
        world.Add(e, default(Component_95));
        world.Add(e, default(Component_96));
        world.Add(e, default(Component_97));
        world.Add(e, default(Component_98));
        world.Add(e, default(Component_99));
        world.Add(e, default(Component_100));

        var archetype = world.entityArchetypes[e.Id];
        Assert.That(archetype.mask.GetIndices().Count(), Is.EqualTo(100));

        e.Destroy();
    }
}
