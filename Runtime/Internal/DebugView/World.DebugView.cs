using System.Collections.Generic;
using System.Diagnostics;

namespace Xeno {
    [DebuggerTypeProxy(typeof(World_Debug))]
    public partial class World {
        internal class World_Debug {
            private readonly World _world;
            public World_Debug(World world) => _world = world;

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Entity_Debug[] Entities {
                get {
                    var entities = new Entity_Debug[_world.entityCount];
                    int i = 0;
                    foreach (var e in _world.entities) {
                        if ((e.Version & AllocatedMask) == 0) continue;
                        entities[i] = new Entity_Debug(e);
                    }
                    return entities;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public List<Archetype> Archetypes {
                get {
                    var list = new List<Archetype>();
                    var v = _world.archetypes.head;
                    while (v != null) {
                        list.Add(v);
                        v = v.next;
                    }
                    return list;
                }
            }
        }
    }
}
