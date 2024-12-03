namespace ECS.Impl {
    public static class _1 {
        public static void _11() {
            var world = new World("_", 1);
            var e = world.CreateEmpty();
            e.Add(10);
            e.Add(10, "asdasd", 3232);
        }
    }
}
