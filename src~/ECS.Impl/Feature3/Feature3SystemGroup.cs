using ECS.Impl;
using Xeno;

namespace ECS.Feature3 {
    public struct Feature3Component {
        public float a;
        public float b;
    }

    public class Feature3SystemGroup {
        [SystemMethod(SystemMethodKind.Startup, 1)] public static void Startup() { }
        [SystemMethod(SystemMethodKind.Update, 2)] public void System2(ref Feature3Component c1) { }
        [SystemMethod(SystemMethodKind.Update, 3)] public void System3(in Entity e, ref Feature3Component c1) { }
        [SystemMethod(SystemMethodKind.Shutdown, 1)] public void Shutdown() { }
    }
}
