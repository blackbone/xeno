using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public sealed partial class World {
        // this part of file is about creating entities
        private const uint AllocatedMask = 0b10000000_00000000_00000000_00000000U;
        private const uint NonAllocationMask = ~AllocatedMask;

        private uint entityCount;
        internal RWEntity[] entities;
        private uint freeIdsCount;
        internal uint[] freeIds;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsEntityValid_Internal(in Entity entity) {
            if (entity.WorldId != Id) return false;
            return entities[entity.Id].Version == entity.Version;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitEntities() {
            entityCount = 0;
            entities = Array.Empty<RWEntity>();
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
            entity = Unsafe.As<RWEntity, Entity>(ref entities[e_id]);
            entityCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DeleteEntity_Internal(in Entity entity) {
            ref var e = ref entities[entity.Id];
            e.Version &= NonAllocationMask;
            e.Version++;
            entityCount--;

            if (freeIdsCount == freeIds.Length) Array.Resize(ref freeIds, (int)(freeIdsCount << 1));
            freeIds[freeIdsCount++] = e.Id;
        }
    }
}
