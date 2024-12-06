using System.Runtime.InteropServices;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct EntityHandle
    {
        public readonly uint EntityId;
        public readonly uint Version;
        public readonly WorldHandle World;
    }
}


public static class EntityExtensions {
    public static void Delete(this Xeno.EntityHandle entity) => throw new System.NotImplementedException();
    public static void Add(this Xeno.EntityHandle entity, params object[] components) => throw new System.NotImplementedException();
    public static void Remove(this Xeno.EntityHandle entity, params object[] components) => throw new System.NotImplementedException();

    public static void Get<T1>(this Xeno.EntityHandle entity, ref T1 c1) => throw new System.NotImplementedException();
    public static void Get<T1, T2>(this Xeno.EntityHandle entity, ref T1 c1, ref T2 c2) => throw new System.NotImplementedException();
    public static void Get<T1, T2, T3>(this Xeno.EntityHandle entity, ref T1 c1, ref T2 c2, ref T3 c3) => throw new System.NotImplementedException();
}
