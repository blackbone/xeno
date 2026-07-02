using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public partial class World {
        // this part of file is about creating entities
        private const uint AllocatedMask = 0b10000000_00000000_00000000_00000000U;
        private const uint NonAllocationMask = ~AllocatedMask;

        private int entityCount;
        protected internal Entity[] entities;
        private int freeIdsCount;
        internal int[] freeIds;

        public ReadOnlySpan<Entity> Entities {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entities;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsEntityValid_Internal(in Entity entity) {
            if (entity.WorldId != Id) return false;
            if (entity.Id >= entities.Length) return false;
            return entities[entity.Id].Version == entity.Version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitEntities() {
            entityCount = 0;
            entities = Array.Empty<Entity>();
            freeIdsCount = 0;
            freeIds = Array.Empty<int>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitEmptyEntities_Internal(int from, in int to) {
            var count = to - from + 1;
            var freeIdsLength = freeIds.Length;

            var size = freeIdsLength == 0 ? 1 : freeIdsLength;
            while (size < freeIdsCount + count) size <<= 1;
            Array.Resize(ref freeIds, size);

            // Start filling freeIds in reverse order
            for (var i = to; i >= from; i--) {
                entities[i] = new Entity(i, 0, Id);

                freeIds[freeIdsCount++] = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CreateEntity_Internal(out Entity entity) {
            var e_id = entityCount;
            if (freeIdsCount > 0) {
                freeIdsCount--;
                e_id = freeIds[freeIdsCount];
            }

            var stored = entities[e_id];
            entity = new Entity(stored.Id, stored.Version | AllocatedMask, stored.WorldId);
            entities[e_id] = entity;
            entityCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteEntity_Internal(in Entity entity) {
            DeleteEntity_Internal(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteEntity_Internal(in int entityId) {
            ref var e = ref entities[entityId];
            var id = e.Id;
            e = new Entity(id, (e.Version & NonAllocationMask) + 1, e.WorldId);
            entityCount--;

            if (freeIdsCount == freeIds.Length) Array.Resize(ref freeIds, freeIdsCount << 1);
            freeIds[freeIdsCount++] = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureFreeIdsCapacity_Internal(in int capacity) {
            if (capacity <= freeIds.Length) return;

            var size = freeIds.Length == 0 ? 1 : freeIds.Length;
            while (size < capacity) size <<= 1;
            Array.Resize(ref freeIds, size);
        }
    }
}
