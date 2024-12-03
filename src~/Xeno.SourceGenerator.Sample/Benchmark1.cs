// using System;
// using System.Diagnostics;
//
// namespace Xeno.SourceGenerator.Sample;
//
// [System]
// public partial class XenoSystem1
// {
//     
//     [SystemMethod(SystemMethodType.Update)]
//     private static void Update(ref XenoBaseinfo.Context.Component1 component1) => component1.Value++;
// }
//
// public class Benchmark1 : XenoBaseContext
// {
//     private Entity? someEntity = null;
//     public Benchmark1(int entityCount, int entityPadding)
//     {
//         World.AddSystem(new XenoSystem1());
//                 
//         for (int i = 0; i < entityCount; ++i)
//         {
//             for (int j = 0; j < entityPadding; ++j)
//             {
//                 World.CreateEntity();
//             }
//
//             if (someEntity == null) someEntity = World.CreateEntity(new Component1());
//             else World.CreateEntity(new Component1());
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
//     public Benchmark1 Run(int iterations)
//     {
//         var sw = Stopwatch.StartNew();
//         while (iterations-- > 0)
//             World.Tick(0f);
//         
//         Console.WriteLine($"{GetType().Name}: {sw.Elapsed}");
//         return this;
//     }
// }
