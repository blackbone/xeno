using System.Runtime.InteropServices;
using ECS.Feature1;
using ECS.Feature2;
using ECS.Impl;
using Xeno;

namespace ECS.Feature3 {
    [Guid("58B614BE-C7C7-4CA0-851C-24A2DA6E3ADA")]
    public struct Feature3Component {
        public float Value;
    }

    public class Feature3SystemGroup {
        public WorldHandle World;

        public int EntityCount;

        [SystemMethod(SystemMethodKind.Startup, 3)]
        public void Startup() {
            var e = World.CreateEntity();
        }

        [SystemMethod(SystemMethodKind.Shutdown, 3)]
        public void Shutdown() {
            var e = World.CreateEntity();
        }

        [SystemMethod(SystemMethodKind.Update, 3)]
        public void System(ref Feature3Component f3c, ref Feature2Component f2c, ref Feature1Component f1c) {
            f3c.Value = f2c.Value + f1c.Value;
        }
    }
}
