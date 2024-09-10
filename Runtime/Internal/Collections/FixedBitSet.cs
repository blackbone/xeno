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
    public unsafe struct FixedBitSet : IEquatable<FixedBitSet>
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
        
        public override int GetHashCode()
        {
#if VECTORIZATION
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                return v512.GetHashCode();
            }
#endif
#if NET5_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                return HashCode.Combine(v256_1.GetHashCode(), v256_2.GetHashCode());
            }
            
            if (Vector128.IsHardwareAccelerated)
            {
                return HashCode.Combine(v128_1.GetHashCode(), v128_2.GetHashCode(), v128_3.GetHashCode(), v128_4.GetHashCode());
            }
#endif
#endif
            
            return HashCode.Combine(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7]);
        }

        public bool Equals(FixedBitSet other)
        {
#if VECTORIZATION
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
            {
                return v512.Equals(other.v512);
            }
#endif
#if NET5_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
            {
                return v256_1.Equals(other.v256_1) && v256_2.Equals(other.v256_2);
            }
            
            if (Vector128.IsHardwareAccelerated)
            {
                return v128_1.Equals(other.v128_1) && v128_2.Equals(other.v128_2) && v128_3.Equals(other.v128_3) && v128_4.Equals(other.v128_4);
            }
#endif
#endif
            
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

    public sealed class FixedBitSetComparer : IEqualityComparer<FixedBitSet>
    {
        [ThreadStatic] private static FixedBitSetComparer _shared;
        
        public static FixedBitSetComparer Shared => _shared ??= new FixedBitSetComparer();
        
        public unsafe bool Equals(FixedBitSet x, FixedBitSet y)
        {
#if VECTORIZATION
#if NET8_0_OR_GREATER
            if (Vector512.IsHardwareAccelerated)
                return x.v512.Equals(y.v512);
#endif
#if NET5_0_OR_GREATER
            if (Vector256.IsHardwareAccelerated)
                return x.v256_1.Equals(y.v256_1) && x.v256_2.Equals(y.v256_2);

            if (Vector128.IsHardwareAccelerated)
                return x.v128_1.Equals(y.v128_1)
                       && x.v128_2.Equals(y.v128_2)
                       && x.v128_3.Equals(y.v128_3)
                       && x.v128_4.Equals(y.v128_4);
#endif
#endif

            return new Span<ulong>(x.data, FixedBitSet.MASK_ULONG_SIZE)
                .SequenceEqual(new Span<ulong>(y.data, FixedBitSet.MASK_ULONG_SIZE));

        }

        public int GetHashCode(FixedBitSet obj)
            => obj.GetHashCodeRef();
    }

    internal static class FixedBitSetExtensions
    {
        private const uint DIVISION_MASK = 0b00000000_00000000_00000000_00111111;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetHashCodeRef(this ref FixedBitSet origin) => origin.GetHashCode();
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe bool Get(this ref FixedBitSet origin, in int index)
            => (origin.data[index >> 6] & 1ul << (int)(index & DIVISION_MASK)) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Set(this ref FixedBitSet origin, in int index)
        {
            origin.data[index >> 6] |= 1ul << (int)(index & DIVISION_MASK);
            return ref origin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe ref FixedBitSet Unset(this ref FixedBitSet origin, in int index)
        {
            origin.data[index >> 6] &= ~(1ul << (int)(index & DIVISION_MASK));
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