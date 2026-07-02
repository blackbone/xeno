using System;
using System.Runtime.CompilerServices;

namespace Xeno {
    internal sealed class Archetypes {
        private const byte AddKind = 1;
        private const byte RemoveKind = 2;
        private const int InitialTransitions = 4;
        private const int TransitionCacheSize = 4;

        internal readonly World world;
        internal Archetype[] freeArchetypes;
        internal int freeArchetypesCount;
        internal Archetype head;
        internal ArchetypeTransitionCache[] transitionCache;
        internal int transitionCacheCount;

        public Archetypes(World world) {
            this.world = world;
            freeArchetypes = new Archetype[Constants.PreInitializedArchetypesCount];
            transitionCache = new ArchetypeTransitionCache[TransitionCacheSize];
            freeArchetypesCount = Constants.PreInitializedArchetypesCount;
            for (var i = 0; i < freeArchetypesCount; i++)
                freeArchetypes[i] = new Archetype(true, world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Archetype AddPermanent(ref BitSetReadOnly mask) {
            var node = new Archetype(false, world) {
                mask = mask
            };
            PushHead(node);
            return node;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref BitSet mask, in int entityId, out Archetype archetype, out int inArchetypeLocalIndex) {
            AddToExisting(FindOrCreate(ref mask), entityId, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(BitSetReadOnly mask, int entityId, out Archetype archetype, out int inArchetypeLocalIndex) {
            AddToExisting(FindOrCreate(in mask), entityId, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddCached(BitSetReadOnly mask, ref object cache, int entityId, out Archetype archetype, out int inArchetypeLocalIndex) {
            var target = cache as Archetype;
            if (target == null) {
                target = FindOrCreate(in mask);
                cache = target;
            }

            AddToExisting(target, entityId, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveAdd(Archetype from, in int entityId, in BitSetReadOnly addMask, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            MoveAdd_Internal(from, entityId, in addMask, 0, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveAdd(Archetype from, in int entityId, in BitSetReadOnly addMask, int key, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            MoveAdd_Internal(from, entityId, in addMask, key, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveRemove(Archetype from, in int entityId, in BitSetReadOnly removeMask, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            MoveRemove_Internal(from, entityId, in removeMask, 0, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MoveRemove(Archetype from, in int entityId, in BitSetReadOnly removeMask, int key, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            MoveRemove_Internal(from, entityId, in removeMask, key, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveAdd_Internal(Archetype from, int entityId, in BitSetReadOnly addMask, int key, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            if (!TryGetCachedTransition(from, AddKind, key, in addMask, out var target)) {
                if (!TryGetTransition(from, AddKind, key, in addMask, out target)) {
                    ref var fromMask = ref from.mask;
                    var mask = new BitSet(stackalloc ulong[addMask.maskSize > fromMask.maskSize ? addMask.maskSize : fromMask.maskSize]);
                    mask.FromAdd(fromMask, addMask);

                    target = fromMask.Equals(mask) ? from : FindOrCreate(ref mask);
                    StoreTransition(from, AddKind, key, in addMask, target);
                }

                StoreCachedTransition(from, AddKind, key, in addMask, target);
            }

            if (ReferenceEquals(from, target)) {
                archetype = from;
                inArchetypeLocalIndex = inArchetypeLocalIndices[entityId];
                return;
            }

            MoveKnownTarget(from, target, entityId, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MoveRemove_Internal(Archetype from, int entityId, in BitSetReadOnly removeMask, int key, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            if (!TryGetCachedTransition(from, RemoveKind, key, in removeMask, out var target)) {
                if (!TryGetTransition(from, RemoveKind, key, in removeMask, out target)) {
                    ref var fromMask = ref from.mask;
                    var mask = new BitSet(stackalloc ulong[removeMask.maskSize > fromMask.maskSize ? removeMask.maskSize : fromMask.maskSize]);
                    mask.FromRemove(fromMask, removeMask);

                    target = fromMask.Equals(mask) ? from : FindOrCreate(ref mask);
                    StoreTransition(from, RemoveKind, key, in removeMask, target);
                }

                StoreCachedTransition(from, RemoveKind, key, in removeMask, target);
            }

            if (ReferenceEquals(from, target)) {
                archetype = from;
                inArchetypeLocalIndex = inArchetypeLocalIndices[entityId];
                return;
            }

            MoveKnownTarget(from, target, entityId, inArchetypeLocalIndices, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void MoveKnownTarget(Archetype from, Archetype target, int entityId, int[] inArchetypeLocalIndices, out Archetype archetype, out int inArchetypeLocalIndex) {
            if (ReferenceEquals(from, target)) {
                archetype = from;
                inArchetypeLocalIndex = inArchetypeLocalIndices[entityId];
                return;
            }

            Remove(from, entityId, inArchetypeLocalIndices);
            AddToExisting(target, entityId, out archetype, out inArchetypeLocalIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryGetCachedTransition(Archetype from, byte kind, int key, in BitSetReadOnly mask, out Archetype target) {
            for (var i = 0; i < transitionCacheCount; i++) {
                ref var transition = ref transitionCache[i];
                if (ReferenceEquals(transition.From, from)
                    && transition.Kind == kind
                    && (key != 0 ? transition.Key == key : transition.Mask.Equals(mask))) {
                    target = transition.Target;
                    if (i > 0) {
                        var hit = transition;
                        for (var j = i; j > 0; j--)
                            transitionCache[j] = transitionCache[j - 1];
                        transitionCache[0] = hit;
                    }
                    return true;
                }
            }

            target = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StoreCachedTransition(Archetype from, byte kind, int key, in BitSetReadOnly mask, Archetype target) {
            var count = transitionCacheCount;
            if (count < TransitionCacheSize)
                transitionCacheCount = ++count;

            for (var i = count - 1; i > 0; i--)
                transitionCache[i] = transitionCache[i - 1];

            transitionCache[0] = new ArchetypeTransitionCache {
                From = from,
                Kind = kind,
                Key = key,
                Mask = mask,
                Target = target
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(Archetype archetype, int entityId, int[] inArchetypeLocalIndices) {
            var localArchetypeIndex = inArchetypeLocalIndices[entityId];
            if (localArchetypeIndex >= archetype.entitiesCount)
                throw new IndexOutOfRangeException($"entityId: {entityId}, localIndex: {localArchetypeIndex}, entityCount: {archetype.entitiesCount}");

            var lastIndex = archetype.entitiesCount - 1;
            if (localArchetypeIndex != lastIndex) {
                var currentLastId = archetype.entities[lastIndex];
                archetype.entities[localArchetypeIndex] = currentLastId;
                inArchetypeLocalIndices[currentLastId] = localArchetypeIndex;
            }

            archetype.entitiesCount = lastIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Archetype FindOrCreate(ref BitSet mask) {
            var v = head;
            while (v != null && (v.mask.hash != mask.hash || !v.mask.Equals(mask)))
                v = v.next;

            if (v == null) {
                v = AllocateFloating();
                v.mask = mask.AsReadOnly();
                PushHead(v);
                world.InvalidateQueryCache_Internal();
                return v;
            }

            Touch(v);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Archetype FindOrCreate(in BitSetReadOnly mask) {
            var v = head;
            while (v != null && (v.mask.hash != mask.hash || !v.mask.Equals(mask)))
                v = v.next;

            if (v == null) {
                v = AllocateFloating();
                v.mask = mask;
                PushHead(v);
                world.InvalidateQueryCache_Internal();
                return v;
            }

            Touch(v);
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Archetype AllocateFloating() {
            return freeArchetypesCount > 0
                ? freeArchetypes[--freeArchetypesCount]
                : new Archetype(true, world);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddToExisting(Archetype target, int entityId, out Archetype archetype, out int inArchetypeLocalIndex) {
            var len = target.entities.Length;
            if (target.entitiesCount == len)
                Array.Resize(ref target.entities, len << 1);

            target.entities[target.entitiesCount] = entityId;
            inArchetypeLocalIndex = target.entitiesCount;
            target.entitiesCount++;
            archetype = target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryGetTransition(Archetype from, byte kind, int key, in BitSetReadOnly mask, out Archetype target) {
            var transitions = from.transitions;
            for (var i = 0; i < from.transitionsCount; i++) {
                ref var transition = ref transitions[i];
                if (transition.Kind == kind && (key != 0 ? transition.Key == key : transition.Mask.Equals(mask))) {
                    target = transition.Target;
                    return true;
                }
            }

            target = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void StoreTransition(Archetype from, byte kind, int key, in BitSetReadOnly mask, Archetype target) {
            var transitions = from.transitions;
            if (transitions == null) {
                transitions = new ArchetypeTransition[InitialTransitions];
                from.transitions = transitions;
            }
            else if (from.transitionsCount == transitions.Length) {
                Array.Resize(ref transitions, transitions.Length << 1);
                from.transitions = transitions;
            }

            transitions[from.transitionsCount++] = new ArchetypeTransition {
                Kind = kind,
                Key = key,
                Mask = mask,
                Target = target
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void PushHead(Archetype archetype) {
            archetype.prev = null;
            archetype.next = head;
            if (head != null)
                head.prev = archetype;
            head = archetype;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Touch(Archetype archetype) {
            if (archetype == head)
                return;

            Unlink(archetype);
            PushHead(archetype);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unlink(Archetype archetype) {
            if (ReferenceEquals(head, archetype))
                head = archetype.next;
            if (archetype.prev != null)
                archetype.prev.next = archetype.next;
            if (archetype.next != null)
                archetype.next.prev = archetype.prev;
            archetype.prev = null;
            archetype.next = null;
        }
    }
}
