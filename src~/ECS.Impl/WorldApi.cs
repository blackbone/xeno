using ECS.Feature2;
using Xeno;

namespace ECS.Impl
{
    public partial class World {
        string IWorld.Name => Name;

        public static WorldHandle Create(in string name, in Feature2SystemGroup sys_asldkaslkd) {
            var world = new World(name, 0, sys_asldkaslkd);
            return Worlds.Register(name, world);
        }

        public WorldHandle GetHandle() {
            return new WorldHandle();
        }
    }
}
