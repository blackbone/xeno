using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public unsafe struct FixedBitSet
    {
        internal const int MASK_SIZE = 512;
        internal const int MASK_ULONG_SIZE = MASK_SIZE / (sizeof(ulong) * 8);
        internal fixed ulong data[MASK_ULONG_SIZE]; // 512 flags must be enough
    }

    internal static class FixedBitSetExtensions
    {
        private const uint DIVISION_MASK = 0b00000000_00000000_00000000_00111111;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Get(this ref FixedBitSet origin, uint index) => (origin.data[index >> 6] & 1ul << (int)(index & DIVISION_MASK)) != 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Set(this ref FixedBitSet origin, uint index) => origin.data[index >> 6] |= 1ul << (int)(index & DIVISION_MASK);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Unset(this ref FixedBitSet origin, uint index) => origin.data[index >> 6] |= 1ul << (int)(index & DIVISION_MASK);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Reset(this ref FixedBitSet origin)
        {
            for (var i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
                origin.data[i] = 0;
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