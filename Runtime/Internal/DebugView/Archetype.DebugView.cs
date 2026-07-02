using System.Collections.Generic;
using System.Diagnostics;

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
            public int EntitiesCount => _archetype.entitiesCount;

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public Entity[] Entities {
                get {
                    var result = new Entity[EntitiesCount];
                    for (int i = 0; i < _archetype.entitiesCount; i++) {
                        result[i] = _world.entities[_archetype.entities[i]];
                    }
                    return result;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
            public string Mask => _archetype.mask.ToString();
        }
    }
}
