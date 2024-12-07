using Xeno;

namespace ECS.Feature2 {
    public class Feature2SystemGroup {
        [SystemMethod(SystemMethodKind.Startup, 1)] public static void Startup() { }
        [SystemMethod(SystemMethodKind.Update, 2)] public void System2(ref Feature2Component c1) { }
        [SystemMethod(SystemMethodKind.Update, 3)] public void System3(in Entity_Old e, ref Feature2Component c1) { }
        [SystemMethod(SystemMethodKind.Shutdown, 1)] public void Shutdown() { }
    }
}
