using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    internal sealed partial class Archetype {
        public readonly World_Old WorldOld;
        public readonly bool floating;
        public BitSetReadOnly mask;

        public uint[] entities;
        public uint entitiesCount;
        public Archetype prev;
        public Archetype next;

        public Archetype(in bool floating, in World_Old worldOld) {
            this.WorldOld = worldOld;
            this.floating = floating;
            entities = new uint[Constants.DefaultArchetypeEntityCount];
            entitiesCount = 0;
        }

        public override string ToString() {
            return $"{mask.ToS()} ({entitiesCount}):\n\tnext:{next?.mask.ToS()} ({next?.entitiesCount})\n\tprev:{prev?.mask.ToS()} ({prev?.entitiesCount})";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() {
            entitiesCount = 0;
            Array.Clear(entities, 0, entities.Length);
            prev = null;
            next = null;
        }
    }
}
