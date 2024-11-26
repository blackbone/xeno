using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xeno {
    [DebuggerTypeProxy(typeof(Archetype_DebugView))]
    internal partial class Archetype {
        public class Archetype_DebugView {
            private readonly Archetype _archetype;
            private readonly World _world;

            public Archetype_DebugView(Archetype archetype) {
                _archetype = archetype;
                _world = _archetype.world;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public uint EntitiesCount => _archetype.entitiesCount;

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public RWEntity[] Entities {
                get {
                    var result = new RWEntity[EntitiesCount];
                    for (int i = 0; i < _archetype.entitiesCount; i++) {
                        result[i] = _world.entities[_archetype.entities[i]];
                    }
                    return result;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public string Mask => _archetype.mask.ToString();

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public List<Store> Stores {
                get {
                    var result = new List<Store>();
                    result.AddRange(_archetype.mask.GetIndices().Select(i => _world.stores[i]));
                    return result;
                }
            }
        }
    }
}
