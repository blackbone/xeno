using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    internal struct SwapBackList<T>
    {
        private const uint DefaultStep = 4;
        internal readonly uint step;

        internal bool allocated;
        internal uint count;
        internal uint capacity;
        internal T[][] data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SwapBackList(uint step = DefaultStep)
        {
            this.step = step;

            allocated = true;
            count = 0;
            capacity = 0;
            data = Array.Empty<T[]>();
        }
    }
    
    internal static class SwapBackListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Count<T>(this ref SwapBackList<T> list) => list.count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this ref SwapBackList<T> list, uint index)
        {
            var blockIndex = index / list.step;
            list.data[blockIndex] ??= new T[list.step];
            return ref list.data[blockIndex][index % list.step];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ref SwapBackList<T> list, in T value)
        {
            if (list.count >= list.capacity) // resize container array
            {
                var idx = list.data.Length;
                Array.Resize(ref list.data, idx + 32);
                list.data[idx] = new T[list.step];
                list.capacity = (uint)((idx + 32) * list.step);
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

            // check last array empty
            if (list.count % list.step == 0)
                list.data[list.count / list.step] = null; // GC will handle this i hope

            return value;
        }
    }
}