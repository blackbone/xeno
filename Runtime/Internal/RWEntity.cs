using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Xeno {
    /// <summary>
    /// This struct is mirror to <see cref="Entity"/> with RW access.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerTypeProxy(typeof(World.Entity_Debug))]
    internal partial struct RWEntity
    {
        internal uint Id;
        internal uint Version;
        internal ushort WorldId;

        public override string ToString() => $"[{Id}({Version})]RW";
    }
}
