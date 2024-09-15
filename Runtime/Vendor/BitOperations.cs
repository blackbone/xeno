using System.Runtime.CompilerServices;

namespace Xeno.Vendor
{
    internal static class BitOperations
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCap(ulong value) {
            // Do the smearing which turns (for example)
            // this: 0000 0101 0011
            // into: 0000 0111 1111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value |= value >> 32;

            return PopCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCap(uint value) {
            // Do the smearing which turns (for example)
            // this: 0000 0101 0011
            // into: 0000 0111 1111
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;

            return PopCount(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(uint value) {
            const uint c1 = 0x_55555555u;
            const uint c2 = 0x_33333333u;
            const uint c3 = 0x_0F0F0F0Fu;
            const uint c4 = 0x_01010101u;

            value -= (value >> 1) & c1;
            value = (value & c2) + ((value >> 2) & c2);
            value = (((value + (value >> 4)) & c3) * c4) >> 24;

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint PopCount(ulong value) {
            const ulong c1 = 0x_55555555_55555555ul;
            const ulong c2 = 0x_33333333_33333333ul;
            const ulong c3 = 0x_0F0F0F0F_0F0F0F0Ful;
            const ulong c4 = 0x_01010101_01010101ul;

            value -= (value >> 1) & c1;
            value = (value & c2) + ((value >> 2) & c2);
            value = (((value + (value >> 4)) & c3) * c4) >> 56;

            return (uint)value;
        }
    }
}