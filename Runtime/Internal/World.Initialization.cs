using System;
using System.Runtime.CompilerServices;
using Xeno;
using Xeno.Vendor;

namespace Xeno {
    public partial class World {
        private const int QueryCacheSize = 16;

        private struct QueryCacheEntry {
            public BitSetReadOnly Mask;
            public Archetype[] Archetypes;
            public int[][] Chunks;
            public int[] Counts;
            public int Count;
            public uint Version;
            public uint UsedAt;
        }

        private QueryCacheEntry[] queryCache;
        private uint queryCacheVersion;
        private uint queryCacheClock;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected World(string name) : this(name, Worlds.AllocateWorldId(name)) {
        }

        internal World(string name, ushort id)
        {
            Name = name;
            Id = id;

            InitArchetypes(Constants.DefaultEntityCount);
            InitQueryCache();
            InitEntities();
            GrowCapacity_Internal(Constants.DefaultEntityCount);

            Worlds.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GrowCapacity_Internal(in int capacity) {
            var oldLength = entities.Length;
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

            var atiid = new int[capacity];
            Array.Copy(inArchetypeLocalIndices, atiid, inArchetypeLocalIndices.Length);
            inArchetypeLocalIndices = atiid;

            GrowGeneratedCapacity_Internal(capacity);
        }

        protected virtual void GrowGeneratedCapacity_Internal(int capacity) {
        }

        private void InitQueryCache() {
            queryCache = new QueryCacheEntry[QueryCacheSize];
            queryCacheVersion = 1;
            queryCacheClock = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void InvalidateQueryCache_Internal() {
            queryCacheVersion++;
            if (queryCacheVersion == 0) {
                queryCacheVersion = 1;
                Array.Clear(queryCache, 0, queryCache.Length);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetCachedQuery(in BitSetReadOnly mask, out int index) {
            for (var i = 0; i < queryCache.Length; i++) {
                ref var entry = ref queryCache[i];
                if (entry.Archetypes == null || entry.Version != queryCacheVersion)
                    continue;
                if (!entry.Mask.Equals(mask))
                    continue;

                entry.UsedAt = ++queryCacheClock;
                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int StoreCachedQuery(in BitSetReadOnly mask, Archetype[] archetypes, int count, ref int[][] chunks, ref int[] counts) {
            var index = 0;
            var usedAt = uint.MaxValue;
            for (var i = 0; i < queryCache.Length; i++) {
                ref var entry = ref queryCache[i];
                if (entry.Archetypes == null || entry.Version != queryCacheVersion) {
                    index = i;
                    break;
                }

                if (entry.UsedAt < usedAt) {
                    usedAt = entry.UsedAt;
                    index = i;
                }
            }

            int[][] entryChunks = null;
            int[] entryCounts = null;
            EnsureChunkCapacity(ref entryChunks, ref entryCounts, count);
            var activeCount = 0;
            for (var i = 0; i < count; i++) {
                var archetype = archetypes[i];
                var entitiesCount = (int)archetype.entitiesCount;
                if (entitiesCount == 0)
                    continue;

                entryChunks[activeCount] = archetype.entities;
                entryCounts[activeCount] = entitiesCount;
                activeCount++;
            }

            queryCache[index] = new QueryCacheEntry {
                Mask = mask,
                Archetypes = archetypes,
                Chunks = entryChunks,
                Counts = entryCounts,
                Count = count,
                Version = queryCacheVersion,
                UsedAt = ++queryCacheClock
            };

            chunks = entryChunks;
            counts = entryCounts;
            return activeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureChunkCapacity(ref int[][] chunks, ref int[] counts, int capacity) {
            if (chunks == null || chunks.Length == 0)
                chunks = new int[capacity < 4 ? 4 : capacity][];
            if (counts == null || counts.Length == 0)
                counts = new int[chunks.Length];

            if (capacity <= chunks.Length && capacity <= counts.Length)
                return;

            var size = chunks.Length > counts.Length ? chunks.Length : counts.Length;
            while (size < capacity)
                size <<= 1;
            Array.Resize(ref chunks, size);
            Array.Resize(ref counts, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void EnsureArchetypeCapacity(ref Archetype[] archetypes, int capacity) {
            if (archetypes == null || archetypes.Length == 0)
                archetypes = new Archetype[capacity < 4 ? 4 : capacity];
            if (capacity <= archetypes.Length)
                return;

            var size = archetypes.Length;
            while (size < capacity)
                size <<= 1;
            Array.Resize(ref archetypes, size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FillChunks(Archetype[] archetypes, int archetypeCount, ref int[][] chunks, ref int[] counts) {
            EnsureChunkCapacity(ref chunks, ref counts, archetypeCount);
            var activeCount = 0;
            for (var i = 0; i < archetypeCount; i++) {
                var archetype = archetypes[i];
                var entitiesCount = (int)archetype.entitiesCount;
                if (entitiesCount == 0)
                    continue;

                chunks[activeCount] = archetype.entities;
                counts[activeCount] = entitiesCount;
                activeCount++;
            }

            return activeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FillCachedChunks(ref QueryCacheEntry entry, ref int[][] chunks, ref int[] counts) {
            chunks = entry.Chunks;
            counts = entry.Counts;
            var activeCount = 0;
            for (var i = 0; i < entry.Count; i++) {
                var archetype = entry.Archetypes[i];
                var entitiesCount = (int)archetype.entitiesCount;
                if (entitiesCount == 0)
                    continue;

                chunks[activeCount] = archetype.entities;
                counts[activeCount] = entitiesCount;
                activeCount++;
            }

            return activeCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int MatchChunks(in BitSetReadOnly mask, ref int[][] chunks, ref int[] counts) {
            if (TryGetCachedQuery(in mask, out var cachedIndex))
                return FillCachedChunks(ref queryCache[cachedIndex], ref chunks, ref counts);

            Archetype[] matchedArchetypes = null;
            var matchedCount = 0;
            var current = archetypes.head;
            var localMask = mask;
            while (current != null) {
                if (current.mask.Includes(ref localMask)) {
                    EnsureArchetypeCapacity(ref matchedArchetypes, matchedCount + 1);
                    matchedArchetypes[matchedCount++] = current;
                }
                current = current.next;
            }

            matchedArchetypes ??= Array.Empty<Archetype>();
            return StoreCachedQuery(in mask, matchedArchetypes, matchedCount, ref chunks, ref counts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected int MatchGeneratedChunks(in BitSetReadOnly mask, ref int[][] chunks, ref int[] counts) {
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
        protected BitSetReadOnly GetGeneratedEntityMask(in int entityId) {
            return entityArchetypes[entityId].mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool GeneratedMaskIncludes(in BitSetReadOnly set, in BitSetReadOnly mask) {
            var copy = set;
            var localMask = mask;
            return copy.Includes(ref localMask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int GeneratedTrailingZeroCount64(ulong value) {
            return BitOperations.TrailingZeroCount64(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateEntityWithMask_NoLock(in BitSetReadOnly mask) {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            archetypes.Add(mask, entity.Id, out entityArchetypes[entity.Id], out inArchetypeLocalIndices[entity.Id]);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateEntityWithMask_NoLock(in BitSetReadOnly mask, ref object archetypeCache) {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            archetypes.AddCached(mask, ref archetypeCache, entity.Id, out entityArchetypes[entity.Id], out inArchetypeLocalIndices[entity.Id]);
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
