// using System.Runtime.CompilerServices;
// using System.Runtime.InteropServices;
// using Xeno.Vendor;
//
// namespace Xeno {
// #pragma warning disable CS8500
//     [StructLayout(LayoutKind.Sequential)]
//     public abstract class Store2 {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         internal abstract void Remove_Internal(in uint entityId);
//     }
//
//     /// <summary>
//     /// Paged component store. Data lives in unmanaged 4KB pages from PageAllocator.
//     /// Dense index maps to (pageIndex, slotIndex) pair in arrays for O(1) addressing.
//     /// </summary>
//     [StructLayout(LayoutKind.Sequential)]
//     public class Store2<T> : Store2 where T : struct, IComponent {
//         public unsafe T*[] pages = new T*[4];
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public unsafe ref T Ref(in uint entityId) {
//             var pid = (int)(entityId / CI<T>.PageCapacity);
//             var slot = (int)(entityId / CI<T>.PageCapacity);
//             if (pid >= pages.Length) {
//                 var newArr = new T*[BitOperations.Smear(pid) + 1];
//                 for (int i = 0; i < pages.Length; i++) newArr[i] = pages[i];
//                 pages = newArr;
//             }
//             ref var page = ref pages[pid];
//             if (page == null) page = PageAllocator.GetPage<T>();
//             return ref Unsafe.AsRef<T>(page + slot);
//         }
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public unsafe void Reset(in uint entityId) {
//             var pid = entityId / CI<T>.PageCapacity;
//             if (pid >= pages.Length) return;
//
//             var page = pages[pid];
//             if (page == null) return;
//
//             *(page + entityId % CI<T>.PageCapacity) = default;
//         }
//
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         internal override unsafe void Remove_Internal(in uint entityId) {
//             var pid = entityId / CI<T>.PageCapacity;
//             if (pid >= pages.Length) return;
//
//             var page = pages[pid];
//             if (page == null) return;
//
//             *(page + entityId % CI<T>.PageCapacity) = default;
//         }
//     }
// }
