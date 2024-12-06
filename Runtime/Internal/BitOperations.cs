using System.Runtime.CompilerServices;

namespace Xeno.Vendor
{
    internal static class BitOperations {
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
            if ((value & 0xFFFFFFFFul) != 0) { value >>= 32; log += 32; }
            if ((value & 0xFFFF0000ul) != 0) { value >>= 16; log += 16; }
            if ((value & 0xFF00ul) != 0) { value >>= 8; log += 8; }
            if ((value & 0xF0ul) != 0) { value >>= 4; log += 4; }
            if ((value & 0xCul) != 0) { value >>= 2; log += 2; }
            if ((value & 0x2ul) != 0) log += 1;

            return log;
        }
    }
}
