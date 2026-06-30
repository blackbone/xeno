using System;
using System.Runtime.CompilerServices;
using Xeno.Vendor;

// ReSharper disable BadChildStatementIndent
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Xeno {
    public partial class World { // this part class is work with component data, not more or less
        public Store3[] stores2;
        private uint storeCapacity;

        private uint pid;
        private uint slot;

        private int StorePageCapacity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => storeCapacity == 0 ? 32 : (int)((storeCapacity + Store3.Mask) >> Store3.Shift);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponents(in uint storesCount, in uint entitiesCount) {
            stores2 = new Store3[storesCount];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1>(in uint entityId, in T1 component1)
        {
            pid = entityId >> Store3.Shift;
            slot = entityId & Store3.Mask;
            {
                if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
                s ??= new Store3<T1>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T1[Store3.Cap];
                s.pages[pid][slot] = component1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2>(in uint entityId, in T1 component1, in T2 component2)
        {
            pid = entityId >> Store3.Shift;
            slot = entityId & Store3.Mask;
            {
                if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
                s ??= new Store3<T1>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T1[Store3.Cap];
                s.pages[pid][slot] = component1;
            }
            {
                if (CI<T2>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T2>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T2>>(ref stores2[CI<T2>.Index]);
                s ??= new Store3<T2>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T2[Store3.Cap];
                s.pages[pid][slot] = component2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2, T3>(in uint entityId, in T1 component1, in T2 component2, in T3 component3)
        {
            pid = entityId >> Store3.Shift;
            slot = entityId & Store3.Mask;
            {
                if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
                s ??= new Store3<T1>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T1[Store3.Cap];
                s.pages[pid][slot] = component1;
            }
            {
                if (CI<T2>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T2>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T2>>(ref stores2[CI<T2>.Index]);
                s ??= new Store3<T2>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T2[Store3.Cap];
                s.pages[pid][slot] = component2;
            }
            {
                if (CI<T3>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T3>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T3>>(ref stores2[CI<T3>.Index]);
                s ??= new Store3<T3>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T3[Store3.Cap];
                s.pages[pid][slot] = component3;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddComponents_Internal<T1, T2, T3, T4>(in uint entityId, in T1 component1, in T2 component2, in T3 component3, in T4 component4)
        {
            pid = entityId >> Store3.Shift;
            slot = entityId & Store3.Mask;
            {
                if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
                s ??= new Store3<T1>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T1[Store3.Cap];
                s.pages[pid][slot] = component1;
            }
            {
                if (CI<T2>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T2>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T2>>(ref stores2[CI<T2>.Index]);
                s ??= new Store3<T2>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T2[Store3.Cap];
                s.pages[pid][slot] = component2;
            }
            {
                if (CI<T3>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T3>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T3>>(ref stores2[CI<T3>.Index]);
                s ??= new Store3<T3>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T3[Store3.Cap];
                s.pages[pid][slot] = component3;
            }
            {
                if (CI<T4>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T4>.Index) + 1);
                ref var s = ref Unsafe.As<Store3, Store3<T4>>(ref stores2[CI<T4>.Index]);
                s ??= new Store3<T4>(StorePageCapacity);
                if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
                s.pages[pid] ??= new T4[Store3.Cap];
                s.pages[pid][slot] = component4;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal(in uint entityId, in BitSetReadOnly mask) {
            var indices = mask.indices;
            for (var i = 0; i < indices.Length; i++) {
                var index = (int)indices[i];
                ref var s = ref stores2[index];
                if (s == null) continue;

                s.Remove_Internal(entityId);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1>(in uint entityId)
        {
            if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
            ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
            if (s == null) return;

            pid = entityId >> Store3.Shift;
            if (pid >= s.pages.Length) return;

            var page = s.pages[pid];
            if (page == null) return;

            slot = entityId & Store3.Mask;
            page[slot] = default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2>(in uint entityId)
            {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2, T3>(in uint entityId)
        {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
            RemoveComponents_Internal<T3>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveComponents_Internal<T1, T2, T3, T4>(in uint entityId)
        {
            RemoveComponents_Internal<T1>(entityId);
            RemoveComponents_Internal<T2>(entityId);
            RemoveComponents_Internal<T3>(entityId);
            RemoveComponents_Internal<T4>(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RemoveComponents_Internal<T1>(in uint entityId, ref T1 component1)
            {
            if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
            ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
            if (s == null) return false;
            pid = entityId >> Store3.Shift;
            if (pid >= s.pages.Length) return false;
            if (s.pages[pid] == null) return false;
            slot = entityId & Store3.Mask;
            component1 = s.pages[pid][slot];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool) RemoveComponents_Internal<T1, T2>(in uint entityId, ref T1 component1, ref T2 component2)
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool) RemoveComponents_Internal<T1, T2, T3>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3)
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2),
                RemoveComponents_Internal(entityId, ref component3));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool, bool) RemoveComponents_Internal<T1, T2, T3, T4>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
        {
            return (
                RemoveComponents_Internal(entityId, ref component1),
                RemoveComponents_Internal(entityId, ref component2),
                RemoveComponents_Internal(entityId, ref component3),
                RemoveComponents_Internal(entityId, ref component4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasComponent_Internal<T1>(in uint entityId)
        {
            return entityArchetypes[entityId].mask.Includes(ref CI<T1>.Mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2, T3>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId)
                && HasComponent_Internal<T3>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAllComponents_Internal<T1, T2, T3, T4>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
                && HasComponent_Internal<T2>(entityId)
                && HasComponent_Internal<T3>(entityId)
                && HasComponent_Internal<T4>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
                || HasComponent_Internal<T2>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2, T3>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
            || HasComponent_Internal<T2>(entityId)
            || HasComponent_Internal<T3>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasAnyComponents_Internal<T1, T2, T3, T4>(in uint entityId)
            => HasComponent_Internal<T1>(entityId)
                || HasComponent_Internal<T2>(entityId)
                || HasComponent_Internal<T3>(entityId)
                || HasComponent_Internal<T4>(entityId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T1 RefComponent_Internal<T1>(in uint entityId)
            {
            if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
            ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
            s ??= new Store3<T1>(StorePageCapacity);
            pid = entityId >> Store3.Shift;
            if (pid >= s.pages.Length) Array.Resize(ref s.pages, (int)BitOperations.Smear(pid) + 1);
            s.pages[pid] ??= new T1[Store3.Cap];
            slot = entityId & Store3.Mask;
            return ref s.pages[pid][slot];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool RefComponents_Internal<T1>(in uint entityId, ref T1 component1)
            {
            if (CI<T1>.Index >= stores2.Length) Array.Resize(ref stores2, BitOperations.Smear(CI<T1>.Index) + 1);
            ref var s = ref Unsafe.As<Store3, Store3<T1>>(ref stores2[CI<T1>.Index]);
            if (s == null) return false;
            pid = entityId >> Store3.Shift;
            if (pid >= s.pages.Length) return false;
            if (s.pages[pid] == null) return false;
            slot = entityId & Store3.Mask;
            component1 = s.pages[pid][slot];
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool) RefComponents_Internal<T1, T2>(in uint entityId, ref T1 component1, ref T2 component2)
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool) RefComponents_Internal<T1, T2, T3>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3)
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2),
                RefComponents_Internal(entityId, ref component3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (bool, bool, bool, bool) RefComponents_Internal<T1, T2, T3, T4>(in uint entityId, ref T1 component1, ref T2 component2, ref T3 component3, ref T4 component4)
            => (
                RefComponents_Internal(entityId, ref component1),
                RefComponents_Internal(entityId, ref component2),
                RefComponents_Internal(entityId, ref component3),
                RefComponents_Internal(entityId, ref component4));
    }
}
