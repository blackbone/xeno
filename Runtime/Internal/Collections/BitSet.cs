using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xeno.Vendor;

namespace Xeno.Collections
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BitSet {
        public ulong[] data;

        public BitSet(int capacity) => data = capacity > 0 ? new ulong[capacity] : Array.Empty<ulong>();
    }

    internal static class BitSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set(this ref BitSet bitSet, in uint index) {
            bitSet.Ensure(index);
            bitSet.data[index >> Constants.LONG_DIVIDER] |= 1ul << ((int)index & Constants.LONG_DIVISION_MASK);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unset(this ref BitSet bitSet, in uint index) {
            if (bitSet.data.Length < index >> Constants.LONG_DIVIDER)
                return;

            bitSet.data[index >> Constants.LONG_DIVIDER] &= ~(1ul << ((int)index & Constants.LONG_DIVISION_MASK));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this ref BitSet bitSet, in uint index) {
            if (bitSet.data.Length < index >> Constants.LONG_DIVIDER)
                return false;

            return (bitSet.data[index >> Constants.LONG_DIVIDER] & 1ul << ((int)index & Constants.LONG_DIVISION_MASK)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure(this ref BitSet bitSet, in uint capacity) {
            var size = capacity >> Constants.LONG_DIVIDER + 1;
            if (bitSet.data.Length < size) Array.Resize(ref bitSet.data, (int)size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCap(this ref BitSet bitSet) {
            for (var i = bitSet.data.Length - 1; i >= 0; i--) {
                if (bitSet.data[i] == 0ul) continue;

                return (uint)i * Constants.LONG_SIZE + BitOperations.PopCap(bitSet.data[i]);
            }

            return 0;
        }
    }
}