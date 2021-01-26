using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UIForia.Util {
    
    public class IntMap<T> {

        private int[] buckets;
        private Entry[] entries;
        private int count;
        private int freeList;
        private int freeCount;
        private int capacity;

        public IntMap() : this(7) { }

        public IntMap(int capacity) {
            int size = HashHelpers.GetPrime(capacity);
            this.capacity = size;
            buckets = new int[size];
            entries = new Entry[size];
            for (int i = 0; i < size; i++) {
                buckets[i] = -1;
            }
            freeList = -1;
        }

        public IntMap(IDictionary<int, T> collection) : this(7) {
            foreach (KeyValuePair<int,T> pair in collection) {
                Insert(pair.Key, pair.Value, true);
            }
        }

        public int Count => count - freeCount;

        [DebuggerStepThrough]
        public void Add(int key, T value) {
            Insert(key, value, true);
        }

        [DebuggerStepThrough]
        public bool TryGetValue(int key, out T value) {
            int i = FindEntry(key);
            if (i >= 0) {
                value = entries[i].value;
                return true;
            }
            value = default(T);
            return false;
        }

        [DebuggerStepThrough]
        public bool Remove(int key) {

            int hashCode = key & 0x7FFFFFFF;
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
        
        public bool Remove(int key, out T retn) {

            int hashCode = key & 0x7FFFFFFF;
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

        public T this[int key] {
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
        public T GetOrDefault(int key, T defaultValue = default(T)) {
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
        public bool ContainsKey(int key) {
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
                    int bucket = newEntries[i].key % capacity;
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
        private int FindEntry(int key) {
            if (count == 0) return -1;
            
            int hashCode = key & 0x7FFFFFFF;
            for (int i = buckets[hashCode % capacity]; i >= 0; i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    return i;
                }
            }
            
            return -1;
        }

        [DebuggerStepThrough]
        private void Insert(int key, T value, bool add) {

            int hashCode = key & 0x7FFFFFFF;
            int targetBucket = hashCode % capacity;

            for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next) {
                if (entries[i].hashCode == hashCode && entries[i].key == key) {
                    if (add) {
                        throw new Exception("Duplicate key in IntMap: " + key);
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
        public int CopyKeyValuesToArray(ref KeyValuePair<int, T>[] array, int index = 0) {
            if (array == null) {
                array = ArrayPool<KeyValuePair<int, T>>.GetMinSize(Count);
            }  
            if (index < 0) {
                index = 0;
            }
            
            if (index + Count > array.Length ) {
                ArrayPool<KeyValuePair<int, T>>.Resize(ref array, index + Count);
            }
           
            // count not Count -> we don't know if there are holes in the array
            // hashcode will be < 0 if empty
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = new KeyValuePair<int, T>(entries[i].key, entries[i].value);
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
        
        private int CopyTo(ref KeyValuePair<int, T>[] array, int index = 0) {
            if (array == null) {
                array = ArrayPool<KeyValuePair<int, T>>.GetExactSize(Count);
            }
            
            if (index < 0) {
                index = 0;
            }
            
            if (index + Count > array.Length ) {
                ArrayPool<KeyValuePair<int, T>>.Resize(ref array, index + Count);
            }
           
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    array[index++] = new KeyValuePair<int, T>(entries[i].key, entries[i].value);
                }
            }
            return index + Count;
        }

        private struct Entry {

            public int next;
            public int key;
            public T value;
            public int hashCode;

        }

        public void ForEach(Action<int, T> action) {
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    action.Invoke(entries[i].key, entries[i].value);
                }
            }
        }
        
        public void ForEach<U>(U closureArg, Action<int, T, U> action) {
            for (int i = 0; i < count; i++) {
                if (entries[i].hashCode >= 0) {
                    action.Invoke(entries[i].key, entries[i].value, closureArg);
                }
            }
        }
    }

    internal static class HashHelpers {

        private const int MaxPrimeArrayLength = 0x7FEFFFFD;

        public static int ExpandPrime(int oldSize) {
            int newSize = 2 * oldSize;

            if ((uint) newSize > MaxPrimeArrayLength && MaxPrimeArrayLength > oldSize) {
                return MaxPrimeArrayLength;
            }

            return GetPrime(newSize);
        }

        public static int GetPrime(int min) {
            for (int index = 0; index < s_Primes.Length; ++index) {
                int prime = s_Primes[index];
                if (prime >= min) {
                    return prime;
                }
            }
            int candidate = min | 1;
            while (candidate < int.MaxValue) {
                if (IsPrime(candidate) && (candidate - 1) % 101 != 0) {
                    return candidate;
                }
                candidate += 2;
            }
            return min;
        }

        private static bool IsPrime(int candidate) {
            if ((candidate & 1) != 0) {
                int limit = (int) Math.Sqrt(candidate);
                for (int divisor = 3; divisor <= limit; divisor += 2) {
                    if ((candidate % divisor) == 0) {
                        return false;
                    }
                }
                return true;
            }
            return candidate == 2;
        }

        private static readonly int[] s_Primes = new int[72] {
            3,
            7,
            11,
            17,
            23,
            29,
            37,
            47,
            59,
            71,
            89,
            107,
            131,
            163,
            197,
            239,
            293,
            353,
            431,
            521,
            631,
            761,
            919,
            1103,
            1327,
            1597,
            1931,
            2333,
            2801,
            3371,
            4049,
            4861,
            5839,
            7013,
            8419,
            10103,
            12143,
            14591,
            17519,
            21023,
            25229,
            30293,
            36353,
            43627,
            52361,
            62851,
            75431,
            90523,
            108631,
            130363,
            156437,
            187751,
            225307,
            270371,
            324449,
            389357,
            467237,
            560689,
            672827,
            807403,
            968897,
            1162687,
            1395263,
            1674319,
            2009191,
            2411033,
            2893249,
            3471899,
            4166287,
            4999559,
            5999471,
            7199369
        };

    }

}