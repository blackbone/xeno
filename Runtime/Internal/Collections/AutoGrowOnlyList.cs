using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct AutoGrowOnlyList<T>
    {
        internal readonly int step;
        internal T[][] data;

        public AutoGrowOnlyList(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultStep)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<T[]>() : new T[capacity][];
        }
    }
    
    public struct AutoGrowOnlyListUInt
    {
        internal readonly int step;
        internal uint[] data;

        public AutoGrowOnlyListUInt(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultStep)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<uint>() : new uint[capacity];
        }
    }
    
    public struct AutoGrowOnlyListULong
    {
        internal readonly int step;
        internal ulong[][] data;

        public AutoGrowOnlyListULong(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultStep)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<ulong[]>() : new ulong[capacity][];
        }
    }

    public static class AutoGrowOnlyListExtensions
    {
        #region UInt

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint At(this ref AutoGrowOnlyListUInt list, in int index)
        {
            var size = list.data.Length;
            if (index >= size)
            {
                var newSize = (index / list.step + 1) * list.step;
                Array.Resize(ref list.data, newSize);
            }
            return ref list.data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint At(this ref AutoGrowOnlyListUInt list, in uint index)
        {
            var size = list.data.Length;
            if (index >= size)
            {
                var newSize = (index / list.step + 1) * list.step;
                Array.Resize(ref list.data, (int)newSize);
            }
            return ref list.data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref AutoGrowOnlyListUInt list, in int capacity)
        {
            var size = list.data.Length;
            if (capacity >= size)
            {
                var newSize = (capacity / list.step + 1) * list.step;
                Array.Resize(ref list.data, newSize);
            }
        }
        
        #endregion

        #region ULong

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ulong At(this ref AutoGrowOnlyListULong list, in int index)
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
        public static void Ensure(this ref AutoGrowOnlyListULong list, in int capacity)
        {
            var n = capacity / list.step + 1;
            if (n > list.data.Length) // resize container array
            {
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new ulong[list.step];
            }
        }
        #endregion

        #region Generic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref AutoGrowOnlyList<T> list, in int index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = dataIndex + list.step;
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new T[list.step];
            }

            list.data[dataIndex] ??= new T[list.step];
            return ref list.data[dataIndex][index % list.step];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AtRO<T>(this ref AutoGrowOnlyList<T> list, int index)
        {
            var dataIndex = index / list.step;
            if (dataIndex >= list.data.Length) return default;
            if (list.data[dataIndex] == null) return default;

            return list.data[dataIndex][index % list.step];
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
            var n = capacity / list.step + 1;
            if (n > list.data.Length) // resize container array
            {
                Array.Resize(ref list.data, n);
                var i = 0;
                while (i < n) list.data[i++] ??= new T[list.step];
            }
        }
        
        #endregion
    }
}