using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xeno.Collections;

namespace Xeno
{
    public sealed partial class World
    {
        private readonly SystemGroup defaultSystemGroup = new("Default");
        private readonly LinkedList<SystemGroup> systemGroups = new();

        private AutoGrowOnlyList<ComponentStore> componentStores;
        private EntityJournal entities;
        
        internal event Action Started;
        internal event UpdateDelegate PreUpdate; 
        internal event UpdateDelegate Update; 
        internal event UpdateDelegate PostUpdate;
        internal event Action Stopped;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal World(string name, byte id)
        {
            Name = name;
            Id = id;

            componentStores = new AutoGrowOnlyList<ComponentStore>(16, 16);
            entities = new EntityJournal(this, 4096, 32768, 4);
            entities.SetupDefaults(4096, 32768, 4);
            
            Worlds.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentStore Components(in int componentIndex) => componentStores.data[componentIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentStore<T> Components<T>() where T : struct, IComponent
        {
            if (componentStores.data.Length < Component<T>.Index)
                Array.Resize(ref componentStores.data, Component<T>.Index);

            ref var slot = ref componentStores.data[Component<T>.Index];
            if (slot != null) return (ComponentStore<T>)slot;

            var store = new ComponentStore<T>();
            componentStores.data[Component<T>.Index] = store;
            return store;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1>(uint entityId)
            where T1 : struct, IComponent {

            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Set(Component<T1>.Index)
                .Finalize();
            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index)
                .Set(Component<T4>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T>(uint entityId) where T : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Unset(Component<T>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index)
                .Finalize();

            entities.ChangeArchetype(entityId, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var newArchetype = entities.entityArchetypes.data[entityId].mask;
            newArchetype
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index)
                .Unset(Component<T4>.Index)
                .Finalize();

             entities.ChangeArchetype(entityId, newArchetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool While(in FixedBitSet mask, ref Archetype current, ref uint[] data, ref int count) {
            while (current != null && !current.mask.Includes(mask))
                current = current.next;

            if (current == null) return false;

            data = current.entities.data;
            count = current.entities.count;
            current = current.next;
            return true;
        }
    }
}