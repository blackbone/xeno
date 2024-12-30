// using System.Runtime.InteropServices;
// using Xeno;
//
// namespace Ecs.Feature1 {
//     [Guid("c4a78d54-8471-45ff-9f17-52d04656db04")]
//     public struct Feature1Component {
//         public float Value;
//     }
//
//     public class Feature1SystemGroup {
//         private World world;
//
//         private const string SharedIntFieldName = "someName";
//
//         [SystemMethod(SystemType.Startup, 1)]
//         public void Startup([Uniform(SharedIntFieldName)] in int someField) {
//         }
//
//         [SystemMethod(SystemType.Shutdown, 1)]
//         public void Shutdown([Uniform(SharedIntFieldName)] in int someField) {
//         }
//
//         [SystemMethod(SystemType.Update, 1)]
//         internal void System(in Entity entity, ref Feature1Component f1c) {
//             world.Create(11f);
//             world.Create("ads");
//             world.Create((byte)10);
//         }
//     }
// }
