using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct AutoGrowOnlyList<T>
    {
        internal const uint DefaultStep = 4;
        internal readonly uint density;
        internal T[][] data;

        public AutoGrowOnlyList(uint capacity, uint density = DefaultStep)
        {
            this.density = density;
            data = capacity == 0 ? Array.Empty<T[]>() : new T[capacity][];
        }
    }
    
    public struct AutoGrowOnlyListUInt
    {
        internal const uint DefaultDensity = 4;
        internal readonly uint density;
        internal uint[][] data;

        public AutoGrowOnlyListUInt(uint density = DefaultDensity, uint capacity = 0)
        {
            this.density = density;
            data = capacity == 0 ? Array.Empty<uint[]>() : new uint[capacity][];
        }
    }
    
    public struct AutoGrowOnlyListULong
    {
        internal const uint DefaultDensity = 4;
        internal readonly uint density;
        internal ulong[][] data;

        public AutoGrowOnlyListULong(uint density = DefaultDensity, uint capacity = 0)
        {
            this.density = density;
            data = capacity == 0 ? Array.Empty<ulong[]>() : new ulong[capacity][];
        }
    }

    public static class AutoGrowOnlyListExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint At(this ref AutoGrowOnlyListUInt list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.density);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new uint[list.density];
            }

            list.data[dataIndex] ??= new uint[list.density];
            return ref list.data[dataIndex][index % list.density];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AtRO(this ref AutoGrowOnlyListUInt list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length) return 0U;
            if (list.data[dataIndex] == null) return 0U;

            return list.data[dataIndex][index % list.density];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref ulong At(this ref AutoGrowOnlyListULong list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.density);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new ulong[list.density];
            }

            list.data[dataIndex] ??= new ulong[list.density];
            return ref list.data[dataIndex][index % list.density];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AtRO(this ref AutoGrowOnlyListULong list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length) return 0UL;
            if (list.data[dataIndex] == null) return 0UL;

            return list.data[dataIndex][index % list.density];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref AutoGrowOnlyList<T> list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length)
            {
                var size = list.data.Length;
                var newSize = (int)(dataIndex + list.density);
                Array.Resize(ref list.data, newSize);
                while (size < newSize)
                    list.data[size++] = new T[list.density];
            }

            list.data[dataIndex] ??= new T[list.density];
            return ref list.data[dataIndex][index % list.density];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AtRO<T>(this ref AutoGrowOnlyList<T> list, uint index)
        {
            var dataIndex = index / list.density;
            if (dataIndex >= list.data.Length) return default;
            if (list.data[dataIndex] == null) return default;

            return list.data[dataIndex][index % list.density];
        }
    }
}