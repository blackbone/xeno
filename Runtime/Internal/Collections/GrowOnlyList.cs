using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unsafe = Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
#endif

namespace Xeno.Collections
{
    public struct GrowOnlyList<T> where T : class
    {
        internal readonly int capacityGrow;

        internal int count;
        internal T[] data;

        public GrowOnlyList(in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            count = 0;
            this.capacityGrow = capacityGrow;
            data = capacity == 0 ? Array.Empty<T>() : new T[capacity];
        }
    }

    public struct GrowOnlyListUInt
    {
        internal readonly int capacityGrow;

        internal int count;
        internal uint[] data;

        public GrowOnlyListUInt(in int capacity = Constants.DefaultCapacity, in int capacityGrow = Constants.DefaultCapacityGrow)
        {
            count = 0;
            this.capacityGrow = capacityGrow;
            data = capacity == 0 ? Array.Empty<uint>() : new uint[capacity];
        }
    }

    public static class GrowOnlyListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref GrowOnlyList<T> list, in T value) where T : class {
            if (list.count >= list.data.Length) // resize container array
                Array.Resize(ref list.data, list.data.Length + list.capacityGrow);

            list.data[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref GrowOnlyList<T> list, int capacity) where T : class {
            if (capacity <= list.data.Length) return;

            capacity = ((capacity - list.data.Length) / list.capacityGrow + 1) * list.capacityGrow;
            Array.Resize(ref list.data, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TakeLast<T>(this ref GrowOnlyList<T> list) where T : class {
            list.count--;
            return list.data[list.count];
        }
    }

    public static class GrowOnlyListUIntExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this ref GrowOnlyListUInt list, in uint value)
        {
            if (list.count >= list.data.Length) // resize container array
                Array.Resize(ref list.data, list.data.Length + list.capacityGrow);

            list.data[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref GrowOnlyListUInt list, int capacity) {
            if (capacity <= list.data.Length) return;

            capacity = ((capacity - list.data.Length) / list.capacityGrow + 1) * list.capacityGrow;
            Array.Resize(ref list.data, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint TakeLast(this ref GrowOnlyListUInt list)
        {
            list.count--;
            return list.data[list.count];
        }
    }
}