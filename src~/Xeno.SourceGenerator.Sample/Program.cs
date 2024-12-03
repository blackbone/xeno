
using System.Runtime.InteropServices;
using ECS.Impl;

namespace SourceGenerator.Sample;

[Guid("C060D394-C1A0-45F1-8531-0823C72C9978")]
public struct Position
{
    public float x;
    public float y;
    public float z;
}

[Guid("859EBA51-B55C-41B0-86CD-F26E09542DA0")]
public struct Velocity
{
    public float x;
    public float y;
    public float z;
}

public struct Rotation
{
    public float x;
    public float y;
    public float z;
}

public struct AngularVelocity
{
    public float x;
    public float y;
    public float z;
}

public class Program
{
    public static void Main(string[] args) {
        var world = new World("", 412);

        var e1 = world.Create();
        var e2 = world.Create();

        world.Dispose();
    }
}
