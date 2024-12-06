using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xeno.Tests;

[TestFixture]
public class StressTests_LargeScaleOperations {
    [SetUp]
    public void SetUp() => Worlds.Create("world");

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void CreateAndDeleteMillionEntities_PerformanceTest() {
        Worlds.TryGet("world", out var world);
        const int entityCount = 1_000_000;

        var entities = new Entity[entityCount];

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
        }
        stopwatch.Stop();
        Console.WriteLine($"Created {entityCount} entities in {stopwatch.ElapsedMilliseconds}ms");

        stopwatch.Restart();
        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
        stopwatch.Stop();
        Console.WriteLine($"Destroyed {entityCount} entities in {stopwatch.ElapsedMilliseconds}ms");

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void CreateAndRemoveComponentsInLoop_StabilityCheck() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity();

        for (var i = 0; i < 10_000; i++) {
            world.AddComponents(e, new ComponentA());
            world.RemoveComponents<ComponentA>(e);
        }

        e.Destroy();
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void RemoveEntitiesInRandomOrder_NoCorruption() {
        Worlds.TryGet("world", out var world);
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
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity();

        for (int i = 0; i < 5_000; i++) {
            world.AddComponents(e, new ComponentA());
            world.RemoveComponents<ComponentA>(e);
        }

        Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0));
        e.Destroy();
    }

    [Test]
    public void RapidEntityCreationDeletion_ParallelThreads() {
        Worlds.TryGet("world", out var world);
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

        Task.WaitAll(tasks);
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void AllocateLargeEntities_VerifyMemoryCapacity() {
        Worlds.TryGet("world", out var world);
        const int entityCount = 500_000;

        var entities = new Entity[entityCount];
        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
            world.AddComponents(entities[i], new LargeComponent());
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));

        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
    }

    [Test]
    public void MassComponentAdditionRemoval_DoesNotCorruptArchetypeLayout() {
        Worlds.TryGet("world", out var world);
        var e = world.CreateEntity();

        for (int i = 0; i < 10_000; i++) {
            world.AddComponents(e, new ComponentA(), new ComponentB());
            world.RemoveComponents<ComponentA, ComponentB>(e);
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        e.Destroy();
    }

    [Test]
    public void MultiThreadedEntityMovementBetweenArchetypes() {
        Worlds.TryGet("world", out var world);
        const int entityCount = 100_000;
        var entities = new Entity[entityCount];

        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
        }

        var tasks = new Task[4];
        for (int t = 0; t < 4; t++) {
            tasks[t] = Task.Run(() => {
                for (int i = 0; i < entityCount / 4; i++) {
                    world.AddComponents(entities[i], new ComponentA());
                    world.RemoveComponents<ComponentA>(entities[i]);
                }
            });
        }

        Task.WaitAll(tasks);

        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(entityCount));

        for (int i = 0; i < entityCount; i++) {
            entities[i].Destroy();
        }
    }

    [Test]
    public void Benchmark_AddRemoveEntities_ScaleTest() {
        Worlds.TryGet("world", out var world);
        const int iterations = 1_000_000;

        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) {
            var e = world.CreateEntity();
            e.Destroy();
        }
        stopwatch.Stop();

        Console.WriteLine($"Processed {iterations} entities in {stopwatch.ElapsedMilliseconds}ms");
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
    }

    [Test]
    public void ECSFragmentationUnderHeavyUsage() {
        Worlds.TryGet("world", out var world);
        const int entityCount = 500_000;
        var entities = new Entity[entityCount];

        for (int i = 0; i < entityCount; i++) {
            entities[i] = world.CreateEntity();
            if (i % 2 == 0) world.AddComponents(entities[i], new ComponentA());
        }

        for (int i = 0; i < entityCount; i++) {
            if (i % 3 == 0) entities[i].Destroy();
        }

        for (int i = 0; i < entityCount; i++) {
            if (i % 5 == 0) world.AddComponents(entities[i], new ComponentB());
        }

        Assert.That(world.zeroArchetype.entitiesCount, Is.LessThan(entityCount));
    }
}
