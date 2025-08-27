using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    public sealed partial class World {
        public readonly string Name;
        public readonly ushort Id;

        /// <summary>
        /// Buffer of matching entities from LAST MATCH CALL
        /// </summary>
        public uint[] buffer;

        public uint EntityCount {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entityCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Worlds.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity() {
            lock (this) {
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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1>(in T1 component)
            where T1 : struct, IComponent {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component);
            AddToArchetype_Internal<T1>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2>(in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component1, component2);
            AddToArchetype_Internal<T1, T2>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component1, component2, component3);
            AddToArchetype_Internal<T1, T2, T3>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3, T4>(in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            if (entityCount == entities.Length)
                GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component1, component2, component3, component4);
            AddToArchetype_Internal<T1, T2, T3, T4>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(in Entity entity) {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;

                RemoveEntityFromArchetype_Internal(entity.Id);
                RemoveComponents_Internal(entity.Id, entityArchetypes[entity.Id].mask);
                DeleteEntity_Internal(entity);
                entityArchetypes[entity.Id] = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntityValid(in Entity entity) => IsEntityValid_Internal(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T1>(in Entity entity)
            where T1 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Includes(ref CI<T1>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Includes(ref CI<T1, T2>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Includes(ref CI<T1, T2, T3>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Includes(ref CI<T1, T2, T3, T4>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Cross(ref CI<T1, T2>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Cross(ref CI<T1, T2, T3>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            return entityArchetypes[entity.Id].mask.Cross(ref CI<T1, T2, T3, T4>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1>(in Entity entity, in T1 component1)
            where T1 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                AddComponents_Internal(entity.Id, component1);
                ChangeArchetypeAdd_Internal<T1>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2>(in Entity entity, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                AddComponents_Internal(entity.Id, component1, component2);
                ChangeArchetypeAdd_Internal<T1, T2>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3>(in Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                AddComponents_Internal(entity.Id, component1, component2, component3);
                ChangeArchetypeAdd_Internal<T1, T2, T3>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3, T4>(in Entity entity, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                AddComponents_Internal(entity.Id, component1, component2, component3, component4);
                ChangeArchetypeAdd_Internal<T1, T2, T3, T4>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T>(in Entity entity)
            where T : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                RemoveComponents_Internal<T>(entity.Id);
                ChangeArchetypeRemove_Internal<T>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponents<T>(in Entity entity, ref T component)
            where T : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return false;
                var result = RemoveComponents_Internal(entity.Id, ref component);
                ChangeArchetypeRemove_Internal<T>(entity.Id);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                RemoveComponents_Internal<T1, T2>(entity.Id);
                ChangeArchetypeRemove_Internal<T1, T2>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) RemoveComponents<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return default;
                var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2);
                ChangeArchetypeRemove_Internal<T1, T2>(entity.Id);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                RemoveComponents_Internal<T1, T2, T3>(entity.Id);
                ChangeArchetypeRemove_Internal<T1, T2, T3>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool) RemoveComponents<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return default;
                var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2, ref component3);
                ChangeArchetypeRemove_Internal<T1, T2, T3>(entity.Id);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return;
                RemoveComponents_Internal<T1, T2, T3, T4>(entity.Id);
                ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(entity.Id);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool, bool) RemoveComponents<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            lock (this) {
                if (!IsEntityValid_Internal(entity))
                    return default;
                var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2, ref component3, ref component4);
                ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(entity.Id);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T1 Ref<T1>(in Entity entity)
            where T1 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                throw new InvalidOperationException();
            return ref RefComponent_Internal<T1>(entity.Id);
        }

        // old get component api

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponent<T1>(in Entity entity, ref T1 component1)
            where T1 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return false;
            return RefComponents_Internal(entity.Id, ref component1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return false;
            return RefComponents_Internal(entity.Id, ref component1)
                && RefComponents_Internal(entity.Id, ref component2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) RefComponentsAny<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return false;
            return RefComponents_Internal(entity.Id, ref component1)
                && RefComponents_Internal(entity.Id, ref component2)
                && RefComponents_Internal(entity.Id, ref component3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool) RefComponentsAny<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2),
                RefComponents_Internal(entity.Id, ref component3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return false;
            return RefComponents_Internal(entity.Id, ref component1)
                && RefComponents_Internal(entity.Id, ref component2)
                && RefComponents_Internal(entity.Id, ref component3)
                && RefComponents_Internal(entity.Id, ref component4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool, bool) RefComponentsAny<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            if (!IsEntityValid_Internal(entity))
                return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2),
                RefComponents_Internal(entity.Id, ref component3),
                RefComponents_Internal(entity.Id, ref component4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1>()
            where T1 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            while (While(CI<T1>.Mask, ref current, ref entityIds, ref count))
                result += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count))
                result += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2, T3>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count))
                result += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2, T3, T4>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count))
                result += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Store3<T1> GetStore<T1>() where T1 : struct, IComponent {
            if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, stores2.Length << 1);
            ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
            s ??= new Store3<T1>();
            return s;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Match<T1>() where T1 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            var buf_current = 0;
            while (While(CI<T1>.Mask, ref current, ref entityIds, ref count)) {
                Array.Copy(entityIds, 0, buffer,  buf_current, count);
                buf_current = result += count;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Match<T1, T2>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            var buf_current = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count)) {
                Array.Copy(entityIds, 0, buffer,  buf_current, count);
                buf_current = result += count;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Match<T1, T2, T3>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            var buf_current = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count)) {
                Array.Copy(entityIds, 0, buffer,  buf_current, count);
                buf_current = result += count;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Match<T1, T2, T3, T4>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = null;
            var count = 0;
            var buf_current = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count)) {
                Array.Copy(entityIds, 0, buffer,  buf_current, count);
                buf_current = result += count;
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSystem(in System system) => defaultSystemGroup.AddSystem(system);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSystem(in System system) => defaultSystemGroup.RemoveSystem(system);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddSystemGroup(in SystemGroup systemGroup) => systemGroups.AddLast(systemGroup);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveSystemGroup(in SystemGroup system) => systemGroups.Remove(system);

        public override string ToString() => $"{Name} ({Id})";

        private ulong _ticks;
        public ulong Ticks => _ticks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(float f) {
            PreUpdate?.Invoke(f);
            Update?.Invoke(f);
            PostUpdate?.Invoke(f);
            _ticks++;
        }

        public void Start() {
            defaultSystemGroup.AttachToWorld(this);

            if (systemGroups.Count > 0) {
                var current = systemGroups.First;
                do {
                    current?.Value.AttachToWorld(this);
                    current = current?.Next;
                } while (current != null);
            }

            _ticks = 0ul;
            Started?.Invoke();
        }

        public void Stop() {
            Stopped?.Invoke();

            if (systemGroups.Count > 0) {
                var current = systemGroups.Last;
                do {
                    current?.Value.DetachFromWorld(this);
                    current = current?.Previous;
                } while (current != null);
            }

            defaultSystemGroup.DetachFromWorld(this);
        }

        public void EnsureCapacity(int capacity) {
            // no op
        }
    }
}
