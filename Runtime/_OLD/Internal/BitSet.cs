using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    internal ref struct BitSet {
        internal int indexJoin;
        internal ulong hash;
        internal Span<ulong> data;

        public BitSet(in Span<ulong> data) {
            this.data = data;
            this.hash = 0;
            indexJoin = 0;
        }

        public override string ToString() => $"{this.ToS()}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MaskSize(int indexJoin) {
            if (indexJoin > Constants.MaxArchetypeComponents) throw new IndexOutOfRangeException();
            indexJoin -= 1;
            indexJoin |= indexJoin >> 1;
            indexJoin |= indexJoin >> 2;
            indexJoin |= indexJoin >> 4;
            indexJoin |= indexJoin >> 8;
            indexJoin |= indexJoin >> 16;
            return indexJoin / Constants.LongBitSize + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Zero(ref BitSet set) {
            set.data = Span<ulong>.Empty;
            set.hash = 0;
            set.indexJoin = 0;
        }
    }

    internal static class BitSetExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref BitSet Set(this ref BitSet set, int index) {
            set.data[index >> Constants.LONG_DIVIDER] |= 1ul << (index & Constants.LONG_DIVISION_MASK);
            return ref set;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromAdd(this ref BitSet set, in BitSetReadOnly origin, in BitSetReadOnly add) {
            if (origin.data.Length == 0) {
                add.data.CopyTo(set.data);
                set.hash = add.hash;
                set.indexJoin = add.indexJoin;
                return;
            }

            switch (add.data.Length) {
                case 0:
                    set.hash = 0;
                    break;
                case 1:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.hash = set.data[0];
                    break;
                case 2:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.hash = set.data[0] ^ set.data[1];
                    break;
                case 3:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2];
                    break;
                case 4:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.data[3] = origin.data[3] | add.data[3];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3];
                    break;
                case 5:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.data[3] = origin.data[3] | add.data[3];
                    set.data[4] = origin.data[4] | add.data[4];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4];
                    break;
                case 6:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.data[3] = origin.data[3] | add.data[3];
                    set.data[4] = origin.data[4] | add.data[4];
                    set.data[5] = origin.data[5] | add.data[5];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5];
                    break;
                case 7:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.data[3] = origin.data[3] | add.data[3];
                    set.data[4] = origin.data[4] | add.data[4];
                    set.data[5] = origin.data[5] | add.data[5];
                    set.data[6] = origin.data[6] | add.data[6];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6];
                    break;
                case 8:
                    set.data[0] = origin.data[0] | add.data[0];
                    set.data[1] = origin.data[1] | add.data[1];
                    set.data[2] = origin.data[2] | add.data[2];
                    set.data[3] = origin.data[3] | add.data[3];
                    set.data[4] = origin.data[4] | add.data[4];
                    set.data[5] = origin.data[5] | add.data[5];
                    set.data[6] = origin.data[6] | add.data[6];
                    set.data[7] = origin.data[7] | add.data[7];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6] ^ set.data[7];
                    break;
                default:
                    for (var i = 0; i < add.data.Length; i++) {
                        set.data[i] = origin.data[i] | add.data[i];
                    }
                    break;
            }

            set.indexJoin = origin.indexJoin | add.indexJoin;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromRemove(this ref BitSet set, in BitSetReadOnly origin, in BitSetReadOnly remove) {
            if (origin.data.Length == 0) {
                BitSet.Zero(ref set);
                return;
            }

            switch (remove.data.Length) {
                case 0:
                    set.hash = 0;
                    break;
                case 1:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.hash = set.data[0];
                    break;
                case 2:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.hash = set.data[0] ^ set.data[1];
                    break;
                case 3:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2];
                    break;
                case 4:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.data[3] = origin.data[3] & ~remove.data[3];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3];
                    break;
                case 5:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.data[3] = origin.data[3] & ~remove.data[3];
                    set.data[4] = origin.data[4] & ~remove.data[4];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3] ^ set.data[4];
                    break;
                case 6:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.data[3] = origin.data[3] & ~remove.data[3];
                    set.data[4] = origin.data[4] & ~remove.data[4];
                    set.data[5] = origin.data[5] & ~remove.data[5];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5];
                    break;
                case 7:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.data[3] = origin.data[3] & ~remove.data[3];
                    set.data[4] = origin.data[4] & ~remove.data[4];
                    set.data[5] = origin.data[5] & ~remove.data[5];
                    set.data[6] = origin.data[6] & ~remove.data[6];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6];
                    break;
                case 8:
                    set.data[0] = origin.data[0] & ~remove.data[0];
                    set.data[1] = origin.data[1] & ~remove.data[1];
                    set.data[2] = origin.data[2] & ~remove.data[2];
                    set.data[3] = origin.data[3] & ~remove.data[3];
                    set.data[4] = origin.data[4] & ~remove.data[4];
                    set.data[5] = origin.data[5] & ~remove.data[5];
                    set.data[6] = origin.data[6] & ~remove.data[6];
                    set.data[7] = origin.data[7] & ~remove.data[7];
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6] ^ set.data[7];
                    break;
                default:
                    set.hash = 0;
                    for (var i = 0; i < remove.data.Length; i++) {
                        set.data[i] = origin.data[i] & ~remove.data[i];
                        set.hash ^= set.data[i];
                    }
                    break;
            }

            set.FinalizeIndexJoin();
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
            set.indexJoin = result;
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
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5];
                    break;
                case 7:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6];
                    break;
                case 8:
                    set.hash = set.data[0] ^ set.data[1] ^ set.data[2] ^ set.data[3]
                        ^ set.data[4] ^ set.data[5] ^ set.data[6] ^ set.data[7];
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

        internal static uint[] GetIndices(this in BitSet set) {
            Span<uint> values = stackalloc uint[Constants.MaxArchetypeComponentsMaskSize];
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
            return values[..n].ToArray();
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
