using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct SparseSet
    {
        internal uint n;
        internal AutoGrowOnlyListUInt dense;
        internal AutoGrowOnlyListUInt sparse;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SparseSet(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity)
        {
            dense = new AutoGrowOnlyListUInt(step, capacity);
            sparse = new AutoGrowOnlyListUInt(step, capacity);
            n = 0;
        }
    }

    public static class SparseSetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add(this ref SparseSet set, in uint value) // 10
        {
            set.sparse.Ensure((int)value);
            var index = set.n;
            set.dense.Ensure((int)index);
            set.dense.data[index] = value; // dense[0] = 10
            set.sparse.data[value] = index; // sparse[10] = 0
            set.n++;

            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Remove(this ref SparseSet set, in uint value)
        {
            var index = set.sparse.data[value];
            set.n--;
            set.dense.data[set.sparse.data[value]] = set.dense.data[set.n];
            set.sparse.data[set.dense.data[set.n]] = set.sparse.data[value];
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this ref SparseSet set, in uint value, ref uint index)
        {
            if (value >= set.sparse.data.Length) return false;
            index = set.sparse.data[value];
            return index < set.n && set.dense.data[index] == value;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this ref SparseSet set, in uint value) {
            if (value >= set.sparse.data.Length) return false;
            ref var index = ref set.sparse.data[value];
            return index < set.n && set.dense.data[index] == value;
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