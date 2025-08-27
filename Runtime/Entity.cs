using System.Runtime.InteropServices;

namespace Xeno
{
    /// <summary>
    /// Entity representation struct. 9 bytes only.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Entity
    {
        internal uint Id;
        internal uint Version;
        internal ushort WorldId;

        public override string ToString() => $"[{WorldId}|{Id}({Version})]{GetArchetype()}";

        private string GetArchetype() {
            if (!Worlds.TryGet(WorldId, out var world)) return "N/A";
            return world.entityArchetypes[Id].ToString();
        }
    }
}
