using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xeno.Collections;

namespace Xeno
{
    [StructLayout(LayoutKind.Sequential)]
    internal abstract class ComponentStore
    {
        public ComponentStore<T> As<T>() where T : struct, IComponent
            => Unsafe.As<ComponentStore<T>>(this);

        internal abstract void RemoveInternal(uint id);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal sealed class ComponentStore<T> : ComponentStore where T : struct, IComponent
    {
        internal BitSet disabled;
        internal SparseSet mapping;
        internal SwapBackList<T> components;

        public ComponentStore(uint density = 1024)
        {
            disabled = new BitSet(density);
            mapping = new SparseSet(density);
            components = new SwapBackList<T>(density);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal override void RemoveInternal(uint entityId)
        {
            if (!mapping.Contains(entityId)) return;
            
            var index = mapping.Remove(entityId);
            components.RemoveAtAndSwapBack(index);
        }
    }

    internal static class ComponentStoreExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint Count<T>(this ComponentStore<T> store) where T : struct, IComponent
            => store.components.Count();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
            => store.mapping.Contains(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Ref<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
        {
            // todo point to discussion
            uint index = default;
            if (store.mapping.Contains(entityId, ref index))
                return ref store.components.At(index);

            return ref Component<T>.Default;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref T RefUnsafe<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
        {
            return ref store.components.At(store.mapping.sparse.At(entityId));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T RefAt<T>(this ComponentStore<T> store, uint index) where T : struct, IComponent
            => ref store.components.At(index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add<T>(this ComponentStore<T> store, uint entityId, in T value) where T : struct, IComponent
        {
            if (store.mapping.Contains(entityId))
                throw new IndexOutOfRangeException();

            ref var mapping = ref store.mapping;
            mapping.Add(entityId);
            ref var components = ref store.components;
            components.Add(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Set<T>(this ComponentStore<T> store, uint entityId, in T value) where T : struct, IComponent
        {
            uint index = default;
            if (store.mapping.Contains(entityId, ref index))
                store.components.At(index) = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
        {
            uint index = default;
            if (store.mapping.Contains(entityId, ref index))
                return store.components.At(index);

            return Component<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Remove<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
        {
            uint index = default;
            if (store.mapping.Contains(entityId))
            {
                store.mapping.Remove(entityId);
                return store.components.RemoveAtAndSwapBack(index);
            }

            return Component<T>.Default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveInternal<T>(this ComponentStore<T> store, uint entityId) where T : struct, IComponent
        {
            if (store.mapping.Contains(entityId)) return;
            var index = store.mapping.Remove(entityId);
            store.components.RemoveAtAndSwapBack(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetEntity<T>(this ComponentStore<T> store, uint index) where T : struct, IComponent
            => store.mapping.dense.AtRO(index);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Ensure<T>(this ComponentStore<T> store, int capacity) where T : struct, IComponent
        {
            store.disabled.Ensure(capacity);
            store.mapping.Ensure(capacity);
            store.components.Ensure((uint)capacity);
        }
    }
}