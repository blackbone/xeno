using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public unsafe struct FixedBitSet
    {
        private fixed ulong data[16];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index) => data[index / 64] |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(uint index) => data[index / 64] |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(uint index) => (data[index / 64] & 1ul << (int)(index % 64)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAndSwapBack(uint index, uint lastIndex)
        {
            if (Get(lastIndex)) Set(index);
            else Unset(index);
            Unset(lastIndex);
        }
    }
}