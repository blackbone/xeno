using System;
using System.Collections.Generic;

namespace Xeno
{
    public static partial class Worlds
    {
        internal static World[] ExistingWorlds = Array.Empty<World>();
        private static readonly Dictionary<string, byte> WorldNameToId = new(1);

        private static byte _worldsCounter;
        
        private static byte WorldIdAllocator => _worldsCounter++;

        internal static void Add(in World world)
        {
            if (ExistingWorlds.Length <= world.Id)
                Array.Resize(ref ExistingWorlds, world.Id + 1);
            ExistingWorlds[world.Id] = world;
            WorldNameToId.Add(world.Name, world.Id);
        }

        internal static void Remove(in World world)
        {
            ExistingWorlds[world.Id] = null;
            WorldNameToId.Remove(world.Name);
        }
    }
}