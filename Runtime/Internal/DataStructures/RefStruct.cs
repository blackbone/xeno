using System.Runtime.CompilerServices;

namespace Xeno.DataStructures
{
    public readonly unsafe struct RefStruct<T>
        where T : unmanaged
    {
        private readonly T* item1;
        
        public ref T Item1 => ref Unsafe.AsRef<T>(item1);

        public RefStruct(ref T item1)
        {
            this.item1 = (T*)Unsafe.AsPointer(ref item1);
        }
    }
    
    public readonly unsafe struct RefStruct<T1, T2>
        where T1 : unmanaged
        where T2 : unmanaged
    {
        private readonly T1* item1;
        private readonly T2* item2;

        public ref T1 Item1 => ref Unsafe.AsRef<T1>(item1);
        public ref T2 Item2 => ref Unsafe.AsRef<T2>(item2);
        public RefStruct(ref T1 item1, ref T2 item2)
        {
            this.item1 = (T1*)Unsafe.AsPointer(ref item1);
            this.item2 = (T2*)Unsafe.AsPointer(ref item2);
        }
    }
    
    public readonly unsafe struct RefStruct<T1, T2, T3>
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
    {
        private readonly T1* item1;
        private readonly T2* item2;
        private readonly T3* item3;

        public ref T1 Item1 => ref Unsafe.AsRef<T1>(item1);
        public ref T2 Item2 => ref Unsafe.AsRef<T2>(item2);
        public ref T3 Item3 => ref Unsafe.AsRef<T3>(item3);
        
        public RefStruct(ref T1 item1, ref T2 item2, ref T3 item3)
        {
            this.item1 = (T1*)Unsafe.AsPointer(ref item1);
            this.item2 = (T2*)Unsafe.AsPointer(ref item2);
            this.item3 = (T3*)Unsafe.AsPointer(ref item3);
        }
    }
    
    public readonly unsafe struct RefStruct<T1, T2, T3, T4>
        where T1 : unmanaged
        where T2 : unmanaged
        where T3 : unmanaged
        where T4 : unmanaged
    {
        private readonly T1* item1;
        private readonly T2* item2;
        private readonly T3* item3;
        private readonly T4* item4;

        public ref T1 Item1 => ref Unsafe.AsRef<T1>(item1);
        public ref T2 Item2 => ref Unsafe.AsRef<T2>(item2);
        public ref T3 Item3 => ref Unsafe.AsRef<T3>(item3);
        public ref T4 Item4 => ref Unsafe.AsRef<T4>(item4);
        
        public RefStruct(ref T1 item1, ref T2 item2, ref T3 item3, ref T4 item4)
        {
            this.item1 = (T1*)Unsafe.AsPointer(ref item1);
            this.item2 = (T2*)Unsafe.AsPointer(ref item2);
            this.item3 = (T3*)Unsafe.AsPointer(ref item3);
            this.item4 = (T4*)Unsafe.AsPointer(ref item4);
        }
    }
}