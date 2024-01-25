using System;

namespace Xeno
{
    public static partial class Worlds
    {
        public static World Create(string name)
        {
            if (WorldNameToId.ContainsKey(name))
                throw new InvalidOperationException($"World with name {name} already exists");

            return new World(name, WorldIdAllocator);
        }

        public static bool TryGet(in byte worldId, out World world)
        {
            if (ExistingWorlds.Length > worldId && ExistingWorlds[worldId] != null)
            {
                world = ExistingWorlds[worldId];
                return true;
            }

            world = default;
            return false;
        }

        public static bool TryGet(in string worldName, out World world)
        {
            if (WorldNameToId.TryGetValue(worldName, out var worldId) && TryGet(worldId, out var internalWorld))
            {
                world = internalWorld;
                return true;
            }

            world = default;
            return false;
        }

        public static World GetOrCreate(in string name)
        {
            if (!TryGet(name, out var world))
                world = new World(name, WorldIdAllocator);
            return world;
        }
    }
}