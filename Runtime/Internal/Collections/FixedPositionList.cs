using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno.Collections {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FixedPositionList<T> {
        public readonly int capacityGrow;

        public UIntStack freeIndices;
        public T[] data;
        public BitSet mask;
        public uint count;
        public uint cap;

        public FixedPositionList(int capacity = Constants.DefaultCapacity, int capacityGrow = Constants.DefaultCapacityGrow) {
            this.capacityGrow = capacityGrow;

            freeIndices = new UIntStack(capacity, capacity);
            data = capacity > 0 ? new T[capacity] : Array.Empty<T>();
            mask = new BitSet(capacityGrow);
            count = 0;
            cap = 0;
        }
    }

    internal static class FixedPositionListExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Add<T>(this ref FixedPositionList<T> list, in T value) {
            var index = uint.MaxValue;
            if (list.freeIndices.Pop(ref index)) {
                list.data[index] = value;
                list.mask.Set(index);
                list.count++;
                return 0;
            }

            index = list.count;
            if (index >= list.data.Length) {
                Array.Resize(ref list.data, list.data.Length + list.capacityGrow);
            }

            list.data[index] = value;
            list.mask.Set(index);
            list.count++;
            list.cap = list.mask.PopCap();
            return index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remove<T>(this ref FixedPositionList<T> list, in uint index) {
            list.freeIndices.Push(index);
            list.mask.Unset(index);
            list.count--;
            list.cap = list.mask.PopCap();
            return list.data[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ref FixedPositionList<T> list, in int capacity) {
            list.mask.Ensure((uint)capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get<T>(this ref FixedPositionList<T> list, in T value, ref uint index) {
            for (uint i = 0; i < list.count; i++) {
                if (!list.mask.Get(i)) continue;

                if (!list.data[i].Equals(value)) continue;

                index = i;
                return true;
            }

            return false;
        }
    }
}