namespace Xeno {
    /// <summary>
    /// Marker attribute used for world types identification.
    /// </summary>
    public interface IWorld { }
}

/// <summary>
/// This class contains API call stubs used to generate non generic api.
/// </summary>
public static class WorldExtensions {
    public static Xeno.IEntity Create(this Xeno.IWorld world, params object[] components)
        => throw new System.NotImplementedException();

    public static void Add(this Xeno.IWorld world, in Xeno.IEntity entity, params object[] components)
        => throw new System.NotImplementedException();

    public static void Remove(this Xeno.IWorld world, in Xeno.IEntity entity, params object[] components)
        => throw new System.NotImplementedException();
}
