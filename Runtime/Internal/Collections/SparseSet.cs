using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct SparseSet
    {
        internal uint n;
        internal AutoGrowOnlyListUInt dense;
        internal AutoGrowOnlyListUInt sparse;

        public uint Count => n;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SparseSet(uint density)
        {
            dense = new AutoGrowOnlyListUInt(density);
            sparse = new AutoGrowOnlyListUInt(density);
            n = 0;
        }
    }

    public static class SparseSetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add(this ref SparseSet set, in uint value) // 10
        {
            var index = set.n;
            set.dense.At(index) = value; // dense[0] = 10
            set.sparse.At(value) = index; // sparse[10] = 0
            set.n++;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remove(this ref SparseSet set, in uint value)
        {
            var index = set.sparse.At(value);
            set.n--;
            set.dense.At(set.sparse.At(value)) = set.dense.At(set.n);
            set.sparse.At(set.dense.At(set.n)) = set.sparse.At(value);
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this ref SparseSet set, in uint value, ref uint index)
        {
            ref var sparse = ref set.sparse;
            index = ref sparse.At(value);
            ref var dense = ref set.dense;
            return index < set.n && dense.At(index) == value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this ref SparseSet set, in uint value)
        {
            ref var sparse = ref set.sparse;
            ref var index = ref sparse.At(value);
            ref var dense = ref set.dense;
            return index < set.n && dense.At(index) == value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(this ref SparseSet set)
        {
            set.n = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref SparseSet set, in int capacity)
        {
            set.dense.Ensure(capacity);
            set.sparse.Ensure(capacity);
        }
    }
}