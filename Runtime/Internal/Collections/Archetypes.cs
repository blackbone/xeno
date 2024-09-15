using System.Runtime.CompilerServices;
using System.Text;

namespace Xeno.Collections {
    internal sealed class Archetypes {
        private readonly int step;
        private readonly int capacityGrow;

        private XStack<Archetype> freeNodes;
        internal Archetype head;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetypes(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow) {
            this.step = step;
            this.capacityGrow = capacityGrow;

            freeNodes = new XStack<Archetype>(capacity, capacityGrow);
            for (var i = 0; i < capacity; i++) freeNodes.Push(new Archetype(true, step, capacityGrow));
            head = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetype AddPermanent(in FixedBitSet archetype) {
            var node = new Archetype(false) {
                mask = archetype,
                entities = new SwapBackListUInt(step, capacityGrow),
                next = head
            };

            if (head != null) head.prev = node;
            head = node;

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Add(in FixedBitSet archetype, in uint entityId, ref Archetype v) {
            // iterate over to find matching archetype
            v = head;
            while (v != null && !v.mask.Equals(archetype)) {
                v = v.next;
            }

            // archetype found, just pushing it there
            // archetype not found - need to create new there
            if (v == null) {
                if (!freeNodes.Pop(ref v)) v = new Archetype(true, step, capacityGrow);
                v.mask = archetype;
            }

            // get the new index
            var index = v.entities.Add(entityId); // need to return this

            // this part of code is about updating the chain
            // if current archetype is head - no need to do anything
            if (v == head) return index;

            // otherwise need to pop it out of linked list
            // and place into head

            // re-link next and prev to each other
            // v.next and v.prev can be nulls - it's valid values, but not ok
            if (v.prev != null) v.prev.next = v.next;
            if (v.next != null) v.next.prev = v.prev;

            // also reset v's prev and next
            v.prev = null;
            v.next = null;

            // this is just popping, now updating the head
            head.prev = v;
            v.next = head;
            head = v;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(in Archetype archetype, in uint localArchetypeIndex) {
            archetype.entities.RemoveAtAndSwapBack(localArchetypeIndex);
            if (!archetype.floating || archetype.entities.count > 0) {
                return;
            }

            if (archetype.prev != null) archetype.prev.next = archetype.next;
            if (archetype.next != null) archetype.next.prev = archetype.prev;
            archetype.next = null;
            archetype.prev = null;

            archetype.entities.Clear();
            freeNodes.Push(archetype);
        }

        public override string ToString() {
            var sb = new StringBuilder();
            var v = head;
            while(true) {
                sb.Append($"[{v}]");
                if (v?.next == null) break;
                sb.Append("->");
                v = v.next;
            }

            return sb.ToString();
        }
    }
}