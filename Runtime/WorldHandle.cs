using System.Runtime.InteropServices;
using Xeno;

namespace Xeno {
    public interface IEntity { }

    public interface IWorld {
        public string Name { get; }

        WorldHandle GetHandle();
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WorldHandle
    {
        public readonly ushort Id;
    }
}

/// <summary>
/// This class contains API call stubs used to generate non generic api.
/// </summary>
public static class WorldExtensions
{
    public static EntityHandle Create(this WorldHandle world, params object[] components)
        => throw new System.NotImplementedException();

    public static void Add(this WorldHandle world, in EntityHandle entity, params object[] components)
        => throw new System.NotImplementedException();

    public static void Remove(this WorldHandle world, in EntityHandle entity, params object[] components)
        => throw new System.NotImplementedException();
}
