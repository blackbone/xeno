namespace Xeno {
    /// <summary>
    /// Marker attribute used for entity types identification.
    /// </summary>
    public interface IEntity { }
}

public static class EntityExtensions {
    public static void Add(this Xeno.IEntity entity, params object[] components)
        => throw new System.NotImplementedException();

    public static void Remove(this Xeno.IEntity entity, params object[] components)
        => throw new System.NotImplementedException();

    public static void Get<T1>(this Xeno.IEntity entity, ref T1 c1) => throw new System.NotImplementedException();
    public static void Get<T1, T2>(this Xeno.IEntity entity, ref T1 c1, ref T2 c2) => throw new System.NotImplementedException();
    public static void Get<T1, T2, T3>(this Xeno.IEntity entity, ref T1 c1, ref T2 c2, ref T3 c3) => throw new System.NotImplementedException();
}
