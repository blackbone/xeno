namespace Xeno.Collections {
    public static class UIntArrayPool {
        public static uint[] Rent(in int capacity) => new uint[capacity];
        public static void Release(in uint[] array) { }
    }
}