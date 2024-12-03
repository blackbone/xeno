using System.Runtime.InteropServices;

namespace Xeno
{
    /// <summary>
    /// Entity representation struct. 9 bytes only.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Entity_Old
    {
        internal readonly uint Id;
        internal readonly uint Version;
        internal readonly ushort WorldId;

        public override string ToString() => $"[{WorldId}|{Id}({Version})]{GetArchetype()}";

        private string GetArchetype() {
            if (!Worlds.TryGet(WorldId, out var world)) return "N/A";
            return world.entityArchetypes[Id].ToString();
        }
    }
}
