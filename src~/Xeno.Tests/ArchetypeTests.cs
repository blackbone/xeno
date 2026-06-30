using NUnit.Framework;

namespace Xeno.Tests;

[TestFixture]
public class ArchetypeTests {
    [SetUp]
    public void OneTimeSetUp() => TestWorlds.Create("world");

    [TearDown]
    public void OneTimeTearDown() {
        if (Worlds.TryGet("world", out var world))
            world.Dispose();
    }

    [Test]
    public void OneEntityMultiPass() {
        OneEntityPass();
        OneEntityPass();
        OneEntityPass();
        OneEntityPass();
    }

    [Test]
    public void OneEntityPass() {
        var world = TestWorlds.Get("world");

        var e = world.CreateEntity();

        Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.Add(e, default(Component1));

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        var firstArchetype = world.entityArchetypes[e.Id];
        Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.Add(e, default(Component2));

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));

        var secondArchetype = world.entityArchetypes[e.Id];
        Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.Add(e, default(Component3));

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent3AndComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));

        var thirdArchetype = world.entityArchetypes[e.Id];
        Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.Add(e, default(Component4));

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));

        var fourthArchetype = world.entityArchetypes[e.Id];
        Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.RemoveComponent1(e);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));

        var fifthArchetype = world.entityArchetypes[e.Id];
        Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.RemoveComponent2(e);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));

        var sixthArchetype = world.entityArchetypes[e.Id];
        Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.RemoveComponent3(e);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));

        var seventhArchetype = world.entityArchetypes[e.Id];
        Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        world.RemoveComponent4(e);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));

        var eigthArchetype = world.entityArchetypes[e.Id];
        Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));

        e.Destroy();
    }

    [Test]
    public void TwoEntitiesMultiPass() {
        TwoEntitiesPass();
        TwoEntitiesPass();
        TwoEntitiesPass();
        TwoEntitiesPass();
    }

    [Test]
    public void TwoEntitiesPass() {
        var world = TestWorlds.Get("world");

        var e1 = world.CreateEntity();

        Assert.That(world.entityArchetypes[e1.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(world.inArchetypeLocalIndices[e1.Id], Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        var e2 = world.CreateEntity();

        Assert.That(world.entityArchetypes[e2.Id], Is.EqualTo(world.zeroArchetype));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(world.inArchetypeLocalIndices[e2.Id], Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.Add(e1, default(Component1));

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        var firstArchetype = world.entityArchetypes[e1.Id];
        Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.Add(e2, default(Component1));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.Add(e1, default(Component2));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));

        var secondArchetype = world.entityArchetypes[e1.Id];
        Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.Add(e2, default(Component2));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.Add(e1, default(Component3));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent3AndComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));

        var thirdArchetype = world.entityArchetypes[e1.Id];
        Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.Add(e2, default(Component3));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent3AndComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.Add(e1, default(Component4));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));

        var fourthArchetype = world.entityArchetypes[e1.Id];
        Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.Add(e2, default(Component4));

        Assert.That(world.CountComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.RemoveComponent1(e1);

        Assert.That(world.CountComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));

        var fifthArchetype = world.entityArchetypes[e1.Id];
        Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.RemoveComponent1(e2);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(2));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.RemoveComponent2(e1);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(1));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));

        var sixthArchetype = world.entityArchetypes[e1.Id];
        Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.RemoveComponent2(e2);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(2));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.RemoveComponent3(e1);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(1));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));

        var seventhArchetype = world.entityArchetypes[e1.Id];
        Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.RemoveComponent3(e2);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent4(), Is.EqualTo(2));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
        Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        world.RemoveComponent4(e1);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent4(), Is.EqualTo(1));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));

        var eigthArchetype = world.entityArchetypes[e1.Id];
        Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(1));
        Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));

        world.RemoveComponent4(e2);

        Assert.That(world.CountComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3(), Is.EqualTo(0));
        Assert.That(world.CountComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.CountComponent4AndComponent1AndComponent2(), Is.EqualTo(0));
        Assert.That(world.CountComponent1AndComponent2AndComponent3AndComponent4(), Is.EqualTo(0));
        Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
        Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));

        Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
        Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(2));
        Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));

        e1.Destroy();
        e2.Destroy();
    }
}