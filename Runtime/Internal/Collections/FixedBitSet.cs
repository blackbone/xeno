using System;
using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public unsafe struct FixedBitSet : IEquatable<FixedBitSet>
    {
        internal const int MASK_SIZE = 512;
        internal const int MASK_ULONG_SIZE = MASK_SIZE / (sizeof(ulong) * 8);
        internal fixed ulong data[MASK_ULONG_SIZE]; // 512 flags must be enough
        
        public override int GetHashCode() =>
            HashCode.Combine(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);

        public bool Equals(FixedBitSet other)
        {
            return data[0] == other.data[0]
                   && data[1] == other.data[1]
                   && data[2] == other.data[2]
                   && data[3] == other.data[3]
                   && data[4] == other.data[4]
                   && data[5] == other.data[5]
                   && data[6] == other.data[6]
                   && data[7] == other.data[7];
        }

        public override bool Equals(object obj) => obj is FixedBitSet other && Equals(other);

        public static bool operator ==(FixedBitSet a, FixedBitSet b) => a.Equals(b);
        public static bool operator !=(FixedBitSet a, FixedBitSet b) => !a.Equals(b);
    }

    internal static class FixedBitSetExtensions
    {
        private const uint DIVISION_MASK = 0b00000000_00000000_00000000_00111111;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Get(this ref FixedBitSet origin, uint index) => (origin.data[index >> 6] & 1ul << (int)(index & DIVISION_MASK)) != 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Set(this ref FixedBitSet origin, uint index)
        {
            origin.data[index >> 6] |= 1ul << (int)(index & DIVISION_MASK);
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Unset(this ref FixedBitSet origin, uint index)
        {
            origin.data[index >> 6] |= 1ul << (int)(index & DIVISION_MASK);
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Reset(this ref FixedBitSet origin)
        {
            for (var i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
                origin.data[i] = 0;
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Includes(this ref FixedBitSet origin, in FixedBitSet other)
        {
            if ((origin.data[0] & other.data[0]) != other.data[0]) return false;
            if ((origin.data[1] & other.data[1]) != other.data[1]) return false;
            if ((origin.data[2] & other.data[2]) != other.data[2]) return false;
            if ((origin.data[3] & other.data[3]) != other.data[3]) return false;
            if ((origin.data[4] & other.data[4]) != other.data[4]) return false;
            if ((origin.data[5] & other.data[5]) != other.data[5]) return false;
            if ((origin.data[6] & other.data[6]) != other.data[6]) return false;
            if ((origin.data[7] & other.data[7]) != other.data[7]) return false;
            return true;
        }
    }
}