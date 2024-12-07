using ECS.Feature1;
using ECS.Feature2;
using ECS.Impl;

static class Program {
    public static void Main(string[] args) {
        // design of desired api
        var world = new World("My World Name", 10, new Feature2SystemGroup());
        var e1 = world.CreateEmpty();
        var e2 = world.Create(default(Feature1Component1), default(Feature1Component2), default(Feature1Component3));
        e2.Add(default(Feature1Component1), default(Feature1Component2), default(Feature1Component3));
        e2.Has(default(Feature1Component1), default(Feature1Component2), default(Feature1Component3));
        e2.Remove(default(Feature1Component1), default(Feature1Component2), default(Feature1Component3));
    }
}
