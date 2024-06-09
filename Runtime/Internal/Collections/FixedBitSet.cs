using System.Runtime.CompilerServices;

namespace Xeno.Collections
{
    public unsafe struct FixedBitSet
    {
        internal const int MASK_SIZE = 512;
        internal const int MASK_ULONG_SIZE = MASK_SIZE / sizeof(ulong);
        internal fixed ulong data[MASK_ULONG_SIZE]; // 512 flags must be enough
    }

    internal static class FixedBitSetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Get(this ref FixedBitSet origin, uint index) => (origin.data[index / 64] & 1ul << (int)(index % 64)) != 0;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Set(this ref FixedBitSet origin, uint index) => origin.data[index / 64] |= 1ul << (int)(index % 64);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Unset(this ref FixedBitSet origin, uint index) => origin.data[index / 64] |= 1ul << (int)(index % 64);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Reset(this ref FixedBitSet origin)
        {
            for (var i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
                origin.data[i] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Includes(this ref FixedBitSet origin, in FixedBitSet other)
        {
            var result = true;
            for (var i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
                result &= (origin.data[i] & other.data[i]) == other.data[i];
            return result;
        }
    }
}