using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xeno {
    [DebuggerTypeProxy(typeof(Archetype_DebugView))]
    internal partial class Archetype {
        public class Archetype_DebugView {
            private readonly Archetype _archetype;
            private readonly World_Old _worldOld;

            public Archetype_DebugView(Archetype archetype) {
                _archetype = archetype;
                _worldOld = _archetype.WorldOld;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public uint EntitiesCount => _archetype.entitiesCount;

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public RWEntity[] Entities {
                get {
                    var result = new RWEntity[EntitiesCount];
                    for (int i = 0; i < _archetype.entitiesCount; i++) {
                        result[i] = _worldOld.entities[_archetype.entities[i]];
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
                    result.AddRange(_archetype.mask.GetIndices().Select(i => _worldOld.stores[i]));
                    return result;
                }
            }
        }
    }
}
