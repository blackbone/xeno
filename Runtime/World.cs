using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xeno.Collections;

namespace Xeno
{
    public sealed partial class World
    {
        public string Name { get; }

        public byte Id { get; }

        public uint EntityCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entities.count;
        }

        public void Dispose() => Worlds.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity() => entities.Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T>(in T component)
            where T : struct, IComponent
        {
            var entity = entities.Create();
            Components<T>().Add(entity.Id, component);
            AddToArchetype<T>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2>(in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            AddToArchetype<T1, T2>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            AddToArchetype<T1, T2, T3>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3, T4>(in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            Components<T4>().Add(entity.Id, component4);
            AddToArchetype<T1, T2, T3, T4>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteEntity(in Entity entity)
        {
            if (entities.currentCapacity <= entity.Id) return;
            if (entities.versions.data[entity.Id] != entity.Version) return;
            entities.Remove(entity.Id);
            for (var i = 0; i < Component.Index; i++)
                componentStores.AtRO(i)?.RemoveInternal(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponent<T>(in Entity entity, in T component)
            where T : struct, IComponent
        {
            Components<T>().Add(entity.Id, component);
            RemoveFromArchetype<T>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2>(in Entity entity, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            RemoveFromArchetype<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3>(in Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            RemoveFromArchetype<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3, T4>(in Entity entity, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            Components<T4>().Add(entity.Id, component4);
            RemoveFromArchetype<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(in Entity entity)
            where T : struct, IComponent
        {
            componentStores.data[Component<T>.Index]?.RemoveInternal(entity.Id);
            RemoveFromArchetype<T>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>(in Entity entity, ref T component)
            where T : struct, IComponent
        {
            var result = Components<T>().Remove(entity.Id, ref component);
            RemoveFromArchetype<T>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            componentStores.data[Component<T1>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T2>.Index]?.RemoveInternal(entity.Id);
            RemoveFromArchetype<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) RemoveComponents<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            (bool, bool) result;
            result.Item1 = Components<T1>().Remove(entity.Id, ref component1);
            result.Item2 = Components<T2>().Remove(entity.Id, ref component2);
            RemoveFromArchetype<T1, T2>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            componentStores.data[Component<T1>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T2>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T3>.Index]?.RemoveInternal(entity.Id);
            RemoveFromArchetype<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool) RemoveComponents<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            (bool, bool, bool) result;
            result.Item1 = Components<T1>().Remove(entity.Id, ref component1);
            result.Item2 = Components<T2>().Remove(entity.Id, ref component2);
            result.Item3 = Components<T3>().Remove(entity.Id, ref component3);
            RemoveFromArchetype<T1, T2, T3>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            componentStores.data[Component<T1>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T2>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T3>.Index]?.RemoveInternal(entity.Id);
            componentStores.data[Component<T4>.Index]?.RemoveInternal(entity.Id);
            RemoveFromArchetype<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool, bool) RemoveComponents<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            (bool, bool, bool, bool) result;
            result.Item1 = Components<T1>().Remove(entity.Id, ref component1);
            result.Item2 = Components<T2>().Remove(entity.Id, ref component2);
            result.Item3 = Components<T3>().Remove(entity.Id, ref component3);
            result.Item4 = Components<T4>().Remove(entity.Id, ref component4);
            RemoveFromArchetype<T1, T2, T3, T4>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T>(ComponentDelegate<T> update)
            where T : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++) {
                var eid = cs1.GetEntity(i);
                update(ref cs1.components.data[cs1.mapping.sparse.data[eid]]);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T>(EntityComponentDelegate<T> update)
            where T : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var eid = cs1.GetEntity(i);
                update(entities.At(eid), ref cs1.components.data[cs1.mapping.sparse.data[eid]]);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<TU, T>(UniformComponentDelegate<TU, T> update, in TU uniform)
            where T : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var eid = cs1.GetEntity(i);
                update(uniform, ref cs1.components.data[cs1.mapping.sparse.data[eid]]);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<TU, T>(EntityUniformComponentDelegate<TU, T> update, in TU uniform)
            where T : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var eid = cs1.GetEntity(i);
                update(entities.At(eid), uniform, ref cs1.components.data[cs1.mapping.sparse.data[eid]]);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2>(ComponentDelegate<T1, T2> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            FixedBitSet mask = default;
            mask.Set(Component<T1>.Index).Set(Component<T2>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();

            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]]
                        );
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2>(EntityComponentDelegate<T1, T2> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var mask = new FixedBitSet();
            mask.Set(Component<T1>.Index);
            mask.Set(Component<T2>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            
            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        new Entity(eid, entities.versions.data[eid], Id),
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3>(ComponentDelegate<T1, T2, T3> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var mask = new FixedBitSet();
            mask.Set(Component<T1>.Index);
            mask.Set(Component<T2>.Index);
            mask.Set(Component<T3>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();

            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]],
                        ref cs3.components.data[cs3.mapping.sparse.data[eid]]
                    );
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3>(EntityComponentDelegate<T1, T2, T3> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var mask = new FixedBitSet();
            mask.Set(Component<T1>.Index);
            mask.Set(Component<T2>.Index);
            mask.Set(Component<T3>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();

            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        new Entity(eid, entities.versions.data[eid], Id),
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]],
                        ref cs3.components.data[cs3.mapping.sparse.data[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3, T4>(ComponentDelegate<T1, T2, T3, T4> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var mask = new FixedBitSet();
            mask.Set(Component<T1>.Index);
            mask.Set(Component<T2>.Index);
            mask.Set(Component<T3>.Index);
            mask.Set(Component<T4>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();
            var cs4 = componentStores.AtRO(Component<T4>.Index).As<T4>();

            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]],
                        ref cs3.components.data[cs3.mapping.sparse.data[eid]],
                        ref cs4.components.data[cs4.mapping.sparse.data[eid]]
                    );
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3, T4>(EntityComponentDelegate<T1, T2, T3, T4> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var mask = new FixedBitSet();
            mask.Set(Component<T1>.Index);
            mask.Set(Component<T2>.Index);
            mask.Set(Component<T3>.Index);
            mask.Set(Component<T4>.Index);

            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();
            var cs4 = componentStores.AtRO(Component<T4>.Index).As<T4>();

            var current = entities.archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        new Entity(eid, entities.versions.data[eid], Id),
                        ref cs1.components.data[cs1.mapping.sparse.data[eid]],
                        ref cs2.components.data[cs2.mapping.sparse.data[eid]],
                        ref cs3.components.data[cs3.mapping.sparse.data[eid]],
                        ref cs4.components.data[cs4.mapping.sparse.data[eid]]
                    );
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1>()
            where T1 : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            return cs1.Count();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var count = cs1.Count();
            var result = 0;
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                if (!cs2.Has(entityId)) continue;

                result++;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2, T3>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();
            var count = cs1.Count();
            var result = 0;
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                if (!cs2.Has(entityId)) continue;
                if (!cs3.Has(entityId)) continue;

                result++;
            }

            return result;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2, T3, T4>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();
            var cs4 = componentStores.AtRO(Component<T4>.Index).As<T4>();
            var count = cs1.Count();
            var result = 0;
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                if (!cs2.Has(entityId)) continue;
                if (!cs3.Has(entityId)) continue;
                if (!cs4.Has(entityId)) continue;

                result++;
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
        public void Tick(float f)
        {
            PreUpdate?.Invoke(f);
            Update?.Invoke(f);
            PostUpdate?.Invoke(f);
            _ticks++;
        }

        public void Start()
        {
            defaultSystemGroup.AttachToWorld(this);
            
            if (systemGroups.Count > 0)
            {
                var current = systemGroups.First;
                do
                {
                    current.Value.AttachToWorld(this);
                    current = current.Next;
                } while (current != null);
            }

            _ticks = 0ul;
            Started?.Invoke();
        }

        public void Stop()
        {
            Stopped?.Invoke();
            
            if (systemGroups.Count > 0)
            {
                var current = systemGroups.Last;
                do
                {
                    current.Value.DetachFromWorld(this);
                    current = current.Previous;
                } while (current != null);
            }
            
            defaultSystemGroup.DetachFromWorld(this);
        }

        public void EnsureCapacity(int capacity) => entities.Ensure(capacity);

        public void EnsureCapacity<T>(int capacity) where T : struct, IComponent
            => (componentStores.At(Component<T>.Index) ??= new ComponentStore<T>()).As<T>().Ensure(capacity);
    }
}