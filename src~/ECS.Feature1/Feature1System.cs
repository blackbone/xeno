using Xeno;

namespace ECS.Feature1 {
    public class Feature1SystemGroup {
        [SystemMethod(SystemMethodKind.Startup, 1)] public void Startup() { }
        [SystemMethod(SystemMethodKind.Update, 2)] public void System2(ref Feature1Component1 c1) { }

        [SystemMethod(SystemMethodKind.Update, 3)]
        public void System3(in EntityHandle e) {
            e.Add(new Feature1Component1());
            e.Remove(new Feature1Component2());
        }
        [SystemMethod(SystemMethodKind.Shutdown, 1)] public void Shutdown() { }
    }
}
