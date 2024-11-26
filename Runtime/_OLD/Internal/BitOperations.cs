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
    }
}
