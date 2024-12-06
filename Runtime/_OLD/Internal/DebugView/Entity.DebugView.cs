// using System.Diagnostics;
// using Xeno;
//
// namespace Xeno {
//     [DebuggerTypeProxy(typeof(World_Old.Entity_Debug))]
//     public readonly partial struct Entity_Old { }
//
//     [DebuggerTypeProxy(typeof(World_Old.Entity_Debug))]
//     internal partial struct RWEntity { }
//
//     public sealed partial class World_Old {
//         internal struct Entity_Debug {
//             public readonly uint Id;
//             public readonly uint Version;
//             public readonly bool IsAlive;
//             public readonly Archetype Archetype;
//             public readonly long ArchetypeIndex;
//
//             public Entity_Debug(RWEntity entity) {
//                 Worlds.TryGet(entity.WorldId, out var world);
//
//                 Id = entity.Id;
//                 Version = entity.Version & ~AllocatedMask;
//                 IsAlive = entity.Id == world.Id && entity.Version == world.entities[entity.Id].Version;
//                 Archetype = IsAlive ? world.entityArchetypes[Id] : null;
//                 ArchetypeIndex = IsAlive ? world.inArchetypeLocalIndices[Id] : -1;
//             }
//
//             public Entity_Debug(Entity_Old entityOld) {
//                 Worlds.TryGet(entityOld.WorldId, out var world);
//
//                 Id = entityOld.Id;
//                 Version = entityOld.Version & ~AllocatedMask;
//                 IsAlive = entityOld.Id == world.Id && entityOld.Version == world.entities[entityOld.Id].Version;
//                 Archetype = IsAlive ? world.entityArchetypes[Id] : null;
//                 ArchetypeIndex = IsAlive ? world.inArchetypeLocalIndices[Id] : -1;
//             }
//         }
//     }
// }
