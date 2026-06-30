using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    [Serializable]
    public abstract class Store3 {
        public const int Shift = 6;
        public const int Cap = Mask + 1;
        public const int Mask = (1 << Shift) - 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void Remove_Internal(in uint entityId);
    }

    [Serializable]
    public class Store3<T> : Store3 {
        public T[][] pages;

        public Store3() : this(32) {
        }

        public Store3(int pageCount) {
            pages = new T[pageCount <= 0 ? 1 : pageCount][];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void Remove_Internal(in uint entityId) {
            var pid = entityId >> Shift;
            if (pid >= pages.Length) return;

            var page = pages[pid];
            if (page == null) return;

            page[entityId & Mask] = default;
        }
    }
}
