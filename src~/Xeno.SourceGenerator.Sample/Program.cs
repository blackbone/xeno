using System.Runtime.InteropServices;
using Xeno.SourceGenerator.Sample;
using IComponent = Xeno.IComponent;

namespace SourceGenerator.Sample;

[Guid("C060D394-C1A0-45F1-8531-0823C72C9978")]
public struct Position : IComponent
{
    public float x;
    public float y;
    public float z;
}

[Guid("859EBA51-B55C-41B0-86CD-F26E09542DA0")]
public struct Velocity : IComponent
{
    public float x;
    public float y;
    public float z;
}

public struct Rotation : IComponent
{
    public float x;
    public float y;
    public float z;
}

public struct AngularVelocity : IComponent
{
    public float x;
    public float y;
    public float z;
}

public class Program
{
    public static void Main(string[] args)
    {
            // Console.WriteLine(typeof(Filter.IncludeDisabledAttribute).FullName);
            // Console.WriteLine(typeof(Filter.ChangedOnlyAttribute).FullName);
            // // Console.WriteLine(Marshal.SizeOf<Chunk>());
            // Console.WriteLine(Marshal.SizeOf<int>());

            // var chunk = new Chunk(Component<Position>.Info);

            // Test<byte>();
            // Test<short>();
            // Test<int>();
            // Test<long>();
            // Test<float>();
            // Test<Position>();

            new WorldTests().Run();
            new PerfTest().Run();

            // new Benchmark1(100_000, 20).Run(1_000).Dispose();
            // new Benchmark2(100_000, 20).Run(1_000).Dispose();
            // new Benchmark3(100_000, 20).Run(1_000).Dispose();
    }
}
