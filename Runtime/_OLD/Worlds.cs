using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Xeno
{
    public static class Worlds
    {
        private static readonly Dictionary<string, ushort> _worldNameToId = new(1);
        private static World_Old[] _existingWorlds = Array.Empty<World_Old>();
        private static ushort _worldsCounter;

        private static ushort WorldIdAllocator => _worldsCounter++;

        internal static void Add(in World_Old worldOld)
        {
            if (_existingWorlds.Length <= worldOld.Id) Array.Resize(ref _existingWorlds, worldOld.Id + 1);
            _existingWorlds[worldOld.Id] = worldOld;
            _worldNameToId.Add(worldOld.Name, worldOld.Id);
        }

        internal static void Remove(in World_Old worldOld)
        {
            _existingWorlds[worldOld.Id] = null;
            _worldNameToId.Remove(worldOld.Name);
        }

        public static World_Old Create(string name)
        {
            if (_worldNameToId.ContainsKey(name)) throw new InvalidOperationException($"World with name {name} already exists");
            return new World_Old(name, WorldIdAllocator);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(in ushort worldId, out World_Old worldOld)
        {
            if (_existingWorlds.Length > worldId && _existingWorlds[worldId] != null)
            {
                worldOld = _existingWorlds[worldId];
                return true;
            }

            worldOld = default;
            return false;
        }

        public static bool TryGet(in string worldName, out World_Old worldOld)
        {
            if (_worldNameToId.TryGetValue(worldName, out var worldId) && TryGet(worldId, out var internalWorld))
            {
                worldOld = internalWorld;
                return true;
            }

            worldOld = default;
            return false;
        }

        public static World_Old GetOrCreate(in string name)
        {
            if (!TryGet(name, out var world))
                world = new World_Old(name, WorldIdAllocator);
            return world;
        }
    }
}
