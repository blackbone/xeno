// using System.Runtime.InteropServices;
//
// namespace Xeno.Tests;
//
// public class ChunkTests
// {
//     // [TestCaseSource(nameof(ComponentSources))]
//     // public void ChunkLayoutTest(ComponentInfo[] infos)
//     // {
//     //     var chunk = new Chunk(infos);
//     //     Assert.That(chunk.componentsCount, Is.EqualTo(infos.Length));
//     //     Assert.That(chunk.entitiesCount, Is.EqualTo(0));
//     //     var archetypeSize = infos.Sum(info => info.Size) + Marshal.SizeOf<Entity>();
//     //     var maxEntities = (ushort)(Chunk.ChunkSize / archetypeSize);
//     //     // additional check that all our mappings fit into chunk limit
//     //     while (maxEntities * archetypeSize + maxEntities * 8 + 3 > Chunk.ChunkSize)
//     //         maxEntities--;
//     //     Assert.That(chunk.maxEntities, Is.EqualTo(maxEntities));
//     //     Assert.Pass(chunk.ToString());
//     // }
//
//     [TestCaseSource(nameof(ComponentSources))]
//     public void AddEntityTest(ComponentInfo[] infos)
//     {
//         var chunk = new Chunk(infos);
//         
//         Assert.That(chunk.componentsCount, Is.EqualTo(infos.Length));
//         Assert.That(chunk.entitiesCount, Is.EqualTo(0));
//         
//         var freeSize = Chunk.ChunkSize;
//         freeSize -= sizeof(byte) // component count
//                     - sizeof(ushort) // maxEntities
//                     - sizeof(ushort) // entitiesCount
//                     - sizeof(int) * infos.Length // hashes
//                     - sizeof(int) * infos.Length; // offsets
//         
//         var archetypeSize = infos.Sum(info => info.Size) + sizeof(uint); // space per all components set + entity id
//         var maxEntities = (ushort)(freeSize / archetypeSize);
//         
//         Assert.That(chunk.maxEntities, Is.EqualTo(maxEntities));
//
//         var ids = Enumerable.Range(1, maxEntities).ToArray();
//         foreach (var i in ids)
//             chunk.Add(new Entity((uint)i, 1, 1));
//
//         Assert.That(chunk.entitiesCount, Is.EqualTo(maxEntities));
//         
//         try
//         {
//             var entity = new Entity(maxEntities, 1, 1);
//             chunk.Add(entity);
//         }
//         catch (ArgumentOutOfRangeException e)
//         {
//             Assert.Pass(e.ToString());
//         }
//         
//         
//     }
//     
//     public static IEnumerable<ComponentInfo[]> ComponentSources()
//     {
//         // empty archetype
//         yield return [];
//         
//         // single components
//         yield return [Component<ComponentSmall1>.Info];
//         yield return [Component<ComponentSmall2>.Info];
//         yield return [Component<ComponentSmall3>.Info];
//         yield return [Component<ComponentLarge1>.Info];
//         yield return [Component<ComponentLarge2>.Info];
//         yield return [Component<ComponentLarge3>.Info];
//         
//         // small components
//         yield return [Component<ComponentSmall1>.Info, Component<ComponentSmall2>.Info, Component<ComponentSmall3>.Info];
//         
//         // large components
//         yield return [Component<ComponentLarge1>.Info, Component<ComponentLarge2>.Info, Component<ComponentLarge3>.Info];
//         
//         // all components
//         yield return [Component<ComponentSmall1>.Info, Component<ComponentSmall2>.Info, Component<ComponentSmall3>.Info, Component<ComponentLarge1>.Info, Component<ComponentLarge2>.Info, Component<ComponentLarge3>.Info];
//     }
//     
//     public static IEnumerable<ComponentInfo[]> AllWithC1()
//     {
//         // single components
//         yield return [Component<ComponentSmall1>.Info];
//         
//         // small components
//         yield return [Component<ComponentSmall1>.Info, Component<ComponentSmall2>.Info, Component<ComponentSmall3>.Info];
//         
//         // all components
//         yield return [Component<ComponentSmall1>.Info, Component<ComponentSmall2>.Info, Component<ComponentSmall3>.Info, Component<ComponentLarge1>.Info, Component<ComponentLarge2>.Info, Component<ComponentLarge3>.Info];
//     }
// }