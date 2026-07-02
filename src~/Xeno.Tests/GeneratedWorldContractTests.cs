using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class GeneratedWorldContractTests {
    [SetUp]
    public void SetUp() {
        GeneratedWorldLifecycleSystem.Reset();
        GeneratedWorldStaticUpdateSystem.Reset();
        GeneratedWorldMoveSystem.Reset();
        GeneratedWorldReferenceSystem.Reset();
        GeneratedWorldPipelineSystem.Reset();
        GeneratedWorldPreMutationSystem.Reset();
        GeneratedWorldUpdateMutationSystem.Reset();
        GeneratedWorldPostMutationSystem.Reset();
        GeneratedPureFirstSystem.Reset();
        GeneratedPureSecondSystem.Reset();
        GeneratedInlinePrimitiveSystem.Reset();
        GeneratedBakeQuerySystem.Reset();
    }

    [TearDown]
    public void TearDown() {
        if (Worlds.TryGet("generated-world", out var world))
            world.Dispose();
        if (Worlds.TryGet("generated-stage-world", out var stageWorld))
            stageWorld.Dispose();
        if (Worlds.TryGet("generated-pure-world", out var pureWorld))
            pureWorld.Dispose();
        if (Worlds.TryGet("generated-inline-world", out var inlineWorld))
            inlineWorld.Dispose();
        if (Worlds.TryGet("generated-bake-query-world", out var bakeQueryWorld))
            bakeQueryWorld.Dispose();
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

    [Test]
    public void GeneratedWorldSystemsRunAcrossStagesWithCorrectUniformsAndMaskFiltering() {
        var world = new GeneratedStageWorld("generated-stage-world");
        var matching = world.CreateEntity(new ComponentA { Value = 1 }, new ComponentB { Value = 3 });
        var padding = world.CreateEntity(new ComponentA { Value = 5 });

        world.Start();
        world.Tick(4f);
        world.Tick(2f);

        Assert.That(world.Ticks, Is.EqualTo(2));
        Assert.That(GeneratedWorldPipelineSystem.Stages, Is.EqualTo(new[] {
            "pre:4",
            "update:4",
            "post:4",
            "pre:2",
            "update:2",
            "post:2",
        }));

        Assert.That(GeneratedWorldPreMutationSystem.Deltas, Is.EqualTo(new[] { 4f, 4f, 2f, 2f }));
        Assert.That(GeneratedWorldUpdateMutationSystem.Deltas, Is.EqualTo(new[] { 4f, 2f }));
        Assert.That(GeneratedWorldPostMutationSystem.Deltas, Is.EqualTo(new[] { 4f, 4f, 2f, 2f }));
        Assert.That(GeneratedWorldUpdateMutationSystem.EntityIds, Is.EqualTo(new[] { matching.Id, matching.Id }));

        Assert.That(world.RefComponentA(matching).Value, Is.EqualTo(48));
        Assert.That(world.RefComponentA(padding).Value, Is.EqualTo(26));
        Assert.That(world.RefComponentB(matching).Value, Is.EqualTo(3));
    }

    [Test]
    public void GeneratedWorldDoesNotExposeGeneratedNoLockMethods() {
        var publicDeclaredMethods = typeof(GeneratedStageWorld)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Select(method => method.Name);

        Assert.That(publicDeclaredMethods.Where(name => name.Contains("_NoLock")), Is.Empty);
    }

    [Test]
    public void GeneratedWorldFusesAdjacentPureSystemsWithSameComponentMask() {
        var world = new GeneratedPureWorld("generated-pure-world");
        world.CreateEntity(new ComponentA { Value = 1 });
        world.CreateEntity(new ComponentA { Value = 10 });

        world.Tick(1f);

        Assert.That(GeneratedPureFirstSystem.Calls.Concat(GeneratedPureSecondSystem.Calls), Is.Empty);
        Assert.That(GeneratedPureTrace.Calls, Is.EqualTo(new[] {
            "first:0:1",
            "second:0:2",
            "first:1:10",
            "second:1:11",
        }));
    }

    [Test]
    public void GeneratedWorldSupportsInlineComponentStorage() {
        var world = new GeneratedInlineWorld("generated-inline-world");
        var entity = world.CreateEntity(42);
        var reference = new InlineReferenceComponent { Value = 7 };

        world.Add(entity, reference);

        Assert.That(world.TryGetInt32(entity, out var value), Is.True);
        Assert.That(value, Is.EqualTo(42));
        Assert.That(world.RefInt32(entity), Is.EqualTo(42));
        Assert.That(world.TryGetInlineReferenceComponent(entity, out var storedReference), Is.True);
        Assert.That(storedReference, Is.SameAs(reference));

        world.Tick(1f);
        Assert.That(world.TryGetInt32(entity, out value), Is.True);
        Assert.That(value, Is.EqualTo(43));
        Assert.That(GeneratedInlinePrimitiveSystem.Calls, Is.EqualTo(1));

        world.RemoveInt32(entity);
        world.RemoveInlineReferenceComponent(entity);

        Assert.That(world.TryGetInt32(entity, out _), Is.False);
        Assert.That(world.TryGetInlineReferenceComponent(entity, out _), Is.False);
    }

    [Test]
    public void GeneratedWorldSupportsOptInBakedQueryIndexes() {
        var world = new GeneratedBakeQueryWorld("generated-bake-query-world");
        var first = world.CreateEntity(new ComponentA { Value = 1 });
        var second = world.CreateEntity(new ComponentA { Value = 10 }, new ComponentB { Value = 3 });
        var third = world.CreateEntity(new ComponentA { Value = 20 });

        world.Tick(1f);

        Assert.That(GeneratedBakeQuerySystem.EntityIds, Is.EqualTo(new[] { first.Id, second.Id, third.Id }));

        world.RemoveComponentA(first);
        world.Add(first, new ComponentA { Value = 30 });
        GeneratedBakeQuerySystem.Reset();

        world.Tick(1f);

        Assert.That(GeneratedBakeQuerySystem.EntityIds, Is.EqualTo(new[] { first.Id, second.Id, third.Id }));
    }
}

[RegisterComponent(typeof(ComponentA))]
[RegisterSystem(typeof(GeneratedWorldLifecycleSystem))]
[RegisterSystem(typeof(GeneratedWorldStaticUpdateSystem))]
[RegisterSystem(typeof(GeneratedWorldMoveSystem))]
[RegisterSystem(typeof(GeneratedWorldReferenceSystem))]
public partial class GeneratedCodegenWorld : World {
    public partial Entity CreateEntity(in ComponentA componentA, in ComponentB componentB);
}

[RegisterComponent(typeof(ComponentA))]
[RegisterComponent(typeof(ComponentB))]
[RegisterSystem(typeof(GeneratedWorldPipelineSystem))]
[RegisterSystem(typeof(GeneratedWorldPreMutationSystem))]
[RegisterSystem(typeof(GeneratedWorldUpdateMutationSystem))]
[RegisterSystem(typeof(GeneratedWorldPostMutationSystem))]
public partial class GeneratedStageWorld : World {
    public partial Entity CreateEntity(in ComponentA componentA, in ComponentB componentB);
}

[RegisterComponent(typeof(ComponentA))]
[RegisterSystem(typeof(GeneratedPureFirstSystem))]
[RegisterSystem(typeof(GeneratedPureSecondSystem))]
public partial class GeneratedPureWorld : World {
}

[RegisterComponent(typeof(int), Inline = true)]
[RegisterComponent(typeof(InlineReferenceComponent), Inline = true)]
[RegisterSystem(typeof(GeneratedInlinePrimitiveSystem))]
public partial class GeneratedInlineWorld : World {
}

[RegisterComponent(typeof(ComponentA))]
[RegisterComponent(typeof(ComponentB))]
[RegisterSystem(typeof(GeneratedBakeQuerySystem), bakeQuery: true)]
public partial class GeneratedBakeQueryWorld : World {
    public partial Entity CreateEntity(in ComponentA componentA, in ComponentB componentB);
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

public sealed class InlineReferenceComponent {
    public int Value;
}

public static class GeneratedInlinePrimitiveSystem {
    public static int Calls;

    public static void Reset() => Calls = 0;

    [SystemMethod(SystemMethodType.Update)]
    public static void Bump(ref int value) {
        value++;
        Calls++;
    }
}

public static class GeneratedBakeQuerySystem {
    public static readonly List<int> EntityIds = new();

    public static void Reset() => EntityIds.Clear();

    [SystemMethod(SystemMethodType.Update)]
    public static void Track(in Entity entity, ref ComponentA component) {
        EntityIds.Add(entity.Id);
        component.Value++;
    }
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

public static class GeneratedWorldPipelineSystem {
    public static readonly List<string> Stages = new();

    public static void Reset() => Stages.Clear();

    [SystemMethod(SystemMethodType.PreUpdate, 1)]
    public static void MarkPre([Uniform(true)] in float delta) => Stages.Add($"pre:{(int)delta}");

    [SystemMethod(SystemMethodType.Update, 1)]
    public static void MarkUpdate([Uniform(true)] in float delta) => Stages.Add($"update:{(int)delta}");

    [SystemMethod(SystemMethodType.PostUpdate, 1)]
    public static void MarkPost([Uniform(true)] in float delta) => Stages.Add($"post:{(int)delta}");
}

public static class GeneratedWorldPreMutationSystem {
    public static readonly List<float> Deltas = new();

    public static void Reset() => Deltas.Clear();

    [SystemMethod(SystemMethodType.PreUpdate, 2)]
    public static void Bump([Uniform(true)] in float delta, ref ComponentA componentA) {
        Deltas.Add(delta);
        componentA.Value += 1;
    }
}

public static class GeneratedWorldUpdateMutationSystem {
    public static readonly List<float> Deltas = new();
    public static readonly List<int> EntityIds = new();

    public static void Reset() {
        Deltas.Clear();
        EntityIds.Clear();
    }

    [SystemMethod(SystemMethodType.Update, 2)]
    public static void Apply(in Entity entity, [Uniform(true)] in float delta, ref ComponentA componentA, ref ComponentB componentB) {
        Deltas.Add(delta);
        EntityIds.Add(entity.Id);
        componentA.Value += componentB.Value + (int)delta;
    }
}

public static class GeneratedWorldPostMutationSystem {
    public static readonly List<float> Deltas = new();

    public static void Reset() => Deltas.Clear();

    [SystemMethod(SystemMethodType.PostUpdate, 2)]
    public static void Finish([Uniform(true)] in float delta, ref ComponentA componentA) {
        Deltas.Add(delta);
        componentA.Value *= 2;
    }
}

public static class GeneratedPureTrace {
    public static readonly List<string> Calls = new();
}

public static class GeneratedPureFirstSystem {
    public static readonly List<string> Calls = new();

    public static void Reset() {
        Calls.Clear();
        GeneratedPureTrace.Calls.Clear();
    }

    [SystemMethod(SystemMethodType.Update, 1, pure: true)]
    public static void Apply(in Entity entity, ref ComponentA componentA) {
        Calls.Add("unused");
        GeneratedPureTrace.Calls.Add($"first:{entity.Id}:{componentA.Value}");
        componentA.Value += 1;
        Calls.Clear();
    }
}

public static class GeneratedPureSecondSystem {
    public static readonly List<string> Calls = new();

    public static void Reset() => Calls.Clear();

    [SystemMethod(SystemMethodType.Update, 2, Pure = true)]
    public static void Apply(in Entity entity, ref ComponentA componentA) {
        Calls.Add("unused");
        GeneratedPureTrace.Calls.Add($"second:{entity.Id}:{componentA.Value}");
        componentA.Value += 1;
        Calls.Clear();
    }
}
