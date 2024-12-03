using System;
using SourceGenerator.Sample;

namespace Xeno.SourceGenerator.Sample;

[System]
public sealed partial class TestSystem
{
    [SystemMethod(SystemMethodKind.Update)] private static void SystemMethod1(ref Position position) { }
    [SystemMethod(SystemMethodKind.Update)] private static void SystemMethod2(in Entity_Old entityOld, ref Position position) { }
    [SystemMethod(SystemMethodKind.Update)] private static void SystemMethod3(in float uniform, ref Position position) { }
    [SystemMethod(SystemMethodKind.Update)] private static void SystemMethod4(in Entity_Old entityOld, in float uniform, ref Position position) { }
}