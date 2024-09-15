using System.Runtime.InteropServices;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Entity
    {
        internal readonly byte WorldId;
        internal readonly uint Id;
        internal readonly uint Version;

        internal Entity(uint id, uint version, byte worldId)
        {
            Id = id;
            Version = version;
            WorldId = worldId;
        }

        public override string ToString() => $"[{Id}({Version})]";
    }
}