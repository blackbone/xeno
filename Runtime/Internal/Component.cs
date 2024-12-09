using Xeno;
// ReSharper disable StaticMemberInGenericType

namespace Xeno
{
    internal static class ComponentInfo
    {
        internal static int Index;
    }

    /// <summary>
    /// Component Info. Static class with all necessary variables.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class CI<T>
    {
        public static readonly T Default = default;
        public static readonly int Index = ComponentInfo.Index++;

        public static BitSetReadOnly Mask;
        static CI() {
            var maskSize = BitSet.MaskSize(Index);
            var set = new BitSet(stackalloc ulong[maskSize]) {
                indexJoin = Index
            };
            set.Set(Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2> {
        public static BitSetReadOnly Mask;

        static CI() {
            var indexJoin = CI<T1>.Index | CI<T2>.Index;
            var maskSize = BitSet.MaskSize(indexJoin);
            var set = new BitSet(stackalloc ulong[maskSize]) {
                indexJoin = indexJoin
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2, T3> {
        public static BitSetReadOnly Mask;

        static CI() {
            var indexJoin = CI<T1>.Index | CI<T2>.Index | CI<T3>.Index;
            var maskSize = BitSet.MaskSize(indexJoin);
            var set = new BitSet(stackalloc ulong[maskSize]) {
                indexJoin = indexJoin
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).Set(CI<T3>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2, T3, T4> {
        public static BitSetReadOnly Mask;
        static CI() {
            var indexJoin = CI<T1>.Index | CI<T2>.Index | CI<T3>.Index | CI<T4>.Index;
            var maskSize = BitSet.MaskSize(indexJoin);
            var set = new BitSet(stackalloc ulong[maskSize]) {
                indexJoin = indexJoin
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).Set(CI<T3>.Index).Set(CI<T4>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }
}
