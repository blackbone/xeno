using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unsafe = Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
#endif

namespace Xeno.Collections
{
    public struct GrowOnlyList<T>
    {
        private const uint DefaultStep = 4;

        private readonly uint step;
        private readonly uint capacityGrow;

        private uint count;
        private uint capacity;
        private T[][] data;

        public GrowOnlyList(uint step = DefaultStep, uint capacity = 0, uint capacityGrow = 32)
        {
            this.step = step;

            count = 0;
            this.capacity = capacity;
            this.capacityGrow = capacityGrow;
            data = this.capacity == 0 ? Array.Empty<T[]>() : new T[capacity][];
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new T[step];
                return ref block[index % step];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T value)
        {
            if (count >= capacity) // resize container array
            {
                var idx = (uint)data.Length;
                Array.Resize(ref data, (int)(idx + capacityGrow));
                data[idx] = new T[step];
                capacity = (idx + capacityGrow) * step;
            }

            this[count] = value;
            count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T TakeLast()
        {
            count--;
            return this[count];
        }
    }
    
    public struct GrowOnlyListUInt
    {
        private const uint DefaultStep = 4;

        private readonly uint step;
        private readonly uint capacityGrow;

        private uint count;
        private uint capacity;
        private uint[][] data;

        public GrowOnlyListUInt(uint step = DefaultStep, uint capacity = 0, uint capacityGrow = 32)
        {
            this.step = step;

            count = 0;
            this.capacity = capacity;
            this.capacityGrow = capacityGrow;
            data = this.capacity == 0 ? Array.Empty<uint[]>() : new uint[capacity][];
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => count;
        }

        public ref uint this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var blockIndex = index / step;
                var block = data[blockIndex] ??= new uint[step];
                return ref block[index % step];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint value)
        {
            if (count >= capacity) // resize container array
            {
                var idx = (uint)data.Length;
                Array.Resize(ref data, (int)(idx + capacityGrow));
                data[idx] = new uint[step];
                capacity = (idx + capacityGrow) * step;
            }

            this[count] = value;
            count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint TakeLast()
        {
            count--;
            return this[count];
        }
    }
}