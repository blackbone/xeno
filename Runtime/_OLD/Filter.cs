using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    internal ref struct Set {
        internal uint indexJoin;
        internal ulong hash;
        internal Span<ulong> data;

        public Set(in Span<ulong> data, in uint join) {
            this.data = data;
            indexJoin = join;
            hash = 0;
            this.FinalizeHash();
        }
    }

    internal readonly struct SetReadOnly {
        internal readonly uint indexJoin;
        internal readonly ulong hash;
        internal readonly ulong[] data;

        public SetReadOnly(ref Set set) {
            indexJoin = set.indexJoin;
            hash = set.hash;
            data = set.data.ToArray();
        }
    }

    internal ref struct Filter {
        public Set with;
        public Set without;

        public Filter(in Span<ulong> withData, uint withJoin, in Span<ulong> withoutData, uint withoutJoin) {
            with = new Set(withData, withJoin);
            without = new Set(withoutData, withoutJoin);
        }

        public FilterReadOnly AsReadOnly() => new(ref this);
    }

    internal readonly struct FilterReadOnly {
        public readonly SetReadOnly with;
        public readonly SetReadOnly without;

        public FilterReadOnly(ref Filter filter) {
            with = new SetReadOnly(ref filter.with);
            without = new SetReadOnly(ref filter.without);
        }
    }

    static class Usage {
        static void Foo() {
            // add 1, 10, 100
            // exclude 20, 80

            var filter = new Filter(
                stackalloc ulong[] { 1ul << 1, 1ul << 2, 1ul << 3 },
                1 | 2 | 3,
                stackalloc ulong[] { 1ul << 4, 1ul << 5, 1ul << 6 },
                4 | 5 | 6
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void FinalizeHash(this ref Set set) {
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
                    for (var i = 0; i < set.data.Length; i++)
                        set.hash ^= set.data[i];
                    break;
            }
        }
    }
}
