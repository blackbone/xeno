using System;
using SourceGenerator.Sample;

namespace Xeno.SourceGenerator.Sample;

// [System]
public sealed partial class TestSystem
{
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod1(ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod2(in Entity entity, ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod2(in float uniform, ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod2(in Entity entity, in float uniform, ref Position position) { }

    private static void Foo()
    {
        var d = new Memory<int>[]
        {
            new int[1], 
            new int[1],
            new int[1], 
            new int[1]
        };
        
        ReadOnlySpan<Memory<int>> v = d;
        ref readonly var v1 = ref v[0];
        ref var v2 = ref v1.Span[0];
        v2 = 10;
    }
}