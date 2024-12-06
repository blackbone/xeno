using System;
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
        public static T Default = default;
        public static readonly int Index = ComponentInfo.Index++;

        public static BitSetReadOnly Mask;
        static CI() {
            var set = new BitSet(stackalloc ulong[BitSet.MaskSize(Index)]) {
                max = Index
            };
            set.Set(Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2> {
        internal static readonly int Max = Math.Max(CI<T1>.Index, CI<T2>.Index);
        public static BitSetReadOnly Mask;

        static CI() {
            var set = new BitSet(stackalloc ulong[BitSet.MaskSize(Max)]) {
                max = Max
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2, T3> {
        private static readonly int Max = Math.Max(CI<T1, T2>.Max, CI<T3>.Index);
        public static BitSetReadOnly Mask;

        static CI() {
            var set = new BitSet(stackalloc ulong[BitSet.MaskSize(Max)]) {
                max = Max
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).Set(CI<T3>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }

    internal static class CI<T1, T2, T3, T4> {
        private static readonly int Max = Math.Max(CI<T1, T2>.Max, CI<T3, T4>.Max);
        public static BitSetReadOnly Mask;
        static CI() {
            var set = new BitSet(stackalloc ulong[BitSet.MaskSize(Max)]) {
                max = Max
            };
            set.Set(CI<T1>.Index).Set(CI<T2>.Index).Set(CI<T3>.Index).Set(CI<T4>.Index).FinalizeHash();
            Mask = set.AsReadOnly();
        }
    }
}
