using System.Runtime.CompilerServices;
using Xeno;
using Xeno.Vendor;

namespace Xeno {
    public partial class World { // this part of class is about archetype management and
        internal Archetypes archetypes; // archetypes reorderable linked list
        internal Archetype zeroArchetype;

        internal Archetype[] entityArchetypes;
        internal int[] inArchetypeLocalIndices;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitArchetypes(in int capacity) {
            archetypes = new Archetypes(this);
            zeroArchetype = archetypes.AddPermanent(ref BitSetReadOnly.Zero);

            entityArchetypes = new Archetype[capacity];
            inArchetypeLocalIndices = new int[capacity];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1>(in int entityId) {
             archetypes.Add(CI<T1>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2>(in int entityId) {
             archetypes.Add(CI<T1, T2>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2, T3>(in int entityId) {
             archetypes.Add(CI<T1, T2, T3>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToArchetype_Internal<T1, T2, T3, T4>(in int entityId) {
           archetypes.Add(CI<T1, T2, T3, T4>.Mask, entityId, out entityArchetypes[entityId], out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, CI<T1>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, CI<T1, T2>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, CI<T1, T2, T3>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal<T1, T2, T3, T4>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, CI<T1, T2, T3, T4>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, CI<T1>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, CI<T1, T2>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, CI<T1, T2, T3>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal<T1, T2, T3, T4>(in int entityId)
        {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, CI<T1, T2, T3, T4>.Mask, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveEntityFromArchetype_Internal(in int entityId) {
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
        protected void AddGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask) {
            ChangeArchetypeAdd_Internal(entityId, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask, int key) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, in mask, key, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AddGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask, int key, ref object sourceCache, ref object targetCache) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            var cachedSource = sourceCache as Archetype;
            var cachedTarget = targetCache as Archetype;
            if (cachedTarget != null && ReferenceEquals(fromArchetype, cachedSource)) {
                archetypes.MoveKnownTarget(fromArchetype, cachedTarget, entityId, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
                return;
            }

            var source = fromArchetype;
            archetypes.MoveAdd(fromArchetype, entityId, in mask, key, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
            sourceCache = source;
            targetCache = fromArchetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask) {
            ChangeArchetypeRemove_Internal(entityId, in mask);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask, int key) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, in mask, key, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void RemoveGeneratedMask_NoLock_Valid(in int entityId, in BitSetReadOnly mask, int key, ref object sourceCache, ref object targetCache) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            var cachedSource = sourceCache as Archetype;
            var cachedTarget = targetCache as Archetype;
            if (cachedTarget != null && ReferenceEquals(fromArchetype, cachedSource)) {
                archetypes.MoveKnownTarget(fromArchetype, cachedTarget, entityId, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
                return;
            }

            var source = fromArchetype;
            archetypes.MoveRemove(fromArchetype, entityId, in mask, key, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
            sourceCache = source;
            targetCache = fromArchetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeAdd_Internal(in int entityId, in BitSetReadOnly toAdd) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveAdd(fromArchetype, entityId, in toAdd, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ChangeArchetypeRemove_Internal(in int entityId, in BitSetReadOnly toRemove) {
            ref var fromArchetype = ref entityArchetypes[entityId];
            archetypes.MoveRemove(fromArchetype, entityId, in toRemove, inArchetypeLocalIndices, out fromArchetype, out inArchetypeLocalIndices[entityId]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void ClearGeneratedEntityData(in int entityId, in BitSetReadOnly mask) {
        }
    }
}
