using Xeno;

namespace ECS.Feature1 {
    public static class Feature1StaticSystemGroup {
        [SystemMethod(SystemMethodKind.Startup, 1)] public static void Startup() { }
        [SystemMethod(SystemMethodKind.Update, 2)] public static void System2(ref Feature1Component1 c1) { }
        [SystemMethod(SystemMethodKind.Update, 3)] public static void System3(in EntityHandle e, ref Feature1Component1 c1) { }
        [SystemMethod(SystemMethodKind.Shutdown, 1)] public static void Shutdown() { }
    }
}
