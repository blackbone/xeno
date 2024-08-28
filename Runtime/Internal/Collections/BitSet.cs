using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitSet
    {
        private AutoGrowOnlyListULong data;
        
        public BitSet(uint density) => data = new AutoGrowOnlyListULong(density / 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(uint index) => data.At(index / 64) |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(uint index) => data.At(index / 64) |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(uint index) => (data.At(index / 64) & 1ul << (int)(index % 64)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAndSwapBack(uint index, uint lastIndex)
        {
            if (Get(lastIndex)) Set(index);
            else Unset(index);
            Unset(lastIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(in int capacity) => data.Ensure(capacity);
    }
}