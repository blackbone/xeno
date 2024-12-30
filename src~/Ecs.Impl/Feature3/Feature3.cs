using System.Runtime.InteropServices;
using Ecs.Impl;
using Xeno;

namespace Ecs.Feature3 {
    [Guid("58B614BE-C7C7-4CA0-851C-24A2DA6E3ADA")]
    public struct Feature3Component { public float Value; }

    public class Feature3SystemGroup {
        public World World;

        [SystemMethod(SystemType.Startup, 3)]
        public void System(ref int c_0, ref string c_1) {
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5, (uint)6, (long)7, (ulong)8, (float)9, (double)10);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5, (uint)6, (long)7, (ulong)8, (float)9);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5, (uint)6, (long)7, (ulong)8);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5, (uint)6, (long)7);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5, (uint)6);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4, (int)5);
            World.Create((byte)1, (sbyte)2, (short)3, (ushort)4);
            World.Create((byte)1, (sbyte)2, (short)3);
            World.Create(1, (sbyte)2);
            World.Create(1);
        }
    }
}
