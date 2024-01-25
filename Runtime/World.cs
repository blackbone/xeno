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
            get => entities.Count;
        }

        public void Dispose() => Worlds.Remove(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity() => entities.Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T>(in T component)
            where T : unmanaged, IComponent
        {
            var entity = entities.Create();
            var components = Components<T>();
            components.Add(entity.Id, component);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2>(in T1 component1, in T2 component2)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3>(in T1 component1, in T2 component2, in T3 component3)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity<T1, T2, T3, T4>(in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
        {
            var entity = entities.Create();
            Components<T1>().Add(entity.Id, component1);
            Components<T2>().Add(entity.Id, component2);
            Components<T3>().Add(entity.Id, component3);
            Components<T4>().Add(entity.Id, component4);
            return entity;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DeleteEntity(in Entity entity)
        {
            if (entities.Ref(entity.Id).Version != entity.Version) throw new InvalidOperationException();
            entities.Remove(entity.Id);
            // for (var i = 0; i < Component.Index; i++)
            //     componentStores[(uint)i].RemoveInternal(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Entity> Entities()
        {
            for (uint i = 0; i < entities.Count; i++)
            {
                // if (disabled.Get(i)) continue;
                yield return entities.Ref(i);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T>(ComponentDelegate<T> update)
            where T : unmanaged, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                // if (disabled.Get(entityId)) continue;

                update(ref cs1.RefAt(i));
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T>(DeltaComponentDelegate<T> update)
            where T : unmanaged, IComponent
        {
             var cs1 =  componentStores.AtRO(Component<T>.Index).As<T>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                // var entityId = cs1.GetEntity(i);
                // if (disabled.Get(entityId)) continue;

                update(0f, ref cs1.RefAt(i));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2>(ComponentDelegate<T1, T2> update)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                // if (disabled.Get(entityId)) continue;
                if (!cs2.Has(entityId)) continue;

                update(ref cs1.RefAt(i), ref cs2.Ref(entityId));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3>(ComponentDelegate<T1, T2, T3> update)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
        {
            var cs1 = componentStores.AtRO(Component<T1>.Index).As<T1>();
            var cs2 = componentStores.AtRO(Component<T2>.Index).As<T2>();
            var cs3 = componentStores.AtRO(Component<T3>.Index).As<T3>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                // if (disabled.Get(entityId)) continue;
                if (!cs2.Has(entityId)) continue;
                if (!cs3.Has(entityId)) continue;

                update(ref cs1.RefAt(i), ref cs2.Ref(entityId), ref cs3.Ref(entityId));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Entities<T1, T2, T3, T4>(ComponentDelegate<T1, T2, T3, T4> update)
            where T1 : unmanaged, IComponent
            where T2 : unmanaged, IComponent
            where T3 : unmanaged, IComponent
            where T4 : unmanaged, IComponent
        {
             var cs1 =  componentStores.AtRO(Component<T1>.Index).As<T1>();
             var cs2 =  componentStores.AtRO(Component<T2>.Index).As<T2>();
             var cs3 =  componentStores.AtRO(Component<T3>.Index).As<T3>();
             var cs4 =  componentStores.AtRO(Component<T4>.Index).As<T4>();
            var count = cs1.Count();
            for (uint i = 0; i < count; i++)
            {
                var entityId = cs1.GetEntity(i);
                // if (disabled.Get(entityId)) continue;
                if (!cs2.Has(entityId)) continue;
                if (!cs3.Has(entityId)) continue;
                if (!cs4.Has(entityId)) continue;

                update(ref cs1.RefAt(i), ref cs2.Ref(entityId), ref cs3.Ref(entityId), ref cs4.Ref(entityId));
            }
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
        
        public ulong Ticks { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Tick(float f)
        {
            PreUpdate?.Invoke(f);
            Update?.Invoke(f);
            PostUpdate?.Invoke(f);
            Ticks++;
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

            Ticks = 0ul;
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
    }
}