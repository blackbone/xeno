using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xeno
{
    public static class Worlds
    {
        private static readonly Dictionary<string, ushort> _worldNameToId = new(1);
        private static World[] _existingWorlds = Array.Empty<World>();
        private static ushort _worldsCounter;

        private static ushort WorldIdAllocator => _worldsCounter++;

        internal static void Add(in World world)
        {
            if (_existingWorlds.Length <= world.Id) Array.Resize(ref _existingWorlds, world.Id + 1);
            _existingWorlds[world.Id] = world;
            _worldNameToId.Add(world.Name, world.Id);
        }

        internal static void Remove(in World world)
        {
            _existingWorlds[world.Id] = null;
            _worldNameToId.Remove(world.Name);
        }

        public static World Create(string name)
        {
            if (_worldNameToId.ContainsKey(name)) throw new InvalidOperationException($"World with name {name} already exists");
            return new World(name, WorldIdAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(in ushort worldId, out World world)
        {
            if (_existingWorlds.Length > worldId && _existingWorlds[worldId] != null)
            {
                world = _existingWorlds[worldId];
                return true;
            }

            world = default;
            return false;
        }

        public static bool TryGet(in string worldName, out World world)
        {
            if (_worldNameToId.TryGetValue(worldName, out var worldId) && TryGet(worldId, out var internalWorld))
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
