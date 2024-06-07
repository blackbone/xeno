using System;
using System.Diagnostics;
using Xeno.Collections;

namespace Xeno.SourceGenerator.Sample;

public class GrowOnlyListTest<T> where T : unmanaged
{
    private readonly GrowOnlyList<T> list = new(128);

    public TimeSpan Run()
    {
        const int iterations = 1024 * 1024 * 16;
        var sw = Stopwatch.StartNew();

        // create
        for (var i = 0; i < iterations; i++)
            list.Add(default);

        // access
        for (var i = 0; i < iterations; i++)
        {
            var v = list[(uint)i];
        }

        // pop
        for (var i = 0; i < iterations; i++)
            list.TakeLast();

        sw.Stop();
        Console.WriteLine($"{this} ended in {sw.Elapsed}");
        return sw.Elapsed;
    }
}