using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Xeno {
    [StructLayout(LayoutKind.Sequential)]
    internal  readonly struct BitSetReadOnly : IEquatable<BitSetReadOnly> {
        private static readonly ulong[] emptyUlong = { 0 };
        private static readonly uint[] emptyUInt = { };

        public static BitSetReadOnly Zero = new(0);

        public readonly int max;
        public readonly ulong hash;
        public readonly ulong[] data;
        public readonly uint[] indices;

        private BitSetReadOnly(int _) {
            hash = 0;
            max = 0;
            data = emptyUlong;
            indices = emptyUInt;
        }

        public BitSetReadOnly(ref BitSet set) {
            hash = set.hash;
            data = new ulong[set.data.Length];
            set.data.CopyTo(data);
            max = set.max;

            Span<uint> buffer = stackalloc uint[Constants.MaxArchetypeComponents];
            set.GetIndices(ref buffer, out var count);
            indices = buffer[..count].ToArray();
        }

        public bool Equals(BitSetReadOnly other) => max == other.max && hash == other.hash;
        public bool Equals(BitSet other) => max == other.max && hash == other.hash;
        public override bool Equals(object obj) => obj is BitSetReadOnly other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(max, hash);
        public override string ToString() => $"{BitSetExtensions.ToS(data)}";
    }

    internal static class BitSetReadOnlyExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this ref BitSetReadOnly set, int index) {
            return (set.data[index >> Constants.LONG_DIVIDER] & 1ul << (index & Constants.LONG_DIVISION_MASK)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Includes(this ref BitSetReadOnly set, ref BitSetReadOnly other) {
            if (set.hash == other.hash) return true;
            if (other.data.Length > set.data.Length) return false;

            for (var i = 0; i < other.data.Length; i++)
            {
                if ((set.data[i] & other.data[i]) != other.data[i])
                    return false;
            }

            return true;
        }

        internal static IEnumerable<uint> GetIndices(this BitSetReadOnly set) {
            var l = set.data.Length;
            uint u_i = 0;
            for (var i = 0; i < l; i++, u_i++) {
                var v = set.data[i];

                var k = 0u;
                while (v != 0) {
                    if ((v & 1ul) == 1ul) yield return  u_i * Constants.LongBitSize + k;
                    v >>= 1;
                    k++;
                }
            }
        }

        internal static string ToS(this ref BitSetReadOnly bitSet) => ToS(bitSet.data);

        internal static string ToS(this ulong[] data) {
            var _sb = new StringBuilder();
            _sb.Clear();

            foreach (var v in data) {
                _sb.Insert(0, $".{v:b64}");
            }
            _sb.Remove(0, 1);

            return $"[{_sb}]";
        }
    }
}
