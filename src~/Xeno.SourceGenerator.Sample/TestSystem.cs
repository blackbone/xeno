using SourceGenerator.Sample;

namespace Xeno.SourceGenerator.Sample {
    [System]
    public sealed partial class TestSystem
    {
        private int entityCountToSpawn = 10;

        [SystemMethod(SystemMethodType.Startup)] private static void Startup1() { }
        [SystemMethod(SystemMethodType.Startup)] private static void Startup2([Uniform(nameof(entityCountToSpawn))] in int uniform) { }
        [SystemMethod(SystemMethodType.Startup)] private static void Startup3([Uniform(false)] in int uniform) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys1(ref Position position) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys2(in Entity entity, ref Position position) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys3([Uniform(true)] in float uniform, ref Position position) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys4(in Entity entity, [Uniform(true)] in float delta, ref Position position) { }
    }

    [System]
    public sealed partial class TestGenericSystem<T> where T : struct, IComponent
    {
        private int entityCountToSpawn = 10;

        [SystemMethod(SystemMethodType.Startup)] private static void Startup1() { }
        [SystemMethod(SystemMethodType.Startup)] private static void Startup2([Uniform(nameof(entityCountToSpawn))] in int uniform) { }
        [SystemMethod(SystemMethodType.Startup)] private static void Startup3([Uniform(false)] in int uniform) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys1(ref T c1) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys2(in Entity entity, ref T c1) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys3([Uniform(true)] in float uniform, ref T c1) { }
        [SystemMethod(SystemMethodType.Update)] private static void Sys4(in Entity entity, [Uniform(true)] in float delta, ref T c1) { }
    }
}
