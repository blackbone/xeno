using System.Runtime.CompilerServices;
using Xeno;
using Xeno.Vendor;

namespace Xeno {
    public partial class World { // this part of class is about archetype management and
        internal Archetypes archetypes; // archetypes reorderable linked list
        internal Archetype zeroArchetype;

        internal Archetype[] entityArchetypes;
        internal uint[] inArchetypeLocalIndices;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitArchetypes(in uint capacity) {
            archetypes = new Archetypes(this);
            zeroArchetype = archetypes.AddPermanent(ref BitSetReadOnly.Zero);

            entityArchetypes = new Archetype[capacity];
            inArchetypeLocalIndices = new uint[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1>(in uint entityId) {
             archetypes.Add(CI<T1>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2>(in uint entityId) {
             archetypes.Add(CI<T1, T2>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2, T3>(in uint entityId) {
             archetypes.Add(CI<T1, T2, T3>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2, T3, T4>(in uint entityId) {
           archetypes.Add(CI<T1, T2, T3, T4>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1>.MaskSize > fromMask.maskSize ? CI<T1>.MaskSize : fromMask.maskSize]);
            mask.FromAdd(fromMask, CI<T1>.Mask);

            // archetype not changed
            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2>.MaskSize > fromMask.maskSize ? CI<T1, T2>.MaskSize : fromMask.maskSize]);
            mask.FromAdd(fromMask, CI<T1, T2>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2, T3>.MaskSize > fromMask.maskSize ? CI<T1, T2, T3>.MaskSize : fromMask.maskSize]);
            mask.FromAdd(fromMask, CI<T1, T2, T3>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3, T4>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2, T3, T4>.MaskSize > fromMask.maskSize ? CI<T1, T2, T3, T4>.MaskSize : fromMask.maskSize]);
            mask.FromAdd(fromMask, CI<T1, T2, T3, T4>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1>.MaskSize > fromMask.maskSize ? CI<T1>.MaskSize : fromMask.maskSize]);
            mask.FromRemove(fromMask, CI<T1>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2>.MaskSize > fromMask.maskSize ? CI<T1, T2>.MaskSize : fromMask.maskSize]);
            mask.FromRemove(fromMask, CI<T1, T2>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2, T3>.MaskSize > fromMask.maskSize ? CI<T1, T2, T3>.MaskSize : fromMask.maskSize]);
            mask.FromRemove(fromMask, CI<T1, T2, T3>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[CI<T1, T2, T3, T4>.MaskSize > fromMask.maskSize ? CI<T1, T2, T3, T4>.MaskSize : fromMask.maskSize]);
            mask.FromRemove(fromMask, CI<T1, T2, T3, T4>.Mask);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveEntityFromArchetype_Internal(in uint entityId) {
            archetypes.Remove(entityArchetypes[entityId], entityId, inArchetypeLocalIndices);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddGeneratedMask(in Entity entity, in BitSetReadOnly mask) {
            AssertOwnerThread();
            if (!IsEntityValid_Internal(entity))
                return;
            AddGeneratedMask_NoLock_Valid(entity.Id, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveGeneratedMask(in Entity entity, in BitSetReadOnly mask) {
            AssertOwnerThread();
            if (!IsEntityValid_Internal(entity))
                return;
            RemoveGeneratedMask_NoLock_Valid(entity.Id, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddGeneratedMask_NoLock_Valid(in uint entityId, in BitSetReadOnly mask) {
            ChangeArchetypeAdd_Internal(entityId, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveGeneratedMask_NoLock_Valid(in uint entityId, in BitSetReadOnly mask) {
            ChangeArchetypeRemove_Internal(entityId, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal(in uint entityId, in BitSetReadOnly toAdd) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[toAdd.maskSize > fromMask.maskSize ? toAdd.maskSize : fromMask.maskSize]);
            mask.FromAdd(fromMask, toAdd);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal(in uint entityId, in BitSetReadOnly toRemove) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            if (fromMask.maskSize <= 1 && toRemove.maskSize <= 1) {
                var value = fromMask.data[0] & ~toRemove.data[0];
                if (value == fromMask.data[0])
                    return;

                var fastMask = new BitSet(stackalloc ulong[1]);
                fastMask.data[0] = value;
                fastMask.hash = value;
                fastMask.max = value != 0 ? BitOperations.Log2(value) : 0;
                fastMask.maskSize = 1;

                archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
                archetypes.Add(ref fastMask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
                return;
            }

            var mask = new BitSet(stackalloc ulong[toRemove.maskSize > fromMask.maskSize ? toRemove.maskSize : fromMask.maskSize]);
            mask.FromRemove(fromMask, toRemove);

            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices, false);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }
    }
}
