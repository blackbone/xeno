using System.Collections.Generic;
using System.Diagnostics;
using Xeno;

namespace Xeno {
    [DebuggerTypeProxy(typeof(World_Debug))]
    public sealed partial class World_Old {
        internal class World_Debug {
            private readonly World_Old _worldOld;
            public World_Debug(World_Old worldOld) => _worldOld = worldOld;

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Entity_Debug[] Entities {
                get {
                    var entities = new Entity_Debug[_worldOld.entityCount];
                    int i = 0;
                    foreach (var e in _worldOld.entities) {
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
                    var v = _worldOld.archetypes.head;
                    while (v != null) {
                        list.Add(v);
                        v = v.next;
                    }
                    return list;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public List<Store> Stores {
                get {
                    var list = new List<Store>();
                    for (int i = 0; i < _worldOld.stores.Length; i++) {
                        if (_worldOld.stores[i] == null) continue;

                        list.Add(_worldOld.stores[i]);
                    }
                    return list;
                }
            }
        }
    }
}
