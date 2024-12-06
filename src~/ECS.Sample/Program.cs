using ECS.Feature2;
using Xeno;

static class Program {
    public static void Main(string[] args) {
        // design of desired api
        var world = ECS.Impl.World.Create("My World Name", new Feature2SystemGroup());
        var entity = world.Create();


        //
        // var world = Worlds.Create("12333");
        // var e1 = world.CreateEmpty();
        // e1.Add("10", 10);
        // var e2 = world.Create(10);
        // var e3 = world.Create(10, "foo");
        //
        // int i = 0;
        // ref var v_i = ref i;
        // e1.Get(ref v_i);
        //
        //
        // world.Delete(e1);
        // world.Dispose();
    }
}
