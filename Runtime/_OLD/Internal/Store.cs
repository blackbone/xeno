using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NotImplementedException = System.NotImplementedException;

namespace Xeno {
#pragma warning disable CS8500
    [StructLayout(LayoutKind.Sequential)]
    internal abstract class Store {
        public uint[] sparse;
        public uint[] dense;
        public uint count;

        protected Store(in uint capacity) {
            sparse = new uint[capacity];
            dense = new uint[Constants.InitialComponentsCapacity];
            count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal abstract void SwapData_Internal(in uint d1, in uint last);
        internal abstract object Get_Debug(uint entityId);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class Store<T> : Store where T : struct, IComponent {
        public T[] data;

        public Store(in uint capacity) : base(capacity)
            => data = new T[Constants.InitialComponentsCapacity];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void SwapData_Internal(in uint d1, in uint last)
            => data[d1] = data[last];

        internal override object Get_Debug(uint entityId) => data[sparse[entityId]];
    }
}