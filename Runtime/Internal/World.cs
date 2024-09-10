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

            componentStores = new AutoGrowOnlyList<ComponentStore>(16);
            entities = new EntityJournal(this, 16384);
            
            Worlds.Add(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ComponentStore<T> Components<T>() where T : struct, IComponent
        {
            ref var slot = ref componentStores.At(Component<T>.Index);
            if (slot != null) return slot.As<T>();

            var store = new ComponentStore<T>();
            componentStores.At(Component<T>.Index) = store;
            return store;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1>(uint entityId)
            where T1 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Set(Component<T1>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index)
                .Set(Component<T4>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T>(uint entityId) where T : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Unset(Component<T>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            var oldArchetype = entities.archetypes[entityId];
            ref var newArchetype = ref entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index)
                .Unset(Component<T4>.Index);

            entities.ChangeArchetype(entityId, oldArchetype, newArchetype);
        }
    }
}