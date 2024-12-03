// See https://aka.ms/new-console-template for more information

using ECS.Impl;

static class Program {
    public static void Main(string[] args) {
        var world = new World("name", 412);
        var e1 = world.CreateEmpty();
        e1.Add("10", 10);
        var e2 = world.Create(10);
        var e3 = world.Create(10, "foo");

        int i = 0;
        ref var v_i = ref i;
        e1.Get(ref v_i);


        world.Delete(e1);
        world.Dispose();
    }
}
