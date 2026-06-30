using System.Runtime.CompilerServices;

namespace Xeno.Vendor
{
    internal static class BitOperations {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Smear(int value) {
            // Do the smearing which turns (for example)
            // this: 0000 0101 0011
            // into: 0000 0111 1111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Smear(uint value) {
            // Do the smearing which turns (for example)
            // this: 0000 0101 0011
            // into: 0000 0111 1111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong Smear(ulong value) {
            // Do the smearing which turns (for example)
            // this: 0000 0101 0011
            // into: 0000 0111 1111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(ulong value) {
            if (value <= 0) return -1;

            var log = 0;
            if ((value & 0xFFFFFFFF00000000ul) != 0) { value >>= 32; log += 32; }
            if ((value & 0xFFFF0000ul) != 0) { value >>= 16; log += 16; }
            if ((value & 0xFF00ul) != 0) { value >>= 8; log += 8; }
            if ((value & 0xF0ul) != 0) { value >>= 4; log += 4; }
            if ((value & 0xCul) != 0) { value >>= 2; log += 2; }
            if ((value & 0x2ul) != 0) log += 1;

            return log;
        }

        private static readonly int[] TrailingZeroCount64Index = {
            0, 1, 2, 53, 3, 7, 54, 27, 4, 38, 41, 8, 34, 55, 48, 28,
            62, 5, 39, 46, 44, 42, 22, 9, 24, 35, 59, 56, 49, 18, 29, 11,
            63, 52, 6, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
            51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int TrailingZeroCount64(ulong value) {
            unchecked {
                var isolated = value & (0ul - value);
                return TrailingZeroCount64Index[(isolated * 0x022fdd63cc95386dul) >> 58];
            }
        }
    }
}
