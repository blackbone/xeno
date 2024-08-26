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
        private BitSet disabled;
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

            entities = new EntityJournal(this);
            componentStores = new AutoGrowOnlyList<ComponentStore>(16);
            disabled = new BitSet(1024);
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
        internal void Enable(uint entityId)
            => disabled.Unset(entityId);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Disable(uint entityId)
            => disabled.Set(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1>(uint entityId)
            where T1 : struct, IComponent
        {
            entities.archetypes[entityId].Set(Component<T1>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddToArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Set(Component<T1>.Index)
                .Set(Component<T2>.Index)
                .Set(Component<T3>.Index)
                .Set(Component<T4>.Index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T>(uint entityId) where T : struct, IComponent
        {
            entities.archetypes[entityId].Unset(Component<T>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RemoveFromArchetype<T1, T2, T3, T4>(uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            entities.archetypes[entityId]
                .Unset(Component<T1>.Index)
                .Unset(Component<T2>.Index)
                .Unset(Component<T3>.Index)
                .Unset(Component<T4>.Index);
        }
    }
}