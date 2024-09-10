using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xeno.Collections;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EntityJournal
    {
        internal readonly World world;
        internal GrowOnlyListUInt freeSlots;
        internal GrowOnlyListUInt versions;
        internal GrowOnlyListFixedBitSet archetypes;
        internal DictionarySlim<FixedBitSet, SwapBackListUInt> archetypesMap;
        internal GrowOnlyListInt archetypeMapIndices;
        internal uint count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityJournal(World world, in int grow = Constants.DefaultCapacityGrow)
        {
            this.world = world;
            freeSlots = new GrowOnlyListUInt(grow, grow);
            versions = new GrowOnlyListUInt(grow, grow);
            archetypes = new GrowOnlyListFixedBitSet(grow, grow);
            archetypesMap = new DictionarySlim<FixedBitSet, SwapBackListUInt>(grow, FixedBitSetComparer.Shared);
            archetypeMapIndices = new GrowOnlyListInt(grow, grow);
            count = 0;
        }
    }

    public delegate void InerationDelegate(in ReadOnlySpan<uint> entityIds);

    internal static class EntityJournalExtensions
    {
        private const uint VersionMask = 0b01111111_11111111_11111111_11111111U;
        private const uint EmptyMask = 0b10000000_00000000_00000000_00000000U;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity At(this ref EntityJournal journal, uint index) => new(index, journal.versions[index] & VersionMask, journal.world.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ForEach(this ref EntityJournal journal, in FixedBitSet mask, InerationDelegate iterationDelegate) {
            for (int i = 0; i < journal.archetypesMap._count; i++) {
                ref var entry = ref journal.archetypesMap._entries[i];
                if (entry.key.Includes(mask))
                    iterationDelegate(entry.value.GetSpan());

            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool With(this ref EntityJournal journal, in FixedBitSet componentMask, ref uint from, ref Span<Entity> entities, ref int count) {
            var worldId = journal.world.Id;
            var i = 0;
            var writableEntities = new Span<RWEntity>(Unsafe.AsPointer(ref entities[0]), entities.Length);
            while (from < journal.archetypes.count && i < entities.Length)
            {
                if (journal.archetypes[from].Includes(componentMask)) {
                    writableEntities[i].Id = from;
                    writableEntities[i].Version = from;
                    writableEntities[i++].WorldId = worldId;
                }
                from++;
            }
            
            count = i;
            return from == journal.archetypes.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Create(this ref EntityJournal journal)
        {
            journal.count++;
            if (journal.freeSlots.count > 0)
            {
                var index = journal.freeSlots.TakeLast();
                journal.versions[index] = (journal.versions[index] & VersionMask) + 1;
                journal.archetypes[index] = FixedBitSet.Zero; // zeroes
                journal.archetypeMapIndices[index] = -1;
                return new Entity(index, journal.versions[index] & VersionMask, journal.world.Id);
            }

            journal.versions.Add(0);
            journal.archetypes.Add(FixedBitSet.Zero); // zeroes
            journal.archetypeMapIndices.Add(-1);
            return new Entity(journal.count - 1, 0, journal.world.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this ref EntityJournal journal, in uint index)
        {
            journal.count--;
            journal.versions[index] |= EmptyMask;
            journal.archetypes[index] = default;
            journal.freeSlots.Add(index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref EntityJournal journal, in int capacity)
        {
            journal.versions.Ensure(capacity);
            journal.archetypes.Ensure(capacity);
            journal.archetypeMapIndices.Ensure(capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeArchetype(this ref EntityJournal journal, in uint entityId, in FixedBitSet oldArchetype, in FixedBitSet newArchetype) {
            var localIndex = journal.archetypeMapIndices[entityId];

            SwapBackListUInt oldList = default;
            if (localIndex >= 0 && journal.archetypesMap.TryGetValue(oldArchetype, ref oldList)) {
                oldList.RemoveAtAndSwapBack(localIndex);
            }

            if (newArchetype == FixedBitSet.Zero) {
                return;
            }

            ref var newList = ref journal.archetypesMap.GetOrAddValueRef(newArchetype, out var @new);
            if (@new) newList = new SwapBackListUInt(Constants.DefaultCapacityGrow);
            newList.Add(entityId);
            journal.archetypeMapIndices[entityId] = newList.count - 1;
        }
    }
}