using System;
using System.Diagnostics;
using ECS.Impl;

static class Program {
    public static void Main(string[] args) {
        const int entities = 1000;
        const int iterations = 1_000;
        // design of desired api
        // var world = new World("My World Name", 10);
        // Console.Write("creating entities...");
        // for (int x = 0; x < entities; x++)
        //     world.CreateEmpty();
        // Console.WriteLine("done");
        // world.Startup();
        // var i = 0;
        // var sw = Stopwatch.StartNew();
        // while (i < iterations) {
        //     world.PreUpdate(0f);
        //     world.Update(0f);
        //     world.PostUpdate(0f);
        //     i++;
        //     if (i % 1000 == 0) Console.WriteLine($"done: {i}\tin\t{sw.Elapsed}");
        // }
        // sw.Stop();
        // Console.WriteLine($"done in\t{sw.Elapsed}\t{sw.Elapsed / iterations} per iteration");
        // world.Shutdown();
    }
}
