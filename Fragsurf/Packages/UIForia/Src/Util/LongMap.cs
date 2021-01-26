using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UIForia.Util {
    public class LongMap<T> {

        private int[] buckets;
        private Entry[] entries;
        private int count;
        private int freeList;
        private int freeCount;
        private int capacity;

        public LongMap() : this(7) { }

        public LongMap(int capacity) {
            int size = HashHelpers.GetPrime(capacity);
            this.capacity = size;
            buckets = ArrayPool<int>.GetMinSize(size);
            entries = ArrayPool<Entry>.GetMinSize(size);
            for (int i = 0; i < capacity; i++) {
                buckets[i] = -1;
            }
            freeList = -1;
        }

        public int Count => count - freeCount;

        [DebuggerStepThrough]
        public void Add(long key, T value) {
            Insert(key, value, true);
        }

        [DebuggerStepThrough]
        public bool TryGetValue(long key, out T value) {
            int i = FindEntry(key);
            if (i >= 0) {
                value = entries[i].value;
                return true;
            }
            value = default(T);
            return false;
        }

        [DebuggerStepThrough]
        public bool Remove(long key) {

            int hashCode = (int) key & 0x7FFFFFFF;
            int bucket = hashCode % capacity;
            int last = -1;
            for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    if (last < 0) {
                        buckets[bucket] = entries[i].next;
                    }
                    else {
                        entries[last].next = entries[i].next;
                    }
                    entries[i].hashCode = -1;
                    entries[i].next = freeList;
                    entries[i].key = 0;
                    entries[i].value = default(T);
                    freeList = i;
                    freeCount++;
                    return true;
                }
            }

            return false;
        }
        
        public bool Remove(long key, out T retn) {

            int hashCode = (int) key & 0x7FFFFFFF;
            int bucket = hashCode % capacity;
            int last = -1;
            for (int i = buckets[bucket]; i >= 0; last = i, i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    if (last < 0) {
                        buckets[bucket] = entries[i].next;
                    }
                    else {
                        entries[last].next = entries[i].next;
                    }

                    retn = entries[i].value;
                    entries[i].hashCode = -1;
                    entries[i].next = freeList;
                    entries[i].key = 0;
                    entries[i].value = default(T);
                    freeList = i;
                    freeCount++;
                    return true;
                }
            }

            retn = default;
            return false;
        }

        public T this[long key] {
            [DebuggerStepThrough]
            get {
                int i = FindEntry(key);
                if (i >= 0) {
                    return entries[i].value;
                }
                return default(T);
            }
            [DebuggerStepThrough]
            set { Insert(key, value, false); }
        }

        [DebuggerStepThrough]
        public T GetOrDefault(long key, T defaultValue = default(T)) {
            int i = FindEntry(key);
            if (i >= 0) {
                return entries[i].value;
            }
            return defaultValue;
        }

        [DebuggerStepThrough]
        public void Clear() {
            if (count > 0) {
                for (int i = 0; i < capacity; i++) {
                    buckets[i] = -1;
                }
                Array.Clear(entries, 0, count);
                freeList = -1;
                count = 0;
                freeCount = 0;
            }
        }

        [DebuggerStepThrough]
        public bool ContainsKey(long key) {
            return FindEntry(key) >= 0;
        }

        private void Resize() {
            capacity = HashHelpers.ExpandPrime(count);
            int[] newBuckets = ArrayPool<int>.GetMinSize(capacity);
            for (int i = 0; i < capacity; i++) {
                newBuckets[i] = -1;
            }
            Entry[] newEntries = ArrayPool<Entry>.GetMinSize(capacity);
            Array.Copy(entries, 0, newEntries, 0, count);
            for (int i = 0; i < count; i++) {
                if (newEntries[i].key >= 0) {
                    int bucket = (int) newEntries[i].key % capacity;
                    newEntries[i].next = newBuckets[bucket];
                    newBuckets[bucket] = i;
                }
            }
            ArrayPool<Entry>.Release(ref entries);
            ArrayPool<int>.Release(ref buckets);
            buckets = newBuckets;
            entries = newEntries;
        }

        [DebuggerStepThrough]
        private int FindEntry(long key) {
            long hashCode = key & 0x7FFFFFFF;
            for (int i = buckets[hashCode % capacity]; i >= 0; i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    return i;
                }
            }
            return -1;
        }

        [DebuggerStepThrough]
        private void Insert(long key, T value, bool add) {

            int hashCode = key.GetHashCode() & 0x7FFFFFFF;
            long targetBucket = hashCode % capacity;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    if (add) {
                        throw new Exception("Duplicate key in LongMap: " + key);
                    }
                    entries[i].value = value;
                    return;
                }
            }
            int index;
            if (freeCount > 0) {
                index = freeList;
                freeList = entries[index].next;
                freeCount--;
            }
            else {
                if (count == capacity) {
                    Resize();
                    targetBucket = hashCode % capacity;
                }
                index = count;
                count++;
            }

            entries[index].hashCode = hashCode;
            entries[index].next = buckets[targetBucket];
            entries[index].key = key;
            entries[index].value = value;
            buckets[targetBucket] = index;
        }

        [DebuggerStepThrough]
        public int CopyKeyValuesToArray(ref KeyValuePair<long, T>[] array, int index = 0) {
            if (array == null) {
                array = ArrayPool<KeyValuePair<long, T>>.GetMinSize(Count);
            }  
            if (index < 0) {
                index = 0;
            }
            
            if (index + Count > array.Length ) {
                ArrayPool<KeyValuePair<long, T>>.Resize(ref array, index + Count);
            }
           
            // count not Count -> we don't know if there are holes in the array
            // hashcode will be < 0 if empty
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = new KeyValuePair<long, T>(entries[i].key, entries[i].value);
                }
            }
            
            return index + Count;
        }
        
        [DebuggerStepThrough]
        public int CopyValuesToArray(ref T[] array, int index = 0) {
            if (array == null) {
                array = ArrayPool<T>.GetMinSize(Count);
            }  
            if (index < 0) {
                index = 0;
            }
            
            if (index + Count > array.Length ) {
                ArrayPool<T>.Resize(ref array, index + Count);
            }
           
            // count not Count -> we don't know if there are holes in the array
            // hashcode will be < 0 if empty
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = entries[i].value;
                }
            }
            
            return index + Count;
        }
        
        private int CopyTo(ref KeyValuePair<long, T>[] array, int index = 0) {
            if (array == null) {
                array = ArrayPool<KeyValuePair<long, T>>.GetExactSize(Count);
            }
            
            if (index < 0) {
                index = 0;
            }
            
            if (index + Count > array.Length ) {
                ArrayPool<KeyValuePair<long, T>>.Resize(ref array, index + Count);
            }
           
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = new KeyValuePair<long, T>(entries[i].key, entries[i].value);
                }
            }
            return index + Count;
        }

        private struct Entry {

            public int next;
            public long key;
            public T value;
            public int hashCode;

        }

        public void ForEach(Action<long, T> action) {
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    action.Invoke(entries[i].key, entries[i].value);
                }
            }
        }
        
        public void ForEach<U>(U closureArg, Action<long, T, U> action) {
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    action.Invoke(entries[i].key, entries[i].value, closureArg);
                }
            }
        }
    }

    

}