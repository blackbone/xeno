using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xeno;

// ReSharper disable BadChildStatementIndent
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Xeno {
    public sealed partial class World_Old { // this part class is work with component data, not more or less
        internal Store[] stores;
        private uint storeCapacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponents(in uint storesCount, in uint entitiesCount) {
            stores = new Store[storesCount];
            storeCapacity = entitiesCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1>(in uint entityId, in T1 component1)
            where T1 : struct, IComponent
        {
            {
                if (CI<T1>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
                s ??= new Store<T1>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component1;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2>(in uint entityId, in T1 component1, in T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            {
                if (CI<T1>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
                s ??= new Store<T1>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component1;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T2>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
                s ??= new Store<T2>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component2;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2, T3>(in uint entityId, in T1 component1, in T2 component2, in T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            {
                if (CI<T1>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
                s ??= new Store<T1>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component1;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T2>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
                s ??= new Store<T2>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component2;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T3>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
                s ??= new Store<T3>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component3;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2, T3, T4>(in uint entityId, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            {
                if (CI<T1>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
                s ??= new Store<T1>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component1;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T2>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T2>>(ref stores[CI<T2>.Index]);
                s ??= new Store<T2>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component2;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T3>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T3>>(ref stores[CI<T3>.Index]);
                s ??= new Store<T3>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component3;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
            {
                if (CI<T4>.Index >= stores.Length) Array.Resize(ref stores, stores.Length << 1);
                ref var s = ref Unsafe.As<Store, Store<T4>>(ref stores[CI<T4>.Index]);
                s ??= new Store<T4>(storeCapacity);
                if (s.count == s.dense.Length) {
                    Array.Resize(ref s.dense, s.dense.Length << 1);
                    Array.Resize(ref s.data, s.data.Length << 1);
                }

                var c = s.count;
                s.data[c] = component4;
                s.sparse[entityId] = c;
                s.dense[c] = entityId;
                s.count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal(in uint entityId, in BitSetReadOnly mask) {
            foreach (var index in mask.indices) {
                ref var s = ref stores[index];
                if (s == null) continue;

                var d1 = s.sparse[entityId];
                if (d1 >= s.count) return;

                ref var sp = ref s.dense[d1];
                if (sp != entityId) return;

                var last = --s.count;

                var ld =  s.dense[last];
                sp = ld;
                s.sparse[entityId] = d1;

                s.SwapData_Internal(d1, last);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1>(in uint entityId)
            where T1 : struct, IComponent
        {
            ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            if (s == null) return;

            var d1 = s.sparse[entityId];
            if (d1 >= s.count) return;

            var sp = s.dense.At(d1);
            if (sp != entityId) return;

            var last = --s.count;

            s.data.At(d1) = s.data.At(last);

            var ld = s.dense.At(last);
            s.dense.At(d1) = ld;
            s.sparse.At(ld) = d1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2, T3>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
            RemoveComponents_Internal<T3>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2, T3, T4>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
            RemoveComponents_Internal<T3>(entityId);
            RemoveComponents_Internal<T4>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RemoveComponents_Internal<T1>(in uint entityId, ref T1 component1)
            where T1 : struct, IComponent {
            ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            if (s == null) return false;

            var d1 = s.sparse[entityId];
            if (d1 > s.count) return false;

            var sp = s.dense.At(d1);
            if (sp != entityId) return false;

            var last = --s.count;

            component1 = s.data.At(d1);
            s.data.At(d1) = s.data.At(last);

            var ld = s.dense.At(last);
            s.dense.At(d1) = ld;
            s.sparse.At(ld) = d1;

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool) RemoveComponents_Internal<T1, T2>(in uint entityId, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool) RemoveComponents_Internal<T1, T2, T3>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2),
                RemoveComponents_Internal(entityId, ref component3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool, bool) RemoveComponents_Internal<T1, T2, T3, T4>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2),
                RemoveComponents_Internal(entityId, ref component3),
                RemoveComponents_Internal(entityId, ref component4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasComponent_Internal<T1>(in uint entityId)
            where T1 : struct, IComponent
        {
            ref var s = ref stores.At(CI<T1>.Index);
            if (s == null) return false;

            var d = s.sparse[entityId];
            return d < s.count && s.dense[entityId] == entityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2, T3>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId)
                && HasComponent_Internal<T3>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2, T3, T4>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId)
                && HasComponent_Internal<T3>(entityId)
                && HasComponent_Internal<T4>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
                || HasComponent_Internal<T2>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2, T3>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
            || HasComponent_Internal<T2>(entityId)
            || HasComponent_Internal<T3>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2, T3, T4>(in uint entityId)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            => HasComponent_Internal<T1>(entityId)
                || HasComponent_Internal<T2>(entityId)
                || HasComponent_Internal<T3>(entityId)
                || HasComponent_Internal<T4>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RefComponents_Internal<T1>(in uint entityId, ref T1 component1)
            where T1 : struct, IComponent {
            ref var s = ref Unsafe.As<Store, Store<T1>>(ref stores[CI<T1>.Index]);
            if (s == null) return false;

            var d = s.sparse[entityId];
            if (d > s.count) return false;
            var sp = s.dense[d];
            if (sp != entityId) return false;
            component1 = s.data[d];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool) RefComponents_Internal<T1, T2>(in uint entityId, ref T1 component1, ref T2 component2)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool) RefComponents_Internal<T1, T2, T3>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2),
                RefComponents_Internal(entityId, ref component3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool, bool) RefComponents_Internal<T1, T2, T3, T4>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            where T1 : struct, IComponent
            where T2 : struct, IComponent
            where T3 : struct, IComponent
            where T4 : struct, IComponent
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2),
                RefComponents_Internal(entityId, ref component3),
                RefComponents_Internal(entityId, ref component4));
    }
}
