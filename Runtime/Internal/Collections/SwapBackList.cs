using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    internal struct SwapBackList<T>
    {
        private const uint DefaultStep = 1024;
        internal readonly uint step;

        internal bool allocated;
        internal uint count;
        internal uint capacity;
        internal T[] data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwapBackList(uint step = DefaultStep)
        {
            this.step = step;

            allocated = true;
            count = 0;
            capacity = 0;
            data = Array.Empty<T>();
        }
    }
    
    internal static class SwapBackListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Count<T>(this ref SwapBackList<T> list) => list.count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref SwapBackList<T> list, uint index) {
            return ref list.data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref SwapBackList<T> list, in T value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var newCapacity = list.count + list.step;
                Array.Resize(ref list.data, (int)newCapacity);
                list.capacity = newCapacity;
            }

            list.At(list.count) = value;
            list.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T RemoveAtAndSwapBack<T>(this ref SwapBackList<T> list, in uint index)
        {
            var value = list.At(index);
            list.count--;

            // if not last element
            if (index < list.count)
                list.At(index) = list.At(list.count);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref SwapBackList<T> list, in uint capacity) where T : struct
        {
            var count = (capacity / list.step + 1) * list.step;
            if (count >= list.data.Length) Array.Resize(ref list.data, (int)count);
        }
    }
}