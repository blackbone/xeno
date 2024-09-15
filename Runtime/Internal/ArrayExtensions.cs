using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#pragma warning disable CS8500

namespace Xeno {
    internal static class BoostExtensions {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this T[] array, in int index) {
            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.As<RDA>(array).Data), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T At<T>(this T[] array, in uint index) {
            return ref Unsafe.Add(ref Unsafe.As<byte, T>(ref Unsafe.As<RDA>(array).Data), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TOut AtCast<TIn, TOut>(this TIn[] array, in int index) {
            return ref Unsafe.Add(ref Unsafe.As<byte, TOut>(ref Unsafe.As<RDA>(array).Data), index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref TOut AtCast<TIn, TOut>(this TIn[] array, in uint index) {
            return ref Unsafe.Add(ref Unsafe.As<byte, TOut>(ref Unsafe.As<RDA>(array).Data), index);
        }
    }

    // Description taken from CoreCLR: see https://source.dot.net/#System.Private.CoreLib/src/System/Runtime/CompilerServices/RuntimeHelpers.CoreCLR.cs,285.
    // CLR arrays are laid out in memory as follows (multidimensional array bounds are optional):
    // [ sync block || pMethodTable || num components || MD array bounds || array data .. ]
    // ^ ^ ^ returned reference
    // | \-- ref Unsafe.As<RawArrayData>(array).Data
    // \-- array
    // The base size of an array includes all the fields before the array data,
    // including the sync block and method table. The reference to RawData.Data
    // points at the number of components, skipping over these two pointer-sized fields.
    [StructLayout(LayoutKind.Sequential)]
    // ReSharper disable once ClassNeverInstantiated.Local
    internal sealed class RDA {
        private readonly IntPtr _len;
        public byte Data;
    }
}
