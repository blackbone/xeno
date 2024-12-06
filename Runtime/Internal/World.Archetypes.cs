using System.Runtime.CompilerServices;
using Xeno;

namespace Xeno {
    public sealed partial class World { // this part of class is about archetype management and
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
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1>.Mask.max, fromMask.max)]);
            mask.FromAdd(fromMask, CI<T1>.Mask);

            // archetype not changed
            if (fromArchetype.mask.Equals(mask)) return;

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2>.Mask.max, fromMask.max)]);
            mask.FromAdd(fromMask, CI<T1, T2>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2, T3>.Mask.max, fromMask.max)]);
            mask.FromAdd(fromMask, CI<T1, T2, T3>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3, T4>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2, T3, T4>.Mask.max, fromMask.max)]);
            mask.FromAdd(fromMask, CI<T1, T2, T3, T4>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1>.Mask.max, fromMask.max)]);
            mask.FromRemove(fromMask, CI<T1>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2>.Mask.max, fromMask.max)]);
            mask.FromRemove(fromMask, CI<T1, T2>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2, T3>.Mask.max, CI<T1>.Index)]);
            mask.FromRemove(fromMask, CI<T1, T2, T3>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(in uint entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            ref var fromMask = ref fromArchetype.mask;
            var mask = new BitSet(stackalloc ulong[BitSet.MaskSize(CI<T1, T2, T3, T4>.Mask.max, fromMask.max)]);
            mask.FromRemove(fromMask, CI<T1, T2, T3, T4>.Mask);

            archetypes.Remove(fromArchetype, entityId, inArchetypeLocalIndices);
            archetypes.Add(ref mask, entityId, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveEntityFromArchetype_Internal(in uint entityId) {
            archetypes.Remove(entityArchetypes[entityId], entityId, inArchetypeLocalIndices);
        }
    }
}
