using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno {
    /// <summary>
    /// 4 MB slab allocator that hands out 4 KB pages. Uses manual aligned unmanaged memory (no GC pinning required).
    /// </summary>
    public static unsafe class PageAllocator {
        internal const int PAGE_SIZE = 4096; // 4 KB fixed-size page
        private const int PAGES_PER_SLAB = 1024;                        // 4MB / 4KB
        private const int SLAB_SIZE      = PAGE_SIZE * PAGES_PER_SLAB;  // 4 MB
        private const int ALIGN          = PAGE_SIZE;                   // 4096 alignment

        private static readonly Stack<IntPtr> _freePages = new();
        private static readonly List<IntPtr> _slabsAligned = new(); // aligned bases (for debugging)

        static PageAllocator() { AllocateSlab(); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IntPtr AllocAligned(int sizeBytes, int alignment)
        {
            var total = checked(sizeBytes + alignment - 1 + IntPtr.Size);
            var raw = Marshal.AllocHGlobal(total);
            var rawAddr = raw.ToInt64();
            var aligned = (rawAddr + IntPtr.Size + (alignment - 1)) & ~((long)alignment - 1);
            Marshal.WriteIntPtr(new IntPtr(aligned - IntPtr.Size), raw);
            return new IntPtr(aligned);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AllocateSlab() {
            var aligned = AllocAligned(SLAB_SIZE, ALIGN);
            _slabsAligned.Add(aligned);

            var p = (byte*)aligned.ToPointer();
            for (var i = 0; i < PAGES_PER_SLAB; i++) {
                _freePages.Push(new IntPtr(p + i * PAGE_SIZE));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* GetPage<T>() where T : struct, IComponent {
            if (_freePages.Count == 0) AllocateSlab();
            var ptr = _freePages.Pop();
            return (T*)ptr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FreePage<T>(T* page) where T : struct, IComponent {
            _freePages.Push((IntPtr)page);
        }

        public static void Clear() {
            for (var i = 0; i < _slabsAligned.Count; i++)
                if (_slabsAligned[i] != IntPtr.Zero) {
                    var raw = Marshal.ReadIntPtr(_slabsAligned[i] - IntPtr.Size);
                    Marshal.FreeHGlobal(raw);
                }

            _slabsAligned.Clear();
            _freePages.Clear();
        }
    }
}
