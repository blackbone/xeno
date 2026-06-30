using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Xeno
{
    /// <summary>
    /// Entity representation struct. 9 bytes only.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Entity
    {
        public uint Id;
        public uint Version;
        public ushort WorldId;

        public override string ToString() => $"[{WorldId}|{Id}({Version})]{GetArchetype()}";

        private string GetArchetype() {
            if (!Worlds.TryGet(WorldId, out var world)) return "N/A";
            return world.entityArchetypes[Id].ToString();
        }
    }
}
