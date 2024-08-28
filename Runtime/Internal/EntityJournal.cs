using System;
using System.Runtime.CompilerServices;
using Xeno.Collections;

namespace Xeno
{
    internal struct EntityJournal
    {
        private const uint VersionMask = 0b01111111_11111111_11111111_11111111U;
        private const uint EmptyMask = 0b10000000_00000000_00000000_00000000U;

        // use arrays?
        private GrowOnlyListUInt freeSlots;
        private GrowOnlyListUInt versions; // first bit of version
        internal GrowOnlyListFixedBitSet archetypes;
        
        internal uint Count;

        private readonly World world;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityJournal(World world, in uint grow = 16384)
        {
            this.world = world;
            freeSlots = new GrowOnlyListUInt(grow, grow);
            versions = new GrowOnlyListUInt(grow, grow);
            archetypes = new GrowOnlyListFixedBitSet(grow, grow);
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity At(uint index) => new(index, versions[index] & VersionMask, world.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool With(in FixedBitSet componentMask, ref uint from, ref Span<Entity> entities, ref int count) {
            var worldId = world.Id;
            var i = 0;
            var writableEntities = new Span<RWEntity>(Unsafe.AsPointer(ref entities[0]), entities.Length);
            while (from < archetypes.count && i < entities.Length)
            {
                if (archetypes[from].Includes(componentMask)) {
                    writableEntities[i].Id = from;
                    writableEntities[i].Version = from;
                    writableEntities[i++].WorldId = worldId;
                }
                from++;
            }
            
            count = i;
            return from == archetypes.count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity Create()
        {
            Count++;
            if (freeSlots.count > 0)
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