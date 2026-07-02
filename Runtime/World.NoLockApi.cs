using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public abstract partial class World {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected Entity CreateEntity_NoLock() {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);

            if (zeroArchetype.entitiesCount == zeroArchetype.entities.Length)
                Array.Resize(ref zeroArchetype.entities, zeroArchetype.entities.Length << 1);
            zeroArchetype.entities[zeroArchetype.entitiesCount] = entity.Id;
            inArchetypeLocalIndices[entity.Id] = zeroArchetype.entitiesCount;
            zeroArchetype.entitiesCount++;
            entityArchetypes[entity.Id] = zeroArchetype;
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DestroyEntity_NoLock(in Entity entity) {
            if (!IsEntityValid_Internal(entity))
                return;

            DestroyValidEntity_Internal(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void DestroyEntities_NoLock(ReadOnlySpan<Entity> entities) {
            EnsureFreeIdsCapacity_Internal(freeIdsCount + entities.Length);
            for (var i = 0; i < entities.Length; i++) {
                var entity = entities[i];
                if (!IsEntityValid_Internal(entity))
                    continue;

                DestroyValidEntity_Internal(entity.Id);
            }
        }
    }
}
