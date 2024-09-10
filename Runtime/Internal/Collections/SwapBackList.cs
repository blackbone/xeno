using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SwapBackList<T>
    {
        internal readonly int step;

        internal int count;
        internal int capacity;
        internal T[] data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwapBackList(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity)
        {
            this.step = step;

            this.step = step;
            this.capacity = capacity;
            data = capacity > 0 ? new T[capacity] : Array.Empty<T>();
            count = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SwapBackListUInt
    {
        internal readonly int step;

        internal int count;
        internal int capacity;
        internal uint[] data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwapBackListUInt(in int step = Constants.DefaultStep, in int capacity = Constants.DefaultCapacity)
        {
            this.step = step;

            this.step = step;
            this.capacity = capacity;
            data = capacity > 0 ? new uint[capacity] : Array.Empty<uint>();
            count = 0;
        }
    }

    internal static class SwapBackListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref SwapBackList<T> list, uint index) => ref list.data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref SwapBackList<T> list, in T value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var newCapacity = list.count + list.step;
                Array.Resize(ref list.data, newCapacity);
                list.capacity = newCapacity;
            }

            list.data[list.count] = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RemoveAtAndSwapBack<T>(this ref SwapBackList<T> list, in uint index)
        {
            var value = list.data[(int)index];
            list.count--;

            // if not last element
            if (index < list.count)
                list.data[(int)index] = list.data[list.count];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref SwapBackList<T> list, in int capacity) where T : struct
        {
            var count = (capacity / list.step + 1) * list.step;
            if (count >= list.data.Length) Array.Resize(ref list.data, count);
        }
    }

    internal static class SwapBackListUIntExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref uint At(this ref SwapBackListUInt list, in int index) => ref list.data[index];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RemoveAtAndSwapBack(this ref SwapBackListUInt list, in int index)
        {
            var value = list.At(index);
            list.count--;

            // if not last element
            if (index < list.count)
                list.At(index) = list.At(list.count);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this ref SwapBackListUInt list, in uint value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var newCapacity = list.count + list.step;
                Array.Resize(ref list.data, newCapacity);
                list.capacity = newCapacity;
            }

            list.At(list.count) = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<uint> GetSpan(this ref SwapBackListUInt list) => new(list.data, 0, list.count);
    }
}