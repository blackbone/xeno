using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class StressTests_LargeScaleOperations {
    [SetUp]
    public void SetUp() => TestWorlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void CreateAndDeleteMillionEntities_PerformanceTest() {
        var world = TestWorlds.Get("world");
        const int entityCount = 1_000_000;

        var entities = new Entity[entityCount];

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
        }
        stopwatch.Stop();

        stopwatch.Restart();
        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
        stopwatch.Stop();

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void CreateAndRemoveComponentsInLoop_StabilityCheck() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity();

        for (var i = 0; i < 10_000; i++) {
            world.Add(e, new ComponentA());
            world.RemoveComponentA(e);
        }

        e.Destroy();
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void RemoveEntitiesInRandomOrder_NoCorruption() {
        var world = TestWorlds.Get("world");
        const int entityCount = 10_000;
        var entities = new List<Entity>();

        for (var i = 0; i < entityCount; i++) {
            entities.Add(world.CreateEntity());
        }

        var rnd = new Random();
        entities = entities.OrderBy(_ => rnd.Next()).ToList();

        foreach (var e in entities) {
            var re = e;
            re.Destroy();
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void ContinuousComponentAdditionRemoval_ValidateIndices() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity();

        for (int i = 0; i < 5_000; i++) {
            world.Add(e, new ComponentA());
            world.RemoveComponentA(e);
        }

        Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0));
        e.Destroy();
    }

    [Test]
    public void RapidEntityCreationDeletion_ParallelThreads() {
        var world = TestWorlds.Get("world");
        const int threadCount = 4;
        const int entitiesPerThread = 250_000;

        var tasks = new Task[threadCount];
        for (int t = 0; t < threadCount; t++) {
            tasks[t] = Task.Run(() => {
                var entities = new Entity[entitiesPerThread];
                for (int i = 0; i < entitiesPerThread; i++) {
                    entities[i] = world.CreateEntity();
                }
                for (int i = 0; i < entitiesPerThread; i++) {
                    entities[i].Destroy();
                }
            });
        }

#if XENO_OWNER_THREAD_GUARD_ASSERTS
        var exception = Assert.Throws<AggregateException>(() => Task.WaitAll(tasks));
        Assert.That(exception!.Flatten().InnerExceptions.Any(ex => ex is InvalidOperationException));
#else
        Task.WaitAll(tasks);
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
#endif
    }

    [Test]
    public void AllocateLargeEntities_VerifyMemoryCapacity() {
        var world = TestWorlds.Get("world");
        const int entityCount = 500_000;

        var entities = new Entity[entityCount];
        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
            world.Add(entities[i], new LargeComponent());
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));

        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
    }

    [Test]
    public void MassComponentAdditionRemoval_DoesNotCorruptArchetypeLayout() {
        var world = TestWorlds.Get("world");
        var e = world.CreateEntity();

        for (int i = 0; i < 10_000; i++) {
            world.Add(e, new ComponentA());
            world.Add(e, new ComponentB());
            world.RemoveComponentAAndComponentB(e);
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        e.Destroy();
    }

    [Test]
    public void MultiThreadedEntityMovementBetweenArchetypes() {
        var world = TestWorlds.Get("world");
        const int entityCount = 100_000;
        var entities = new Entity[entityCount];

        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
        }

        var tasks = new Task[4];
        for (int t = 0; t < 4; t++) {
            tasks[t] = Task.Run(() => {
                for (int i = 0; i < entityCount / 4; i++) {
                    world.Add(entities[i], new ComponentA());
                    world.RemoveComponentA(entities[i]);
                }
            });
        }

#if XENO_OWNER_THREAD_GUARD_ASSERTS
        var exception = Assert.Throws<AggregateException>(() => Task.WaitAll(tasks));
        Assert.That(exception!.Flatten().InnerExceptions.Any(ex => ex is InvalidOperationException));
#else
        Task.WaitAll(tasks);
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(entityCount));
#endif

        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
    }

    [Test]
    public void Benchmark_AddRemoveEntities_ScaleTest() {
        var world = TestWorlds.Get("world");
        const int iterations = 1_000_000;

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) {
            var e = world.CreateEntity();
            e.Destroy();
        }
        stopwatch.Stop();
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void ECSFragmentationUnderHeavyUsage() {
        var world = TestWorlds.Get("world");
        const int entityCount = 500_000;
        var entities = new Entity[entityCount];

        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
            if (i % 2 == 0) world.Add(entities[i], new ComponentA());
        }

        for (int i = 0; i < entityCount; i++) {
            if (i % 3 == 0) entities[i].Destroy();
        }

        for (int i = 0; i < entityCount; i++) {
            if (i % 5 == 0) world.Add(entities[i], new ComponentB());
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.LessThan(entityCount));
    }
}
