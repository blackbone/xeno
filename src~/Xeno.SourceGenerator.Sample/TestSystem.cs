using System;
using SourceGenerator.Sample;

namespace Xeno.SourceGenerator.Sample;

[System]
public sealed partial class TestSystem
{
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod1(ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod2(in Entity entity, ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod3(in float uniform, ref Position position) { }
    [SystemMethod(SystemMethodType.Update)] private static void SystemMethod4(in Entity entity, in float uniform, ref Position position) { }
}