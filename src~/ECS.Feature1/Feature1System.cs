using Xeno;

namespace ECS.Feature1 {
    public class Feature1SystemGroup {
        [SystemMethod(SystemMethodKind.Update, 1)] public void System1() { }
        [SystemMethod(SystemMethodKind.Update, 2)] public void System2(ref Feature1Component c1) { }
        [SystemMethod(SystemMethodKind.Update, 3)] public void System3(in Entity_Old e, ref Feature1Component c1) { }
    }
}
