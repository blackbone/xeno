using System;
using System.Runtime.CompilerServices;
using Xeno.Collections;

namespace Xeno
{
    internal struct EntityJournal
    {
        private const uint VersionMask = 0b01111111_11111111_11111111_11111111U;
        private const uint EmptyMask = 0b10000000_00000000_00000000_00000000U;

        private GrowOnlyListUInt freeSlots;
        private GrowOnlyListUInt versions; // first bit of version
        internal GrowOnlyList<FixedBitSet> archetypes;
        
        internal uint Count;

        private readonly World world;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityJournal(World world)
        {
            this.world = world;
            freeSlots = new GrowOnlyListUInt(1024);
            versions = new GrowOnlyListUInt(1024);
            archetypes = new GrowOnlyList<FixedBitSet>(1024);
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity At(uint index) => new(index, versions[index] & VersionMask, world.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool With(in FixedBitSet componentMask, ref uint from, ref Span<Entity> entities, ref int count)
        {
            var j = 0;
            while (from < archetypes.Count && j < entities.Length)
            {
                if (archetypes[from].Includes(componentMask))
                    entities[j] = new Entity(from, versions[from] & VersionMask, world.Id);
                j++;
                from++;
            }
            
            count = j;
            return from == archetypes.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity Create()
        {
            Count++;
            if (freeSlots.Count > 0)
            {
                var index = freeSlots.TakeLast();
                versions[index] = (versions[index] & VersionMask) + 1;
                return At(index);
            }

            versions.Add(0);
            archetypes.Add(default); // zeroes
            return new Entity(Count - 1, 0, world.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(in uint index)
        {
            Count--;
            versions[index] |= EmptyMask;
            archetypes[index] = default;
            freeSlots.Add(index);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(in int capacity)
        {
            versions.Ensure(capacity);
            archetypes.Ensure(capacity);
        }
    }
}