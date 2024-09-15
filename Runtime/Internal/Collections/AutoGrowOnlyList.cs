using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public struct AutoGrowOnlyList<T>
    {
        internal readonly int step;
        internal T[] data;

        public AutoGrowOnlyList(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultStep)
        {
            this.step = step;
            data = capacity == 0 ? Array.Empty<T>() : new T[capacity];
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

    public static class AutoGrowOnlyListExtensions
    {
        #region UInt

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

        #region Generic

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref AutoGrowOnlyList<T> list, in int index)
        {
            if (index >= list.data.Length)
                Array.Resize(ref list.data, list.data.Length + list.step);

            return ref list.data[index];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T AtRO<T>(this ref AutoGrowOnlyList<T> list, int index) {
            return index >= list.data.Length ? default : list.data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref AutoGrowOnlyList<T> list, int capacity)
        {
            capacity = (capacity / list.step + 1) * list.step;
            if (capacity > list.data.Length)
                Array.Resize(ref list.data, capacity);
        }
        
        #endregion
    }
}