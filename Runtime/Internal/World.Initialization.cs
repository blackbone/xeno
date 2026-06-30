using System;
using System.Runtime.CompilerServices;
using Xeno;

namespace Xeno {
    public partial class World {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected World(string name) : this(name, Worlds.AllocateWorldId(name)) {
        }

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
            var oldLength = (uint)entities.Length;
            if (capacity <= oldLength) return;

            // entities
            var arr = new Entity[capacity];
            Array.Copy(entities, arr, entities.Length);
            entities = arr;

            InitEmptyEntities_Internal(oldLength, capacity - 1);

            // archetypes
            var at = new Archetype[capacity];
            Array.Copy(entityArchetypes, at, entityArchetypes.Length);
            entityArchetypes = at;

            var atiid = new uint[capacity];
            Array.Copy(inArchetypeLocalIndices, atiid, inArchetypeLocalIndices.Length);
            inArchetypeLocalIndices = atiid;

            // components
            storeCapacity = capacity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool While(in BitSetReadOnly mask, ref Archetype current, ref uint[] data, ref int count)
        {
            var localMask = mask;
            while (current != null && !current.mask.Includes(ref localMask))
                current = current.next;

            if (current == null)
                return false;

            data = current.entities;
            count = (int)current.entitiesCount;
            current = current.next;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MatchChunks(in BitSetReadOnly mask, ref uint[][] chunks, ref int[] counts) {
            if (chunks == null || chunks.Length == 0) chunks = new uint[4][];
            if (counts == null || counts.Length == 0) counts = new int[chunks.Length];

            var chunkCount = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            while (While(in mask, ref current, ref entityIds, ref count)) {
                if (chunkCount == chunks.Length) {
                    Array.Resize(ref chunks, chunks.Length << 1);
                    Array.Resize(ref counts, counts.Length << 1);
                }

                chunks[chunkCount] = entityIds;
                counts[chunkCount] = count;
                chunkCount++;
            }

            return chunkCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int MatchGeneratedChunks(in BitSetReadOnly mask, ref uint[][] chunks, ref int[] counts) {
            return MatchChunks(in mask, ref chunks, ref counts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool HasGeneratedMask(in Entity entity, in BitSetReadOnly mask) {
            if (!IsEntityValid_Internal(entity))
                return false;

            var localMask = mask;
            return entityArchetypes[entity.Id].mask.Includes(ref localMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool GeneratedMaskIncludes(in BitSetReadOnly set, in BitSetReadOnly mask) {
            var copy = set;
            var localMask = mask;
            return copy.Includes(ref localMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateEntityWithMask_NoLock(in BitSetReadOnly mask) {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            archetypes.Add(mask, entity.Id, out entityArchetypes[entity.Id], out inArchetypeLocalIndices[entity.Id]);
            return entity;
        }

        protected Entity CreateEntityWithMask(in BitSetReadOnly mask) {
            AssertOwnerThread();
            return CreateEntityWithMask_NoLock(in mask);
        }

        protected static BitSetReadOnly CreateGeneratedMask(params int[] indices) {
            var max = 0;
            for (var i = 0; i < indices.Length; i++)
                if (max < indices[i]) max = indices[i];

            var set = new BitSet(stackalloc ulong[BitSet.MaskSize(max)]) {
                max = max
            };

            for (var i = 0; i < indices.Length; i++)
                set.Set(indices[i]);

            set.FinalizeHash();
            return set.AsReadOnly();
        }
    }
}
