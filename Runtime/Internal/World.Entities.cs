using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public partial class World {
        // this part of file is about creating entities
        private const uint AllocatedMask = 0b10000000_00000000_00000000_00000000U;
        private const uint NonAllocationMask = ~AllocatedMask;

        private uint entityCount;
        public Entity[] entities;
        private uint freeIdsCount;
        internal uint[] freeIds;

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
            freeIds = Array.Empty<uint>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitEmptyEntities_Internal(uint from, in uint to) {
            var count = to - from + 1;
            var freeIdsLength = freeIds.Length;

            var size = freeIdsLength == 0 ? 1 : freeIdsLength;
            while (size < freeIdsCount + count) size <<= 1;
            Array.Resize(ref freeIds, size);

            // Start filling freeIds in reverse order
            for (var i = to; i != uint.MaxValue && i >= from; i--) {
                ref var entity = ref entities[i];
                entity.Id = i;
                entity.WorldId = Id;
                entity.Version = 0;

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

            entities[e_id].Version |= AllocatedMask;
            entity = Unsafe.As<Entity, Entity>(ref entities[e_id]);
            entityCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteEntity_Internal(in Entity entity) {
            DeleteEntity_Internal(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteEntity_Internal(in uint entityId) {
            ref var e = ref entities[entityId];
            e.Version &= NonAllocationMask;
            e.Version++;
            entityCount--;

            if (freeIdsCount == freeIds.Length) Array.Resize(ref freeIds, (int)(freeIdsCount << 1));
            freeIds[freeIdsCount++] = e.Id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureFreeIdsCapacity_Internal(in uint capacity) {
            if (capacity <= freeIds.Length) return;

            var size = freeIds.Length == 0 ? 1 : freeIds.Length;
            while (size < capacity) size <<= 1;
            Array.Resize(ref freeIds, size);
        }
    }
}
