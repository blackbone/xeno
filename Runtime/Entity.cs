using System.Runtime.InteropServices;

namespace Xeno
{
    /// <summary>
    /// Entity representation struct. 9 bytes only.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly partial struct Entity
    {
        public readonly int Id;
        public readonly uint Version;
        public readonly ushort WorldId;

        internal Entity(int id, uint version, ushort worldId) {
            Id = id;
            Version = version;
            WorldId = worldId;
        }

        public override string ToString() => $"[{WorldId}|{Id}({Version})]{GetArchetype()}";

        private string GetArchetype() {
            if (!Worlds.TryGet(WorldId, out var world)) return "N/A";
            return world.entityArchetypes[Id].ToString();
        }
    }
}
