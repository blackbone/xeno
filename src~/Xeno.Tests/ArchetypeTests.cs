// namespace Xeno.Tests;
//
// [TestFixture]
// public class ArchetypeTests {
//     [SetUp]
//     public void OneTimeSetUp() => Worlds.Create("world");
//
//     [TearDown]
//     public void OneTimeTearDown() {
//         if (Worlds.TryGet("world", out var world))
//             world.Dispose();
//     }
//
//     [Test]
//     public void OneEntityMultiPass() {
//         OneEntityPass();
//         OneEntityPass();
//         OneEntityPass();
//         OneEntityPass();
//     }
//
//     [Test]
//     public void OneEntityPass() {
//         Worlds.TryGet("world", out var world);
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         var e = world.CreateEntity();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.entityArchetypes[e.Id], Is.EqualTo(world.zeroArchetype));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(world.inArchetypeLocalIndices[e.Id], Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.AddComponents(default(Component1));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         var firstArchetype = world.entityArchetypes[e.Id];
//         Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.AddComponents(default(Component2));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//
//         var secondArchetype = world.entityArchetypes[e.Id];
//         Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.AddComponents(default(Component3));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3, Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//
//         var thirdArchetype = world.entityArchetypes[e.Id];
//         Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.AddComponents(default(Component4));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//
//         var fourthArchetype = world.entityArchetypes[e.Id];
//         Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.RemoveComponents<Component1>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//
//         var fifthArchetype = world.entityArchetypes[e.Id];
//         Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.RemoveComponents<Component2>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//
//         var sixthArchetype = world.entityArchetypes[e.Id];
//         Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.RemoveComponents<Component3>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
//
//         var seventhArchetype = world.entityArchetypes[e.Id];
//         Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.RemoveComponents<Component4>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
//
//         var eigthArchetype = world.entityArchetypes[e.Id];
//         Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
//         Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e.Id]], Is.EqualTo(e.Id));
//
//         e.Destroy();
//     }
//
//     [Test]
//     public void TwoEntitiesMultiPass() {
//         TwoEntitiesPass();
//         TwoEntitiesPass();
//         TwoEntitiesPass();
//         TwoEntitiesPass();
//     }
//
//     [Test]
//     public void TwoEntitiesPass() {
//         Worlds.TryGet("world", out var world);
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         var e1 = world.CreateEntity();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.entityArchetypes[e1.Id], Is.EqualTo(world.zeroArchetype));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(world.inArchetypeLocalIndices[e1.Id], Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         var e2 = world.CreateEntity();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.entityArchetypes[e2.Id], Is.EqualTo(world.zeroArchetype));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(world.inArchetypeLocalIndices[e2.Id], Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.AddComponents(default(Component1));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
//         var firstArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.AddComponents(default(Component1));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(firstArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.AddComponents(default(Component2));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(1));
//
//         var secondArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.AddComponents(default(Component2));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(secondArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(secondArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.AddComponents(default(Component3));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3, Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(1));
//
//         var thirdArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.AddComponents(default(Component3));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3, Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(thirdArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(thirdArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.AddComponents(default(Component4));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(1));
//
//         var fourthArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.AddComponents(default(Component4));
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(fourthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(fourthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.RemoveComponents<Component1>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(1));
//
//         var fifthArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.RemoveComponents<Component1>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(fifthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(fifthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.RemoveComponents<Component2>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(1));
//
//         var sixthArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.RemoveComponents<Component2>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(sixthArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(sixthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.RemoveComponents<Component3>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(1));
//
//         var seventhArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.RemoveComponents<Component3>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(2));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(seventhArchetype, Is.Not.EqualTo(world.zeroArchetype));
//         Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(seventhArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.RemoveComponents<Component4>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(1));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(seventhArchetype.entitiesCount, Is.EqualTo(1));
//
//         var eigthArchetype = world.entityArchetypes[e1.Id];
//         Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
//         Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(1));
//         Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e1.Id]], Is.EqualTo(e1.Id));
//
//         e2.RemoveComponents<Component4>();
//
//         Console.WriteLine($"{world.archetypes}\n");
//
//         Assert.That(world.Count<Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component4, Component1, Component2>(), Is.EqualTo(0));
//         Assert.That(world.Count<Component1, Component2, Component3, Component4>(), Is.EqualTo(0));
//         Assert.That(world.zeroArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(firstArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(secondArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(thirdArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fourthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(fifthArchetype.entitiesCount, Is.EqualTo(0));
//         Assert.That(sixthArchetype.entitiesCount, Is.EqualTo(0));
//
//         Assert.That(eigthArchetype, Is.EqualTo(world.zeroArchetype));
//         Assert.That(eigthArchetype.entitiesCount, Is.EqualTo(2));
//         Assert.That(eigthArchetype.entities[world.inArchetypeLocalIndices[e2.Id]], Is.EqualTo(e2.Id));
//
//         e1.Destroy();
//         e2.Destroy();
//     }
// }
