using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Xeno.Vendor;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    public ref struct BitSet {
        internal int max;
        internal ulong hash;
        internal Span<ulong> data;

        public BitSet(in Span<ulong> data) {
            this.data = data;
            this.hash = 0;
            max = 0;
        }

        public override string ToString() => $"{this.ToS()}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaskSize(int max1, int max2) {
            var v = max1 > max2 ? max1 : max2;
            return v switch {
                0 => 1,
                > Constants.MaxArchetypeComponents => throw new IndexOutOfRangeException(),
                _ => v / Constants.LongBitSize + 1
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaskSize(int max) {
            return max switch {
                0 => 1,
                >= Constants.MaxArchetypeComponents => throw new IndexOutOfRangeException(),
                _ => max / Constants.LongBitSize + 1
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Zero(ref BitSet set) {
            set.data = Span<ulong>.Empty;
            set.hash = 0;
            set.max = 0;
        }
    }

    internal static class BitSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BitSet Set(this ref BitSet set, int index) {
            set.data[index >> Constants.LONG_DIVIDER] |= 1ul << (index & Constants.LONG_DIVISION_MASK);
            return ref set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BitSet Unset(this ref BitSet set, int index) {
            set.data[index >> Constants.LONG_DIVIDER] &= ~(1ul << (index & Constants.LONG_DIVISION_MASK));
            return ref set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromAdd(this ref BitSet set, in BitSetReadOnly origin, in BitSetReadOnly add) {
            if (origin.data.Length == 0) {
                add.data.CopyTo(set.data);
                set.hash = add.hash;
                set.max = add.max;
                return;
            }

            var l1 = origin.data.Length;
            var l2 = add.data.Length;
            var i = 0;
            while (i < l1 && i < l2) {
                set.data[i] = origin.data[i] | add.data[i];
                i++;
            }

            while (i < l2) {
                set.data[i] = add.data[i];
                i++;
            }

            set.max = origin.max > add.max ? origin.max : add.max;
            set.FinalizeHash();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromRemove(this ref BitSet set, in BitSetReadOnly origin, in BitSetReadOnly remove) {
            if (origin.data.Length == 0) {
                BitSet.Zero(ref set);
                return;
            }

            var l1 = origin.data.Length;
            var l2 = remove.data.Length;
            var i = 0;
            while (i < l1 && i < l2) {
                set.data[i] = origin.data[i] & ~remove.data[i];
                i++;
            }

            while (i < l1) {
                set.data[i] = origin.data[i];
                i++;
            }

            set.FinalizeMax();
            set.FinalizeHash();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Get(this ref BitSet set, int index) {
            return (set.data[index >> Constants.LONG_DIVIDER] & 1ul << (index & Constants.LONG_DIVISION_MASK)) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FinalizeIndexJoin(this ref BitSet set) {
            var result = 0;
            var l = set.data.Length;
            for (var i = 0; i < l; i++) {
                var v = set.data[i];

                var k = 0;
                while (v != 0) {
                    if ((v & 1ul) == 1ul) result |= i * Constants.LongBitSize + k;
                    v >>= 1;
                    k++;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FinalizeMax(this ref BitSet set)
        {
            for (int i = set.data.Length - 1; i >= 0; i--) {
                var v = set.data[i];
                if (v != 0) {
                    set.max = i * 64 + BitOperations.Log2(v);
                    return;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FinalizeHash(this ref BitSet set) {
            switch (set.data.Length) {
                case 0:
                    set.hash = 0;
                    break;
                case 1:
                    set.hash = set.data[0];
                    break;
                case 2:
                    set.hash = set.data[0] ^ set.data[1];
                    break;
                case 3:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2];
                    break;
                case 4:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3];
                    break;
                case 5:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4];
                    break;
                case 6:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5];
                    break;
                case 7:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5] ^ set.data[6];
                    break;
                case 8:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4] ^ set.data[5] ^ set.data[6] ^ set.data[7];
                    break;
                default:
                    set.hash = 0;
                    for (var i = 0; i < set.data.Length; i++) {
                        set.hash ^= set.data[i];
                    }
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static BitSetReadOnly AsReadOnly(ref this BitSet set) => new(ref set);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void GetIndices(this in BitSet set, ref Span<uint> values, out int count) {
            var n = 0;
            var l = set.data.Length;
            uint u_i = 0;
            for (var i = 0; i < l; i++, u_i++) {
                var v = set.data[i];

                var k = 0u;
                while (v != 0) {
                    if ((v & 1ul) == 1ul) values[n++] = u_i * Constants.LongBitSize + k;
                    v >>= 1;
                    k++;
                }
            }
            count = n;
        }

        internal static string ToS(this ref BitSet bitSet) => ToS(bitSet.data);

        internal static string ToS(this Span<ulong> data) {
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
