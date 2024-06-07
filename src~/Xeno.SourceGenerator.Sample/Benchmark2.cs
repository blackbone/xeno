// using System;
// using System.Diagnostics;
//
// namespace Xeno.SourceGenerator.Sample;
//
// [System]
// public partial class XenoSystem2
// {
//     [SystemMethod(SystemMethodType.Update)]
//     public static void Update(
//         ref XenoBaseContext.Component1 component1,
//         ref XenoBaseContext.Component2 component2)
//     {
//         component1.Value += component2.Value;
//     }
// }
//
// public class Benchmark2 : XenoBaseContext
// {
//     private Entity? someEntity = null;
//     public Benchmark2(int entityCount, int entityPadding)
//     {
//         World.AddSystem(new XenoSystem2());
//
//         for (int i = 0; i < entityCount; ++i)
//         {
//             for (int j = 0; j < entityPadding; ++j)
//             {
//                 var padding = World.CreateEntity();
//                 switch (j % 2)
//                 {
//                     case 0:
//                         padding.AddComponent(new Component1());
//                         break;
//
//                     case 1:
//                         padding.AddComponent(new Component2());
//                         break;
//                 }
//             }
//
//             someEntity = World.CreateEntity(
//                 new Component1(),
//                 new Component2 { Value = 1 });
//         }
//                 
//         World.Start();
//     }
//     
//     public override void Dispose()
//     {
//         Console.WriteLine($"{GetType().Name} -> World: ticks {World.Ticks}");
//         Console.WriteLine($"{GetType().Name} -> World: entities {World.EntityCount}");
//         Console.WriteLine($"{GetType().Name} -> Entity: {someEntity!.Value.GetComponent<Component1>().Value}");
//         World.Stop();
//         base.Dispose();
//     }
//
//     public Benchmark2 Run(int iterations)
//     {
//         var sw = Stopwatch.StartNew();
//         while (iterations-- > 0)
//             World.Tick(0f);
//         
//         Console.WriteLine($"{GetType().Name}: {sw.Elapsed}");
//         return this;
//     }
// }

