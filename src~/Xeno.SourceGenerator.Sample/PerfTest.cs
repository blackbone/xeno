using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Xeno.SourceGenerator.Sample;

public class PerfTest
{
    public void Run()
    {
        const int count = 1_000;
        const int iterations = 1_000_000;

        Run0(count, iterations);
        Run1(count, iterations);
        Run2(count, iterations);
        Run3(count, iterations);
        Run4(count, iterations);
        Run5(count, iterations);
    }

    private static void Job(ref Comp1 c1, ref Comp2 c2, ref Comp3 c3, ref Comp4 c4, ref Comp5 c5)
    {
        c1.value++;
        c2.value++;
        c3.value++;
        c4.value++;
        c5.value++;
    }

    private void Run5(in int count, in int iterations)
    {
        var atData = new At5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref atData[j].comp1,
                ref atData[j].comp2,
                ref atData[j].comp3,
                ref atData[j].comp4,
                ref atData[j].comp5);
        Console.WriteLine($"{nameof(Run5)}: {sw.Elapsed}");
    }

    private void Run4(in int count, in int iterations)
    {
        var atData = new At4[count];
        var c5Data = new Comp5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref atData[j].comp1,
                ref atData[j].comp2,
                ref atData[j].comp3,
                ref atData[j].comp4,
                ref c5Data[j]);

        Console.WriteLine($"{nameof(Run4)}: {sw.Elapsed}");
    }

    private void Run3(in int count, in int iterations)
    {
        var atData = new At3[count];
        var c4Data = new Comp4[count];
        var c5Data = new Comp5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref atData[j].comp1,
                ref atData[j].comp2,
                ref atData[j].comp3,
                ref c4Data[j],
                ref c5Data[j]);
        Console.WriteLine($"{nameof(Run3)}: {sw.Elapsed}");
    }

    private void Run2(in int count, in int iterations)
    {
        var atData = new At2[count];
        var c3Data = new Comp3[count];
        var c4Data = new Comp4[count];
        var c5Data = new Comp5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref atData[j].comp1,
                ref atData[j].comp2,
                ref c3Data[j],
                ref c4Data[j],
                ref c5Data[j]);
        Console.WriteLine($"{nameof(Run2)}: {sw.Elapsed}");
    }

    private void Run1(in int count, in int iterations)
    {
        var atData = new At1[count];
        var c2Data = new Comp2[count];
        var c3Data = new Comp3[count];
        var c4Data = new Comp4[count];
        var c5Data = new Comp5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref atData[j].comp1,
                ref c2Data[j],
                ref c3Data[j],
                ref c4Data[j],
                ref c5Data[j]);
        Console.WriteLine($"{nameof(Run1)}: {sw.Elapsed}");
    }

    private void Run0(in int count, in int iterations)
    {
        var c1Data = new Comp1[count];
        var c2Data = new Comp2[count];
        var c3Data = new Comp3[count];
        var c4Data = new Comp4[count];
        var c5Data = new Comp5[count];

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        for (var j = 0; j < count; j++)
            Job(
                ref c1Data[j],
                ref c2Data[j],
                ref c3Data[j],
                ref c4Data[j],
                ref c5Data[j]);
        Console.WriteLine($"{nameof(Run0)}: {sw.Elapsed}");
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Comp1
    {
        public int value;
        public int value2;
        public int value3;
        public fixed int value4[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Comp2
    {
        public int value;
        public int value2;
        public int value3;
        public fixed int value4[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Comp3
    {
        public int value;
        public int value2;
        public int value3;
        public fixed int value4[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Comp4
    {
        public int value;
        public int value2;
        public int value3;
        public fixed int value4[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct Comp5
    {
        public int value;
        public int value2;
        public int value3;
        public fixed int value4[128];
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct At1
    {
        public Comp1 comp1;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct At2
    {
        public Comp1 comp1;
        public Comp2 comp2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct At3
    {
        public Comp1 comp1;
        public Comp2 comp2;
        public Comp3 comp3;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct At4
    {
        public Comp1 comp1;
        public Comp2 comp2;
        public Comp3 comp3;
        public Comp4 comp4;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct At5
    {
        public Comp1 comp1;
        public Comp2 comp2;
        public Comp3 comp3;
        public Comp4 comp4;
        public Comp5 comp5;
    }
}