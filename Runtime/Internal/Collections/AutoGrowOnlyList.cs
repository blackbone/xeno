using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct AutoGrowOnlyList<T>
    {
        internal const uint DefaultStep = 4;
        internal readonly uint step;
        internal T[][] data;

        public AutoGrowOnlyList(uint capacity, uint step = DefaultStep)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<T[]>() : new T[capacity][];
        }
    }
    
    public struct AutoGrowOnlyListUInt
    {
        internal const uint DefaultStep = 256;
        internal readonly uint step;
        internal uint[][] data;

        public AutoGrowOnlyListUInt(uint step = DefaultStep, uint capacity = 0)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<uint[]>() : new uint[capacity][];
        }
    }
    
    public struct AutoGrowOnlyListULong
    {
        internal const uint DefaultStep = 128;
        internal readonly uint step;
        internal ulong[][] data;

        public AutoGrowOnlyListULong(uint step = DefaultStep, uint capacity = 0)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<ulong[]>() : new ulong[capacity][];
        }
    }

    public static class AutoGrowOnlyListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint At(this ref AutoGrowOnlyListUInt list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.step);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new uint[list.step];
            }

            list.data[dataIndex] ??= new uint[list.step];
            return ref list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AtRO(this ref AutoGrowOnlyListUInt list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length) return 0U;
            if (list.data[dataIndex] == null) return 0U;

            return list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref AutoGrowOnlyListUInt list, in int capacity)
        {
            var n = (int)(capacity / list.step + 1);
            if (n > list.data.Length) // resize container array
            {
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new uint[list.step];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ulong At(this ref AutoGrowOnlyListULong list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.step);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new ulong[list.step];
            }

            list.data[dataIndex] ??= new ulong[list.step];
            return ref list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AtRO(this ref AutoGrowOnlyListULong list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length) return 0UL;
            if (list.data[dataIndex] == null) return 0UL;

            return list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref AutoGrowOnlyListULong list, in int capacity)
        {
            var n = (int)(capacity / list.step + 1);
            if (n > list.data.Length) // resize container array
            {
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new ulong[list.step];
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref AutoGrowOnlyList<T> list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.step);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new T[list.step];
            }

            list.data[dataIndex] ??= new T[list.step];
            return ref list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AtRO<T>(this ref AutoGrowOnlyList<T> list, uint index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length) return default;
            if (list.data[dataIndex] == null) return default;

            return list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref AutoGrowOnlyList<T> list, in int capacity)
        {
            var n = (int)(capacity / list.step + 1);
            if (n > list.data.Length) // resize container array
            {
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new T[list.step];
            }
        }
    }
}