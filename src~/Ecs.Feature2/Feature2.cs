// using System.Runtime.InteropServices;
// using Ecs.Feature1;
// using Xeno;
//
// namespace Ecs.Feature2 {
//     [Guid("65E04C21-A2CB-4B8A-A38D-CE5DE907F0FC")]
//     public struct Feature2Component {
//         public float Value;
//     }
//
//     public class Feature2SystemGroup {
//         private World world;
//
//         public int EntityCount;
//
//         [SystemMethod(SystemType.Startup, 2)]
//         public void Startup([Uniform(nameof(EntityCount))] ref int entityCount) {
//         }
//
//         [SystemMethod(SystemType.Shutdown, 2)]
//         public void Shutdown([Uniform(nameof(EntityCount))] ref int entityCount) {
//         }
//
//         [SystemMethod(SystemType.Update, 2)]
//         public void System(ref Feature2Component f2c, ref Feature1Component f1c) {
//             f2c.Value += f1c.Value;
//         }
//     }
// }
