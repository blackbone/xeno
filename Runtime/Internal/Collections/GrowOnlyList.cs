using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unsafe = Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
#endif

namespace Xeno.Collections
{
    public struct GrowOnlyListInt
    {
        internal readonly int step;
        internal readonly int capacityGrow;

        internal int count;
        internal int capacity;
        internal int[][] data;

        public GrowOnlyListInt(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            this.step = step;

            count = 0;
            this.capacity = capacity;
            this.capacityGrow = capacityGrow;
            data = this.capacity == 0 ? Array.Empty<int[]>() : new int[capacity][];
        }

        public ref int this[in int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new int[step];
                return ref block[index % step];
            }
        }

        public ref int this[in uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new int[step];
                return ref block[index % step];
            }
        }
    }

    public struct GrowOnlyListUInt
    {
        internal readonly int step;
        internal readonly int capacityGrow;

        internal int count;
        internal int capacity;
        internal uint[][] data;

        public GrowOnlyListUInt(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            this.step = step;

            count = 0;
            this.capacity = capacity;
            this.capacityGrow = capacityGrow;
            data = this.capacity == 0 ? Array.Empty<uint[]>() : new uint[capacity][];
        }
        
        public ref uint this[in int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new uint[step];
                return ref block[index % step];
            }
        }

        public ref uint this[in uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new uint[step];
                return ref block[index % step];
            }
        }
    }

    public struct GrowOnlyListFixedBitSet
    {
        internal readonly int step;
        internal readonly int capacityGrow;

        internal int count;
        internal int capacity;
        internal FixedBitSet[][] data;

        public GrowOnlyListFixedBitSet(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            this.step = step;

            count = 0;
            this.capacity = capacity;
            this.capacityGrow = capacityGrow;
            data = this.capacity == 0 ? Array.Empty<FixedBitSet[]>() : new FixedBitSet[capacity][];
        }

        public ref FixedBitSet this[in int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new FixedBitSet[step];
                return ref block[index % step];
            }
        }

        public ref FixedBitSet this[in uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new FixedBitSet[step];
                return ref block[index % step];
            }
        }
    }

    public static class GrowOnlyListIntExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this ref GrowOnlyListInt list, in int value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var idx = list.data.Length;
                Array.Resize(ref list.data, idx + list.capacityGrow);
                list.data[idx] = new int[list.step];
                list.capacity = (idx + list.capacityGrow) * list.step;
            }

            list[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref GrowOnlyListInt list, in int capacity)
        {
            if (capacity >= list.capacity) // resize container array
            {
                var n = capacity /list. step + 1;
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new int[list.step];
            }
        }
    }

    public static class GrowOnlyListUIntExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this ref GrowOnlyListUInt list, in uint value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var idx = list.data.Length;
                Array.Resize(ref list.data, idx + list.capacityGrow);
                list.data[idx] = new uint[list.step];
                list.capacity = (idx + list.capacityGrow) * list.step;
            }

            list[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref GrowOnlyListUInt list, in int capacity)
        {
            if (capacity >= list.capacity) // resize container array
            {
                var n = capacity / list.step + 1;
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new uint[list.step];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TakeLast(this ref GrowOnlyListUInt list)
        {
            list.count--;
            return list[list.count];
        }
    }

    public static class GrowOnlyListFixedBitSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this ref GrowOnlyListFixedBitSet list, in FixedBitSet value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var idx = list.data.Length;
                Array.Resize(ref list.data, idx + list.capacityGrow);
                list.data[idx] = new FixedBitSet[list.step];
                list.capacity = (idx + list.capacityGrow) * list.step;
            }

            list[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref GrowOnlyListFixedBitSet list, in int capacity)
        {
            if (capacity >= list.capacity) // resize container array
            {
                var n = capacity /list. step + 1;
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new FixedBitSet[list.step];
            }
        }
    }
}