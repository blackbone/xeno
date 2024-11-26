using System;
using System.Runtime.CompilerServices;
using Xeno;

namespace Xeno {
    public sealed partial class World {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal World(string name, ushort id)
        {
            Name = name;
            Id = id;

            InitComponents(Constants.DefaultComponentTypesCount, Constants.DefaultEntityCount);
            InitArchetypes(Constants.DefaultEntityCount);
            InitEntities();
            GrowCapacity_Internal(Constants.DefaultEntityCount);

            Worlds.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GrowCapacity_Internal(in uint capacity) {
            // entities
            var arr = new RWEntity[capacity];
            Array.Copy(entities, arr, entities.Length);
            entities = arr;

            InitEmptyEntities_Internal(entityCount, capacity - 1);

            // archetypes
            var at = new Archetype[capacity];
            Array.Copy(entityArchetypes, at, entityArchetypes.Length);
            entityArchetypes = at;

            var atiid = new uint[capacity];
            Array.Copy(inArchetypeLocalIndices, atiid, inArchetypeLocalIndices.Length);
            inArchetypeLocalIndices = atiid;

            // components
            storeCapacity = capacity;
            for (var i = 0; i < stores.Length; i++) {
                ref var s = ref stores[i];
                if (s == null) continue;
                var ss = new uint[capacity];
                Array.Copy(s.sparse, ss, s.sparse.Length);
                s.sparse = ss;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool While(BitSetReadOnly mask, ref Archetype current, ref uint[] data, ref int count)
        {
            while (current != null && !current.mask.Includes(ref mask))
                current = current.next;

            if (current == null)
                return false;

            data = current.entities;
            count = (int)current.entitiesCount;
            current = current.next;
            return true;
        }
    }
}
