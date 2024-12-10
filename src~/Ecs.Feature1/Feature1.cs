using System.Runtime.InteropServices;
using Xeno;

namespace ECS.Feature1 {
    [Guid("c4a78d54-8471-45ff-9f17-52d04656db04")]
    public struct Feature1Component {
        public float Value;
    }

    public class Feature1SystemGroup {
        private const string SharedIntFieldName = "someName";

        [SystemMethod(SystemMethodKind.Startup, 1)]
        public void Startup([Uniform(SharedIntFieldName)] in int someField) {
        }

        [SystemMethod(SystemMethodKind.Shutdown, 1)]
        public void Shutdown([Uniform(SharedIntFieldName)] in int someField) {
        }

        [SystemMethod(SystemMethodKind.Update, 1)]
        public void System(ref Feature1Component f1c) {
            f1c.Value++;
        }
    }
}
