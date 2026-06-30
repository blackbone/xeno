namespace Xeno.Tests;

[TestFixture]
public class GeneratedWorldContractTests {
    [SetUp]
    public void SetUp() {
        GeneratedWorldLifecycleSystem.Reset();
        GeneratedWorldStaticUpdateSystem.Reset();
        GeneratedWorldMoveSystem.Reset();
        GeneratedWorldReferenceSystem.Reset();
    }

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("generated-world", out var world))
            world.Dispose();
    }

    [Test]
    public void GeneratedWorldRunsLifecycleAndUpdatesWithoutRuntimeSystemRegistration() {
        var world = new GeneratedCodegenWorld("generated-world", "life");
        var matching = world.CreateEntity(new ComponentA { Value = 3 }, new ComponentB { Value = 4 });
        var padding = world.CreateEntity(new ComponentA { Value = 10 });
        var reference = new GeneratedReferenceComponent { Value = 7 };
        world.CreateEntity(reference);

        world.Start();
        world.Tick(5f);
        world.Stop();

        Assert.That(world.Ticks, Is.EqualTo(1));
        Assert.That(GeneratedWorldLifecycleSystem.Calls, Is.EqualTo(new[] {
            "life-start-1",
            "life-start-2",
            "life-stop",
        }));
        Assert.That(GeneratedWorldStaticUpdateSystem.Values, Is.EquivalentTo(new[] { 6, 20 }));
        Assert.That(GeneratedWorldMoveSystem.Values, Is.EqualTo(new[] { 15 }));
        Assert.That(reference.Value, Is.EqualTo(18));
        Assert.That(GeneratedWorldReferenceSystem.Calls, Is.EqualTo(1));
    }
}

[RegisterComponent(typeof(ComponentA))]
[RegisterSystem(typeof(GeneratedWorldLifecycleSystem))]
[RegisterSystem(typeof(GeneratedWorldStaticUpdateSystem))]
[RegisterSystem(typeof(GeneratedWorldMoveSystem))]
[RegisterSystem(typeof(GeneratedWorldReferenceSystem))]
public partial class GeneratedCodegenWorld : World {
}

public sealed class GeneratedWorldLifecycleSystem {
    private readonly string prefix;
    public static readonly List<string> Calls = new();

    public GeneratedWorldLifecycleSystem(string prefix) {
        this.prefix = prefix;
    }

    public static void Reset() => Calls.Clear();

    [SystemMethod(SystemMethodType.Startup, 2)]
    public void Start2() => Calls.Add($"{prefix}-start-2");

    [SystemMethod(SystemMethodType.Startup, 1)]
    public void Start1() => Calls.Add($"{prefix}-start-1");

    [SystemMethod(SystemMethodType.Shutdown)]
    public void Stop() => Calls.Add($"{prefix}-stop");
}

public static class GeneratedWorldStaticUpdateSystem {
    public static readonly List<int> Values = new();

    public static void Reset() => Values.Clear();

    [SystemMethod(SystemMethodType.Update, 1)]
    public static void DoubleA(ref ComponentA a) {
        a.Value *= 2;
        Values.Add(a.Value);
    }
}

public sealed class GeneratedWorldMoveSystem {
    public static readonly List<int> Values = new();

    public static void Reset() => Values.Clear();

    [SystemMethod(SystemMethodType.Update, 2)]
    public void Move(in Entity entity, [Uniform(true)] in float delta, ref ComponentA a, ref ComponentB b) {
        Assert.That(entity.Id, Is.GreaterThanOrEqualTo(0));
        a.Value += b.Value + (int)delta;
        Values.Add(a.Value);
    }
}

public sealed class GeneratedReferenceComponent {
    public int Value;
}

public static class GeneratedWorldReferenceSystem {
    public static int Calls;

    public static void Reset() => Calls = 0;

    [SystemMethod(SystemMethodType.Update, 3)]
    public static void Bump(ref GeneratedReferenceComponent component) {
        component.Value += 11;
        Calls++;
    }
}
