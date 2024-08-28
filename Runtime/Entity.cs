using System.Runtime.InteropServices;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct RWEntity {
        internal byte WorldId;
        public uint Id;
        public uint Version;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Entity
    {
        private static Entity defaultEntity = new(uint.MaxValue, 0, 0);
        public static ref Entity Default => ref defaultEntity;

        internal readonly byte WorldId;
        public readonly uint Id;
        public readonly uint Version;

        internal Entity(uint id, uint version, byte worldId)
        {
            Id = id;
            Version = version;
            WorldId = worldId;
        }

        public override string ToString()
        {
            return $"[{Id}({Version})]";
        }
    }
}