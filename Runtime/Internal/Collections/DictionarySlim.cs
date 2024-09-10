// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Xeno.Collections {
    public class DictionarySlim<TKey, TValue> : IReadOnlyCollection<KeyValuePair<TKey, TValue>> where TKey : IEquatable<TKey> {
        // We want to initialize without allocating arrays. We also want to avoid null checks.
        // Array.Empty would give divide by zero in modulo operation. So we use static one element arrays.
        // The first add will cause a resize replacing these with real arrays of three elements.
        // Arrays are wrapped in a class to avoid being duplicated for each <TKey, TValue>
        private static readonly Entry[] InitialEntries = new Entry[1];
        private readonly IEqualityComparer<TKey> _comparer;
        internal int _count;
        // 0-based index into _entries of head of free chain: -1 means empty
        private int _freeList = -1;
        // 1-based index into _entries; 0 means empty
        private int[] _buckets;
        internal Entry[] _entries;

        internal struct Entry {
            public TKey key;
            public TValue value;
            // 0-based index of next entry in chain: -1 means end of chain
            // also encodes whether this entry _itself_ is part of the free list by changing sign and subtracting 3,
            // so -2 means end of free list, -3 means index 0 but on free list, -4 means index 1 but on free list, etc.
            public int next;
        }

        /// <summary>
        /// Construct with default capacity.
        /// </summary>
        public DictionarySlim() : this(EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// Construct with default capacity and custom comparer
        /// </summary>
        /// <param name="comparer"></param>
        public DictionarySlim(IEqualityComparer<TKey> comparer) {
            _comparer = comparer;
            _buckets = HashHelpers.SizeOneIntArray;
            _entries = InitialEntries;
        }

        /// <summary>
        /// Construct with at least the specified capacity for
        /// entries before resizing must occur.
        /// </summary>
        /// <param name="capacity">Requested minimum capacity</param>
        public DictionarySlim(int capacity) : this(capacity, EqualityComparer<TKey>.Default) { }

        public DictionarySlim(int capacity, IEqualityComparer<TKey> comparer) {
            _comparer = comparer;
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (capacity < 2)
                capacity = 2; // 1 would indicate the dummy array
            capacity = HashHelpers.PowerOf2(capacity);
            _buckets = new int[capacity];
            _entries = new Entry[capacity];
        }

        /// <summary>
        /// Count of entries in the dictionary.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Clears the dictionary. Note that this invalidates any active enumerators.
        /// </summary>
        public void Clear() {
            _count = 0;
            _freeList = -1;
            _buckets = HashHelpers.SizeOneIntArray;
            _entries = InitialEntries;
        }

        /// <summary>
        /// Looks for the specified key in the dictionary.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>true if the key is present, otherwise false</returns>
        public bool ContainsKey(TKey key) {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var entries = _entries;
            var collisionCount = 0;
            for (var i = _buckets[key.GetHashCode() & (_buckets.Length - 1)] - 1; (uint)i < (uint)entries.Length; i = entries[i].next) {
                if (_comparer.Equals(key, entries[i].key)) return true;
                if (collisionCount == entries.Length) {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
                collisionCount++;
            }

            return false;
        }

        /// <summary>
        /// Gets the value if present for the specified key.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <param name="value">Value found, otherwise default(TValue)</param>
        /// <returns>true if the key is present, otherwise false</returns>
        public bool TryGetValue(TKey key, ref TValue value) {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var entries = _entries;
            var collisionCount = 0;
            for (var i = _buckets[key.GetHashCode() & (_buckets.Length - 1)] - 1;
                 (uint)i < (uint)entries.Length; i = entries[i].next) {
                if (_comparer.Equals(key, entries[i].key)) {
                    value = entries[i].value;
                    return true;
                }
                if (collisionCount == entries.Length) {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
                collisionCount++;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Removes the entry if present with the specified key.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>true if the key is present, false if it is not</returns>
        public bool Remove(TKey key) {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var entries = _entries;
            var bucketIndex = key.GetHashCode() & (_buckets.Length - 1);
            var entryIndex = _buckets[bucketIndex] - 1;

            var lastIndex = -1;
            var collisionCount = 0;
            while (entryIndex != -1) {
                var candidate = entries[entryIndex];
                if (_comparer.Equals(key, candidate.key)) {
                    if (lastIndex != -1) { // Fixup preceding element in chain to point to next (if any)
                        entries[lastIndex].next = candidate.next;
                    } else { // Fixup bucket to new head (if any)
                        _buckets[bucketIndex] = candidate.next + 1;
                    }

                    entries[entryIndex] = default;

                    entries[entryIndex].next = -3 - _freeList; // New head of free list
                    _freeList = entryIndex;

                    _count--;
                    return true;
                }
                lastIndex = entryIndex;
                entryIndex = candidate.next;

                if (collisionCount == entries.Length) {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
                collisionCount++;
            }

            return false;
        }

        // Not safe for concurrent _reads_ (at least, if either of them add)
        // For concurrent reads, prefer TryGetValue(key, out value)
        /// <summary>
        /// Gets the value for the specified key, or, if the key is not present,
        /// adds an entry and returns the value by ref. This makes it possible to
        /// add or update a value in a single look up operation.
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>Reference to the new or existing value</returns>
        public ref TValue GetOrAddValueRef(TKey key, out bool @new) {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var entries = _entries;
            var collisionCount = 0;
            var bucketIndex = key.GetHashCode() & (_buckets.Length - 1);
            @new = false;
            for (var i = _buckets[bucketIndex] - 1;
                 (uint)i < (uint)entries.Length; i = entries[i].next) {
                if (_comparer.Equals(key, entries[i].key))
                    return ref entries[i].value;
                if (collisionCount == entries.Length) {
                    // The chain of entries forms a loop; which means a concurrent update has happened.
                    // Break out of the loop and throw, rather than looping forever.
                    throw new InvalidOperationException("ConcurrentOperationsNotSupported");
                }
                collisionCount++;
            }

            @new = true;
            return ref AddKey(key, bucketIndex);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ref TValue AddKey(TKey key, int bucketIndex) {
            var entries = _entries;
            int entryIndex;
            if (_freeList != -1) {
                entryIndex = _freeList;
                _freeList = -3 - entries[_freeList].next;
            } else {
                if (_count == entries.Length || entries.Length == 1) {
                    entries = Resize();
                    bucketIndex = key.GetHashCode() & (_buckets.Length - 1);
                    // entry indexes were not changed by Resize
                }
                entryIndex = _count;
            }

            entries[entryIndex].key = key;
            entries[entryIndex].next = _buckets[bucketIndex] - 1;
            _buckets[bucketIndex] = entryIndex + 1;
            _count++;
            return ref entries[entryIndex].value;
        }

        private Entry[] Resize() {
            Debug.Assert(_entries.Length == _count || _entries.Length == 1); // We only copy _count, so if it's longer we will miss some
            var count = _count;
            var newSize = _entries.Length * 2;
            if ((uint)newSize > int.MaxValue) // uint cast handles overflow
                throw new InvalidOperationException("HTCapacityOverflow");

            var entries = new Entry[newSize];
            Array.Copy(_entries, 0, entries, 0, count);

            var newBuckets = new int[entries.Length];
            while (count-- > 0) {
                var bucketIndex = entries[count].key.GetHashCode() & (newBuckets.Length - 1);
                entries[count].next = newBuckets[bucketIndex] - 1;
                newBuckets[bucketIndex] = count + 1;
            }

            _buckets = newBuckets;
            _entries = entries;

            return entries;
        }

        /// <summary>
        /// Gets an enumerator over the dictionary
        /// </summary>
        public Enumerator GetEnumerator() => new Enumerator(this); // avoid boxing

        /// <summary>
        /// Gets an enumerator over the dictionary
        /// </summary>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() =>
            new Enumerator(this);

        /// <summary>
        /// Gets an enumerator over the dictionary
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

        /// <summary>
        /// Enumerator
        /// </summary>
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>> {
            private readonly DictionarySlim<TKey, TValue> _dictionary;
            private int _index;
            private int _count;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(DictionarySlim<TKey, TValue> dictionary) {
                _dictionary = dictionary;
                _index = 0;
                _count = _dictionary._count;
                _current = default;
            }

            /// <summary>
            /// Move to next
            /// </summary>
            public bool MoveNext() {
                if (_count == 0) {
                    _current = default;
                    return false;
                }

                _count--;

                while (_dictionary._entries[_index].next < -1)
                    _index++;

                _current = new KeyValuePair<TKey, TValue>(
                    _dictionary._entries[_index].key,
                    _dictionary._entries[_index++].value);
                return true;
            }

            /// <summary>
            /// Get current value
            /// </summary>
            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            void IEnumerator.Reset() {
                _index = 0;
                _count = _dictionary._count;
                _current = default;
            }

            /// <summary>
            /// Dispose the enumerator
            /// </summary>
            public void Dispose() {}
        }
    }
}