using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public abstract partial class World {
        public readonly string Name;
        public readonly ushort Id;

        public uint EntityCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entityCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Worlds.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity() {
            AssertOwnerThread();
            return CreateEntity_NoLock();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(in Entity entity) {
            AssertOwnerThread();
            DestroyEntity_NoLock(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntities(Entity[] entities) {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            DestroyEntities(entities.AsSpan());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntities(ReadOnlySpan<Entity> entities) {
            AssertOwnerThread();
            DestroyEntities_NoLock(entities);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DestroyValidEntity_Internal(in uint entityId) {
            var archetype = entityArchetypes[entityId];
            if (archetype.mask.indices.Length != 0)
                ClearGeneratedEntityData(entityId, archetype.mask);
            RemoveEntityFromArchetype_Internal(entityId);
            DeleteEntity_Internal(entityId);
            entityArchetypes[entityId] = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntityValid(in Entity entity) => IsEntityValid_Internal(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static uint EntityId(in Entity entity) => entity.Id;

        public override string ToString() => $"{Name} ({Id})";

        private ulong _ticks;
        public ulong Ticks => _ticks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Tick(float f) {
            _ticks++;
        }

        public virtual void Start() {
            ResetTicks();
        }

        public virtual void Stop() {
        }

        protected void ResetTicks() => _ticks = 0ul;

        protected void IncrementTicks() => _ticks++;

        public void EnsureCapacity(int capacity) {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            GrowCapacity_Internal((uint)capacity);
        }
    }
}
