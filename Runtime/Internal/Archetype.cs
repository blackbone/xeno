using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    internal sealed partial class Archetype : IEquatable<Archetype> {
        public readonly World world;
        public readonly bool floating;
        public BitSetReadOnly mask;

        public uint[] entities;
        public uint entitiesCount;
        public Archetype prev;
        public Archetype next;

        public Archetype(in bool floating, in World world) {
            this.world = world;
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
        public bool Equals(Archetype other) {
            if (other is null) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(world, other.world)
                && floating == other.floating
                && mask.Equals(other.mask) && Equals(entities, other.entities)
                && entitiesCount == other.entitiesCount;
        }
        public override bool Equals(object obj) {
            return ReferenceEquals(this, obj) || obj is Archetype other && Equals(other);
        }
        public override int GetHashCode() {
            return HashCode.Combine(world, floating, mask, entities, entitiesCount, prev, next);
        }
    }
}
