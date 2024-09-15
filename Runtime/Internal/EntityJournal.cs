using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xeno.Collections;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EntityJournal {
        internal readonly World world;
        public readonly int step;
        public readonly int capacity;
        public readonly int capacityGrow;

        internal GrowOnlyListUInt freeSlots;
        internal GrowOnlyListUInt versions;
        internal GrowOnlyList<Archetype> entityArchetypes;
        internal GrowOnlyListUInt entityArchetypeIndices;
        internal Archetypes archetypes;
        internal Archetype zeroArchetype;
        internal int currentCapacity;

        internal uint count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityJournal(World world, in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacityGrow, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            this.world = world;
            this.step = step;
            currentCapacity = this.capacity = capacity;
            this.capacityGrow = capacityGrow;

            freeSlots = new GrowOnlyListUInt(capacity, capacity);
            versions = new GrowOnlyListUInt(capacity, capacity);
            entityArchetypes = new GrowOnlyList<Archetype>(capacity, capacity);
            entityArchetypeIndices = new GrowOnlyListUInt(capacity, capacity);
            archetypes = new Archetypes(step, capacity, capacityGrow);

            zeroArchetype = null;
            count = 0;
        }
    }

    internal static class EntityJournalExtensions
    {
        private const uint VersionMask = 0b01111111_11111111_11111111_11111111U;
        private const uint EmptyMask = 0b10000000_00000000_00000000_00000000U;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetupDefaults(this ref EntityJournal journal, in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacityGrow, in int capacityGrow = Constants.DefaultCapacityGrow) {
            var zero = FixedBitSet.Zero;
            journal.zeroArchetype = journal.archetypes.AddPermanent(zero);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity At(this ref EntityJournal journal, uint index)
            => new(index, journal.versions.data[index] & VersionMask, journal.world.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Entity Create(this ref EntityJournal journal) {
            var index = journal.count;
            journal.count++;
            if (journal.freeSlots.count > 0)
            {
                index = journal.freeSlots.TakeLast();
                journal.Ensure((int)index);

                journal.versions.data[index] = (journal.versions.data[index] & VersionMask) + 1;
                journal.entityArchetypes.data[index] = journal.zeroArchetype;
                journal.entityArchetypeIndices.data[index] = journal.zeroArchetype.entities.Add(index);

                return new Entity(index, journal.versions.data[index] & VersionMask, journal.world.Id);
            }

            journal.Ensure((int)index);
            journal.versions.Add(0); // add new version
            journal.entityArchetypes.Add(journal.zeroArchetype);
            journal.entityArchetypeIndices.Add(journal.zeroArchetype.entities.Add(index));
            return new Entity(index, 0, journal.world.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(this ref EntityJournal journal, in uint entityId)
        {
            journal.count--;
            journal.versions.data[entityId] |= EmptyMask;
            journal.archetypes.Remove(journal.entityArchetypes.data[entityId], entityId);
            journal.entityArchetypes.data[entityId] = null;
            journal.entityArchetypeIndices.data[entityId] = uint.MaxValue;

            journal.freeSlots.Add(entityId);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref EntityJournal journal, int capacity)
        {
            if (capacity <= journal.currentCapacity) return;

            capacity = (journal.currentCapacity - capacity - journal.capacityGrow + 1) * journal.capacityGrow;
            journal.versions.Ensure(capacity);
            journal.entityArchetypes.Ensure(capacity);
            journal.entityArchetypeIndices.Ensure(capacity);
            journal.currentCapacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeArchetype(this ref EntityJournal journal, uint entityId, in FixedBitSet newArchetype) {
            ref var currentArchetype = ref journal.entityArchetypes.data[entityId];
            ref var entityArchetypeIndex = ref journal.entityArchetypeIndices.data[entityId];

            // entity always have archetype, it can be zero or non zero or whatever
            // delete entity from it's origin archetype
            journal.archetypes.Remove(currentArchetype, entityArchetypeIndex);
            entityArchetypeIndex = journal.archetypes.Add(newArchetype, entityId, ref currentArchetype);
        }
    }
}