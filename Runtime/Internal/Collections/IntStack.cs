using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno.Collections {

    [StructLayout(LayoutKind.Sequential)]
    internal struct XStack<T> {
        public readonly int capacityGrow;

        public T[] data;
        public int count;

        public XStack(int capacity = Constants.DefaultCapacity, int capacityGrow = Constants.DefaultCapacityGrow) {
            this.capacityGrow = capacityGrow;
            data = capacity > 0 ? new T[capacity] : Array.Empty<T>();
            count = 0;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct UIntStack {
        public readonly int capacityGrow;

        public uint[] data;
        public int count;

        public UIntStack(int capacity = Constants.DefaultCapacity, int capacityGrow = Constants.DefaultCapacityGrow) {
            this.capacityGrow = capacityGrow;
            data = capacity > 0 ? new uint[capacity] : Array.Empty<uint>();
            count = 0;
        }
    }

    internal static class XStackExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push<T>(this ref XStack<T> stack, in T value) {
            if (stack.count >= stack.data.Length) {
                Array.Resize(ref stack.data, stack.data.Length + stack.capacityGrow);
            }

            stack.data[stack.count] = value;
            stack.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Pop<T>(this ref XStack<T> stack, ref T value) {
            if (stack.count == 0)
                return false;

            stack.count--;
            value = stack.data[stack.count];
            stack.data[stack.count] = default;
            return true;
        }
    }

    internal static class UIntStackExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Push(this ref UIntStack stack, in uint value) {
            if (stack.count >= stack.data.Length) {
                Array.Resize(ref stack.data, stack.data.Length + stack.capacityGrow);
            }

            stack.data[stack.count] = value;
            stack.count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Pop(this ref UIntStack stack, ref uint value) {
            if (stack.count == 0)
                return false;

            stack.count--;
            value = stack.data[stack.count];
            return true;
        }
    }
}