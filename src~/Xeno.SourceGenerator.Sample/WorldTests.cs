using System;
using SourceGenerator.Sample;

namespace Xeno.SourceGenerator.Sample
{
    public class WorldTests
    {
        public void Run()
        {
            const int time = 10000;
            const int entityCount = 1000000;
            const int entityPadding = 10;

            for (var i = 0; i < entityPadding; i++)
            {
                var world = Worlds.GetOrCreate("sample world");
                // world.AddSystem(new TestSystem());

                for (var j = 0; j < entityCount; ++j)
                {
                    for (var k = 0; k < entityPadding; ++k)
                        world.CreateEntity();

                    world.CreateEntity(new Position());
                }

                world.Start();

                var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() < start + time)
                    world.Tick(0f);
            
                Console.WriteLine(world.Ticks);
            
                world.Stop();
                world.Dispose();
            }
            

        }
    }
}

