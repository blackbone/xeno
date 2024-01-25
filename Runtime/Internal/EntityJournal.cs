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
        
        internal uint Count;

        private readonly World world;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityJournal(World world)
        {
            this.world = world;
            freeSlots = new GrowOnlyListUInt(1024);
            versions = new GrowOnlyListUInt(1024);
            Count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity Ref(uint index)
            => new(index, versions[index] & VersionMask, world.Id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity Create()
        {
            Count++;
            if (freeSlots.Count > 0)
            {
                var index = freeSlots.TakeLast();
                versions[index] = (versions[index] & VersionMask) + 1;
                return Ref(index);
            }

            versions.Add(0);
            return new Entity(Count - 1, 0, world.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(in uint index)
        {
            Count--;
            versions[index] |= EmptyMask;
            freeSlots.Add(index);
        }
    }
}