using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitSet
    {
        private AutoGrowOnlyListULong data;
        
        public BitSet(int density) => data = new AutoGrowOnlyListULong(density / 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index) => data.At(index / 64) |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Unset(int index) => data.At(index / 64) |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get(int index) => (data.At(index / 64) & 1ul << (int)(index % 64)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveAndSwapBack(int index, int lastIndex)
        {
            if (Get(lastIndex)) Set(index);
            else Unset(index);
            Unset(lastIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Ensure(in int capacity) => data.Ensure(capacity);
    }
}