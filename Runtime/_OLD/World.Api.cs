using System;
using System.Runtime.CompilerServices;
using Xeno;

namespace Xeno
{
    public sealed partial class World {
        public readonly string Name;
        public readonly ushort Id;

        public uint EntityCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => entityCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => Worlds.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity() {
            if (entityCount == entities.Length) GrowCapacity_Internal(entityCount << 1);

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
        public Entity CreateEntity<T1>(in T1 component)
            where T1 : struct, IComponent
        {
            if (entityCount == entities.Length) GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component);
            AddToArchetype_Internal<T1>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2>(in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (entityCount == entities.Length) GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component1, component2);
            AddToArchetype_Internal<T1, T2>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (entityCount == entities.Length) GrowCapacity_Internal(entityCount << 1);

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
            where T4 : struct, IComponent
        {
            if (entityCount == entities.Length) GrowCapacity_Internal(entityCount << 1);

            CreateEntity_Internal(out var entity);
            AddComponents_Internal(entity.Id, component1, component2, component3, component4);
            AddToArchetype_Internal<T1, T2, T3, T4>(entity.Id);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(in Entity entity)
        {
            if (!IsEntityValid_Internal(entity)) return;

            RemoveEntityFromArchetype_Internal(entity.Id);
            RemoveComponents_Internal(entity.Id, entityArchetypes[entity.Id].mask);
            DeleteEntity_Internal(entity);
            entityArchetypes[entity.Id] = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEntityValid(in Entity entity) => IsEntityValid_Internal(entity);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T1>(in Entity entity)
            where T1 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasComponent_Internal<T1>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAllComponents_Internal<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAllComponents_Internal<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAllComponents_Internal<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAnyComponents_Internal<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAnyComponents_Internal<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return IsEntityValid_Internal(entity) && HasAnyComponents_Internal<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1>(in Entity entity, in T1 component1)
            where T1 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            AddComponents_Internal(entity.Id, component1);
            ChangeArchetypeAdd_Internal<T1>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2>(in Entity entity, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            AddComponents_Internal(entity.Id, component1, component2);
            ChangeArchetypeAdd_Internal<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3>(in Entity entity, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            AddComponents_Internal(entity.Id, component1, component2, component3);
            ChangeArchetypeAdd_Internal<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComponents<T1, T2, T3, T4>(in Entity entity, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            AddComponents_Internal(entity.Id, component1, component2, component3, component4);
            ChangeArchetypeAdd_Internal<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T>(in Entity entity)
            where T : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            RemoveComponents_Internal<T>(entity.Id);
            ChangeArchetypeRemove_Internal<T>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponents<T>(in Entity entity, ref T component)
            where T : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            var result = RemoveComponents_Internal(entity.Id, ref component);
            ChangeArchetypeRemove_Internal<T>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            RemoveComponents_Internal<T1, T2>(entity.Id);
            ChangeArchetypeRemove_Internal<T1, T2>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) RemoveComponents<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2);
            ChangeArchetypeRemove_Internal<T1, T2>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            RemoveComponents_Internal<T1, T2, T3>(entity.Id);
            ChangeArchetypeRemove_Internal<T1, T2, T3>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool) RemoveComponents<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2, ref component3);
            ChangeArchetypeRemove_Internal<T1, T2, T3>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponents<T1, T2, T3, T4>(in Entity entity)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return;
            RemoveComponents_Internal<T1, T2, T3, T4>(entity.Id);
            ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool, bool) RemoveComponents<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            var result = RemoveComponents_Internal(entity.Id, ref component1, ref component2, ref component3, ref component4);
            ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(entity.Id);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponents<T1>(in Entity entity, ref T1 component1)
            where T1 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return false;
            return RefComponents_Internal(entity.Id, ref component1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return false;
            return RefComponents_Internal(entity.Id, ref component1)
                && RefComponents_Internal(entity.Id, ref component2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool) RefComponentsAny<T1, T2>(in Entity entity, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return false;
            return RefComponents_Internal(entity.Id, ref component1)
                && RefComponents_Internal(entity.Id, ref component2)
                && RefComponents_Internal(entity.Id, ref component3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (bool, bool, bool) RefComponentsAny<T1, T2, T3>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2),
                RefComponents_Internal(entity.Id, ref component3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RefComponentsAll<T1, T2, T3, T4>(in Entity entity, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return false;
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
            where T4 : struct, IComponent
        {
            if (!IsEntityValid_Internal(entity)) return default;
            return (RefComponents_Internal(entity.Id, ref component1),
                RefComponents_Internal(entity.Id, ref component2),
                RefComponents_Internal(entity.Id, ref component3),
                RefComponents_Internal(entity.Id, ref component4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1>(ComponentDelegate<T1> update)
            where T1 : struct, IComponent
        {
            // just ref-ing all components data without checking
            ref var cs1 = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            for (var i = 0; i < cs1.count; i++) {
                update(ref cs1.data.At(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1>(EntityComponentDelegate<T1> update)
            where T1 : struct, IComponent {

            // just ref-ing all components data without checking
            ref var cs1 = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            for (var i = 0; i < cs1.count; i++) {
                update(entities.AtCast<RWEntity, Entity>(cs1.dense[i]), ref cs1.data.At(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1>(UniformComponentDelegate<TU, T1> update, in TU uniform)
            where T1 : struct, IComponent
        {
            // just ref-ing all components data without checking
            ref var cs1 = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            for (var i = 0; i < cs1.count; i++) {
                update(uniform, ref cs1.data.At(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1>(EntityUniformComponentDelegate<TU, T1> update, in TU uniform)
            where T1 : struct, IComponent
        {
            // just ref-ing all components data without checking
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            for (var i = 0; i < cs1.count; i++) {
                update(entities.AtCast<RWEntity, Entity>(cs1.dense[i]), uniform, ref cs1.data.At(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2>(ComponentDelegate<T1, T2> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]]
                        );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2>(EntityComponentDelegate<T1, T2> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2>(UniformComponentDelegate<TU, T1, T2> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2>(EntityUniformComponentDelegate<TU, T1, T2> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2, T3>(ComponentDelegate<T1, T2, T3> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2, T3>(EntityComponentDelegate<T1, T2, T3> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2, T3>(UniformComponentDelegate<TU, T1, T2, T3> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2, T3>(EntityUniformComponentDelegate<TU, T1, T2, T3> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2, T3, T4>(ComponentDelegate<T1, T2, T3, T4> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
            var cs4 = Unsafe.As<Store, Store<T4>>(ref stores[CI<T4>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]],
                        ref cs4.data[cs4.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<T1, T2, T3, T4>(EntityComponentDelegate<T1, T2, T3, T4> update)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
            var cs4 = Unsafe.As<Store, Store<T4>>(ref stores[CI<T4>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]],
                        ref cs4.data[cs4.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2, T3, T4>(UniformComponentDelegate<TU, T1, T2, T3, T4> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
            var cs4 = Unsafe.As<Store, Store<T4>>(ref stores[CI<T4>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]],
                        ref cs4.data[cs4.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Iterate<TU, T1, T2, T3, T4>(EntityUniformComponentDelegate<TU, T1, T2, T3, T4> update, in TU uniform)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var cs1 = Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            var cs2 = Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
            var cs3 = Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
            var cs4 = Unsafe.As<Store, Store<T4>>(ref stores[CI<T4>.Index]);

            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count)) {
                for (var i = count - 1; i >= 0; i--) {
                    var eid = entityIds[i];
                    update(
                        entities.AtCast<RWEntity, Entity>(cs1.dense[i]),
                        uniform,
                        ref cs1.data[cs1.sparse[eid]],
                        ref cs2.data[cs2.sparse[eid]],
                        ref cs3.data[cs3.sparse[eid]],
                        ref cs4.data[cs4.sparse[eid]]
                    );
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1>()
            where T1 : struct, IComponent
            => (int)stores.At(CI<T1>.Index).count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2>.Mask, ref current, ref entityIds, ref count))
                result += count;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Count<T1, T2, T3>()
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = default;
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
            where T4 : struct, IComponent
        {
            var result = 0;
            var current = archetypes.head;
            uint[] entityIds = default;
            var count = 0;
            while (While(CI<T1, T2, T3, T4>.Mask, ref current, ref entityIds, ref count))
                result += count;
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
                    current?.Value.AttachToWorld(this);
                    current = current?.Next;
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
                    current?.Value.DetachFromWorld(this);
                    current = current?.Previous;
                } while (current != null);
            }

            defaultSystemGroup.DetachFromWorld(this);
        }

        public void EnsureCapacity(int capacity)
        {
            // no op
        }
    }
}
