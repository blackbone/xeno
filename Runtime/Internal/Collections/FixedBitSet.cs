using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET5_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif

namespace Xeno.Collections
{
    [StructLayout(LayoutKind.Explicit)]
    // ReSharper disable once DefaultStructEqualityIsUsed.Global
#pragma warning disable CS0660, CS0661
    internal unsafe struct FixedBitSet : IEquatable<FixedBitSet>
#pragma warning restore CS0660, CS0661
    {
        public static readonly FixedBitSet Zero = new();

        internal const int MASK_BIT_SIZE = 512;
        internal const int MASK_ULONG_SIZE = MASK_BIT_SIZE / (sizeof(ulong) * 8);
        
        [FieldOffset(0)] internal fixed ulong data[MASK_ULONG_SIZE]; // 512 flags must be enough
#if VECTORIZATION
#if NET8_0_OR_GREATER
        [FieldOffset(0)] internal Vector512<ulong> v512;
#endif
#if NET5_0_OR_GREATER
        [FieldOffset(0)] internal Vector256<ulong> v256_1;
        [FieldOffset(256)] internal Vector256<ulong> v256_2;
        
        [FieldOffset(0)] internal Vector128<ulong> v128_1;
        [FieldOffset(128)] internal Vector128<ulong> v128_2;
        [FieldOffset(256)] internal Vector128<ulong> v128_3;
        [FieldOffset(384)] internal Vector128<ulong> v128_4;
#endif
#endif
        [FieldOffset(512)] internal int hash;
        public override string ToString() => $"{data[7]:B}{data[6]:B}{data[5]:B}{data[4]:B}{data[3]:B}{data[2]:B}{data[1]:B}{data[0]:B}";
        public bool Equals(FixedBitSet other) => hash == other.hash;
        public override bool Equals(object obj) => obj is FixedBitSet other && Equals(other);
        public override int GetHashCode() => hash;
    }

    internal static class FixedBitSetExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Finalize(this ref FixedBitSet origin) {
            var hash = (long)(origin.data[0]
                ^ origin.data[1]
                ^ origin.data[2]
                ^ origin.data[3]
                ^ origin.data[4]
                ^ origin.data[5]
                ^ origin.data[6]
                ^ origin.data[7]);
            origin.hash = (int)(hash >> 32) ^ (int)hash;
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Get(this ref FixedBitSet origin, in int index)
            => (origin.data[index >> Constants.LONG_DIVIDER] & 1ul << (index & Constants.LONG_DIVISION_MASK)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Set(this ref FixedBitSet origin, in int index)
        {
            origin.data[index >> Constants.LONG_DIVIDER] |= 1ul << (index & Constants.LONG_DIVISION_MASK);
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Unset(this ref FixedBitSet origin, in int index)
        {
            origin.data[index >> Constants.LONG_DIVIDER] &= ~(1ul << (index & Constants.LONG_DIVISION_MASK));
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Reset(this ref FixedBitSet origin)
        {
#if VECTORIZATION
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                origin.v512 = Vector512<ulong>.Zero;
                return ref origin;
            }
#endif
#if NET5_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                origin.v256_1 = Vector256<ulong>.Zero;
                origin.v256_2 = Vector256<ulong>.Zero;
                return ref origin;
            }
            
            if (Vector128.IsHardwareAccelerated)
            {
                origin.v128_1 = Vector128<ulong>.Zero;
                origin.v128_2 = Vector128<ulong>.Zero;
                origin.v128_3 = Vector128<ulong>.Zero;
                origin.v128_4 = Vector128<ulong>.Zero;
                return ref origin;
            }
#endif
#endif
            for (var i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
                origin.data[i] = 0;
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Includes(this ref FixedBitSet origin, in FixedBitSet other)
        {
#if VECTORIZATION
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                return (origin.v512 & other.v512) == other.v512;
            }
#endif
#if NET5_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                var mask1 = origin.v256_1 & other.v256_1;
                var mask2 = origin.v256_2 & other.v256_2;
                return mask1.Equals(other.v256_1) && mask2.Equals(other.v256_2);
            }

            if (Vector128.IsHardwareAccelerated)
            {
                var mask1 = origin.v128_1 & other.v128_1;
                var mask2 = origin.v128_2 & other.v128_2;
                var mask3 = origin.v128_3 & other.v128_3;
                var mask4 = origin.v128_4 & other.v128_4;
                return mask1.Equals(other.v128_1) && mask2.Equals(other.v128_2) &&
                    mask3.Equals(other.v128_3) && mask4.Equals(other.v128_4);
            }
#endif
#endif

            // Non-vectorized path: loop and early return on mismatch
            for (int i = 0; i < FixedBitSet.MASK_ULONG_SIZE; i++)
            {
                if ((origin.data[i] & other.data[i]) != other.data[i])
                    return false;
            }

            return true;
        }
    }
}