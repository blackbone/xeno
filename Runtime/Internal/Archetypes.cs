using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Xeno {
    internal sealed class Archetypes {
        internal readonly World world;
        internal Archetype[] freeArchetypes;
        internal int freeArchetypesCount;
        internal Archetype head;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetypes(World world) {
            this.world = world;
            freeArchetypes = new Archetype[Constants.PreInitializedArchetypesCount];
            freeArchetypesCount = Constants.PreInitializedArchetypesCount;
            for (var i = 0; i < freeArchetypesCount; i++)
                freeArchetypes[i] = new Archetype(true, world);

            head = null;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            var v = head;
            while(true) {
                sb.Append($"{v}");
                if (v?.next == null) break;
                sb.Append("\n-> ");
                v = v.next;
            }

            return sb.ToString();
        }
    }

    internal static class ArchetypesExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Archetype AddPermanent(this Archetypes archetypes, ref BitSetReadOnly mask) {
            var node = new Archetype(false, archetypes.world) {
                mask = mask,
                next = archetypes.head
            };

            if (archetypes.head != null) archetypes.head.prev = node;
            archetypes.head = node;

            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetypes archetypes, ref BitSet mask, in uint entityId, out Archetype archetype, out uint inArchetypeLocalIndex) {
             // iterate over to find matching archetype
            var v = archetypes.head;
            while (v != null && v.mask.hash != mask.hash) {
                v = v.next;
            }

            // archetype found, just pushing it there
            // archetype not found - need to create new there
            if (v == null) {
                v = archetypes.freeArchetypesCount > 0
                    ? archetypes.freeArchetypes[--archetypes.freeArchetypesCount]
                    : new Archetype(true, archetypes.world);
                v.mask = mask.AsReadOnly();
            }

            // get the new index
            var len = v.entities.Length;
            if (v.entitiesCount == len)
                Array.Resize(ref v.entities, len << 1);
            v.entities[v.entitiesCount] = entityId;
            inArchetypeLocalIndex = v.entitiesCount;
            v.entitiesCount++;

            // we're selected archetype so can assign it
            archetype = v;

            // this part of code is about updating the chain
            // if current archetype is head - no need to do anything
            if (v == archetypes.head) return;

            // 1. pop it from linked list
            if (v.prev != null) v.prev.next = v.next;
            if (v.next != null) v.next.prev = v.prev;

            v.prev = null;

            // set v as prev of head
            archetypes.head.prev = v;

            // set head as next of v
            v.next = archetypes.head;

            // set head as v
            archetypes.head = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this Archetypes archetypes, BitSetReadOnly mask, uint entityId, out Archetype archetype, out uint inArchetypeLocalIndex) {
            // iterate over to find matching archetype
            var v = archetypes.head;
            while (v != null && v.mask.hash != mask.hash) {
                v = v.next;
            }

            // archetype found, just pushing it there
            // archetype not found - need to create new there
            if (v == null) {
                v = archetypes.freeArchetypesCount > 0
                    ? archetypes.freeArchetypes[--archetypes.freeArchetypesCount]
                    : new Archetype(true, archetypes.world);
                v.mask = mask;
            }

            // get the new index
            var len = v.entities.Length;
            if (v.entitiesCount == len)
                Array.Resize(ref v.entities, len << 1);
            v.entities[v.entitiesCount] = entityId;
            inArchetypeLocalIndex = v.entitiesCount;
            v.entitiesCount++;

            // we're selected archetype so can assign it
            archetype = v;

            // this part of code is about updating the chain
            // if current archetype is head - no need to do anything
            if (v == archetypes.head) return;

            // so fixing relinking logic
            // for sample v is on index 2

            // 1. pop it from linked list
            if (v.prev != null) v.prev.next = v.next;
            if (v.next != null) v.next.prev = v.prev;

            v.prev = null;
            v.next = null;
            // not v is out of linked list

            // set v as prev of head
            archetypes.head.prev = v;

            // set head as next of v
            v.next = archetypes.head;

            // set head as v
            archetypes.head = v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this Archetypes archetypes, Archetype archetype, uint entityId, uint[] inArchetypeLocalIndices) {
            var localArchetypeIndex = inArchetypeLocalIndices[entityId];
            if (localArchetypeIndex >= archetype.entitiesCount) throw new IndexOutOfRangeException($"entityId: {entityId}, localIndex: {localArchetypeIndex}, entityCount: {archetype.entitiesCount}");

            var lastIndex = archetype.entitiesCount - 1;
            // if deleting element which is not last - need to fill hole with last
            if (localArchetypeIndex < archetype.entitiesCount) {
                // get last entity id
                var currentLastId = archetype.entities[lastIndex];

                // clear the value
                archetype.entities[lastIndex] = 0;
                archetype.entities[localArchetypeIndex] = currentLastId;
                inArchetypeLocalIndices[currentLastId] = localArchetypeIndex;
            }
            archetype.entitiesCount--;

            if (!archetype.floating || archetype.entitiesCount > 0)
                return;

            if (archetype.Equals(archetypes.head)) archetypes.head = archetype.next;
            if (archetype.prev != null) archetype.prev.next = archetype.next;
            if (archetype.next != null) archetype.next.prev = archetype.prev;

            archetype.Clear();
            var len = archetypes.freeArchetypes.Length;
            if (archetypes.freeArchetypesCount == len)
                Array.Resize(ref archetypes.freeArchetypes, len << 1);
            archetypes.freeArchetypes[archetypes.freeArchetypesCount] = archetype;
        }
    }
}
