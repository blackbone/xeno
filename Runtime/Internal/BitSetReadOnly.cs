using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xeno.Vendor;

namespace Xeno {
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BitSetReadOnly : IEquatable<BitSetReadOnly> {
        private static readonly ulong[] emptyUlong = { 0 };
        private static readonly uint[] emptyUInt = { };

        public static BitSetReadOnly Zero = new(0);

        public readonly int max;
        public readonly int maskSize;
        public readonly ulong hash;
        public readonly ulong[] data;
        public readonly uint[] indices;

        private BitSetReadOnly(int _) {
            hash = 0;
            max = 0;
            maskSize = 0;
            data = emptyUlong;
            indices = emptyUInt;
        }

        public BitSetReadOnly(ref BitSet set) {
            hash = set.hash;
            data = new ulong[set.data.Length];
            set.data.CopyTo(data);
            max = set.max;
            maskSize = set.maskSize;

            Span<uint> buffer = stackalloc uint[Constants.MaxArchetypeComponents];
            set.GetIndices(ref buffer, out var count);
            indices = buffer[..count].ToArray();
        }

        public bool Equals(BitSetReadOnly other) => max == other.max && hash == other.hash && DataEquals(data, other.data, max);
        public bool Equals(BitSet other) => max == other.max && hash == other.hash && DataEquals(data, other.data, max);
        public override bool Equals(object obj) => obj is BitSetReadOnly other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(max, hash);
        public override string ToString() => $"{BitSetExtensions.ToS(data)}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DataEquals(ulong[] left, ulong[] right, int max) {
            var words = BitSet.MaskSize(max);
            for (var i = 0; i < words; i++) {
                var leftWord = i < left.Length ? left[i] : 0ul;
                var rightWord = i < right.Length ? right[i] : 0ul;
                if (leftWord != rightWord)
                    return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool DataEquals(ulong[] left, ReadOnlySpan<ulong> right, int max) {
            var words = BitSet.MaskSize(max);
            for (var i = 0; i < words; i++) {
                var leftWord = i < left.Length ? left[i] : 0ul;
                var rightWord = i < right.Length ? right[i] : 0ul;
                if (leftWord != rightWord)
                    return false;
            }

            return true;
        }
    }

    internal static class BitSetReadOnlyExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this ref BitSetReadOnly set, int index) {
            return (set.data[index >> Constants.LONG_DIVIDER] & 1ul << (index & Constants.LONG_DIVISION_MASK)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Cross(this ref BitSetReadOnly set, ref BitSetReadOnly other) {
            var max = set.max < other.max ? set.max : other.max;
            var length = BitSet.MaskSize(max);

            for (var i = 0; i < length; i++)
            {
                var setWord = i < set.data.Length ? set.data[i] : 0ul;
                var otherWord = i < other.data.Length ? other.data[i] : 0ul;
                if ((setWord & otherWord) != 0)
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Includes(this ref BitSetReadOnly set, ref BitSetReadOnly other) {
            if (other.max > set.max) return false;

            var length = BitSet.MaskSize(other.max);
            for (var i = 0; i < length; i++)
            {
                var setWord = i < set.data.Length ? set.data[i] : 0ul;
                var otherWord = i < other.data.Length ? other.data[i] : 0ul;
                if ((setWord & otherWord) != otherWord)
                    return false;
            }

            return true;
        }

        internal static IEnumerable<uint> GetIndices(this BitSetReadOnly set) {
            var l = set.data.Length;
            for (var i = 0; i < l; i++) {
                var v = set.data[i];

                while (v != 0) {
                    var offset = BitOperations.TrailingZeroCount64(v);
                    yield return (uint)(i * Constants.LongBitSize + offset);
                    v &= v - 1;
                }
            }
        }

        internal static string ToS(this ref BitSetReadOnly bitSet) => ToS(bitSet.data);

        internal static string ToS(this ulong[] data) {
            var sb = new StringBuilder();
            sb.Clear();

            foreach (var v in data) {
                sb.Insert(0, $".{v:b64}");
            }
            sb.Remove(0, 1);

            return $"[{sb}]";
        }
    }
}
