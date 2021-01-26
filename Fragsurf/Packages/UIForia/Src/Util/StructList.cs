using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UIForia.Util {

    internal class StructListDebugView<T> where T : struct {

        private readonly StructList<T> structList;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] array;

        public StructListDebugView(StructList<T> structList) {
            this.structList = structList;
            array = structList.ToArray();
        }

    }

    [DebuggerDisplay("StructList Count = {" + nameof(size) + "} | Capacity = {array.Length}")]
    [DebuggerTypeProxy(typeof(StructListDebugView<>))]
    public class StructList<T> where T : struct {

        public T[] array;
        public int size;
        private bool isInPool;

        public T[] Array => array;

        public StructList(int capacity = 8) {
            this.size = 0;
            this.array = new T[capacity];
        }

        public StructList(T[] array) {
            this.array = array;
            this.size = array.Length;
        }

        public int Count {
            get { return size; }
            set { size = value; }
        }

        public void Add(in T item) {
            if (size + 1 > array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            array[size] = item;
            size++;
        }

        public void AddUnsafe(in T item) {
            array[size++] = item;
        }

        public void AddRange(T[] collection) {
            if (size + collection.Length >= array.Length) {
                System.Array.Resize(ref array, size + collection.Length * 2);
            }

            if (collection.Length < HandCopyThreshold) {
                int idx = size;
                for (int i = 0; i < collection.Length; i++) {
                    array[idx++] = collection[i];
                }
            }
            else {
                System.Array.Copy(collection, 0, array, size, collection.Length);
            }

            size += collection.Length;
        }

        public void AddRange(IList<T> collection, int resizeFactor = 1) {
            
            if (size + collection.Count >= array.Length) {
                System.Array.Resize(ref array, size + collection.Count * resizeFactor);
            }

            int idx = size;
            for (int i = 0; i < collection.Count; i++) {
                array[idx++] = collection[i];
            }

            size += collection.Count;
        }

        public void AddRange(T[] collection, int start, int count) {
            if (size + count >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            if (count < HandCopyThreshold) {
                int idx = size;
                for (int i = start; i < count; i++) {
                    array[idx++] = collection[i];
                }
            }
            else {
                System.Array.Copy(collection, start, array, size, count);
            }

            size += count;
        }

        public void AddRange(StructList<T> collection) {
            if (size + collection.size >= array.Length) {
                System.Array.Resize(ref array, size + collection.size * 2);
            }

            if (collection.size < HandCopyThreshold) {
                T[] src = collection.array;
                int count = collection.size;
                for (int i = 0; i < count; i++) {
                    array[size + i] = src[i];
                }
            }
            else {
                System.Array.Copy(collection.array, 0, array, size, collection.size);
            }

            size += collection.size;
        }

        private const int HandCopyThreshold = 8;

        public void Reverse() {
            int max = size / 2;
            for (int i = 0; i < max; i++) {
                T tmp = array[i];
                array[i] = array[size - i - 1];
                array[size - i - 1] = tmp;
            }
        }

        public void AddRange(StructList<T> collection, int start, int count) {
            if (size + count >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            if (collection.size < HandCopyThreshold) {
                T[] src = collection.array;
                int idx = size;
                for (int i = 0; i < count; i++) {
                    array[idx++] = src[start + i];
                }
            }
            else {
                System.Array.Copy(collection.array, start, array, size, count);
            }

            size += count;
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length < capacity) {
                System.Array.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (array.Length < size + capacity) {
                System.Array.Resize(ref array, (size + capacity) * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity, int extraSpace) {
            if (array.Length < size + capacity) {
                System.Array.Resize(ref array, (size + capacity) + extraSpace);
            }
        }

        public void QuickClear() {
            size = 0;
        }

        public void Clear() {
            size = 0;
            System.Array.Clear(array, 0, array.Length);
        }

        public T this[int idx] {
            [DebuggerStepThrough] get => array[idx];
            [DebuggerStepThrough] set => array[idx] = value;
        }

        public void SetFromRange(T[] source, int start, int count) {
            if (array.Length <= count) {
                System.Array.Resize(ref array, count * 2);
            }

            System.Array.Copy(source, start, array, 0, count);
            size = count;
        }

        public void ShiftRight(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            EnsureCapacity(startIndex + count + count); // I think this is too big
            System.Array.Copy(array, startIndex, array, startIndex + count, count);
            System.Array.Clear(array, startIndex, count);
            size += count;
        }

        public void ShiftLeft(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            System.Array.Copy(array, startIndex, array, startIndex - count, size - startIndex);
            System.Array.Clear(array, size - count, count);
            size -= count;
        }

        public void IntroSort(IComparer<T> comparison) {
            if (size < 2) return;
            IntroSort(array, 0, size - 1, 2 * FloorLog2(size), comparison);
        }
//
//        public void Sort(int start, int length, IComparer<T> comparison) {
//            if (size < 2) return;
//            IntroSort(array, start, length + start - 1, 2 * FloorLog2(length), comparison);
//        }


        private class Cmp : IComparer<T> {

            public Comparison<T> cmp;

            public int Compare(T x, T y) {
                return cmp.Invoke(x, y);
            }

        }

        private static Cmp s_Comparer = new Cmp();

        public void Sort(IComparer<T> comparison) {
            if (size < 2) return;
            System.Array.Sort(array, 0, size, comparison);
        }

        // NOT FUCKING THREAD SAFE
        public void Sort(Comparison<T> comparison) {
            if (size < 2) return;
            s_Comparer.cmp = comparison;
            System.Array.Sort(array, 0, size, s_Comparer);
            s_Comparer.cmp = null;
        }

        internal static int FloorLog2(int n) {
            int num = 0;
            for (; n >= 1; n /= 2)
                ++num;
            return num;
        }

        public StructList<T> GetRange(int index, int count, StructList<T> retn = null) {
            if (retn == null) {
                retn = GetMinSize(count);
            }
            else {
                retn.EnsureCapacity(count);
            }

            System.Array.Copy(array, index, retn.array, 0, count);
            retn.size = count;
            return retn;
        }

        private static readonly LightList<StructList<T>> s_Pool = new LightList<StructList<T>>();

        public static implicit operator StructList<T>(T[] array) {
            return new StructList<T>(array);
        }

        public static StructList<T> Get() {
            StructList<T> retn = s_Pool.Count > 0 ? s_Pool.RemoveLast() : new StructList<T>();
            retn.isInPool = false;
            return retn;
        }

        public static StructList<T> GetMinSize(int minCapacity) {
            
            if (minCapacity < 1) minCapacity = 4;

            if (s_Pool.size == 0) {
                return new StructList<T>(minCapacity) {isInPool = false};
            }
            
            for (int i = 0; i < s_Pool.size; i++) {
                    
                StructList<T> list = s_Pool.array[i];
                    
                if (list.array.Length < minCapacity) {
                    continue;
                }
                    
                if (s_Pool.size == 1) {
                    s_Pool.array[i] = null;    
                }
                else {
                    s_Pool.array[i] = s_Pool.array[s_Pool.size - 1];
                    s_Pool.array[s_Pool.size - 1] = null;    
                }

                s_Pool.size -= 1;
                        
                list.isInPool = false;
                
                return list;
            }

            return new StructList<T>(minCapacity) {isInPool = false};;
            
        }
        
        public static StructList<T> PreSize(int size) {
            StructList<T> list = GetMinSize(size);
            list.size = size;
            return list;
        }

        private static void SwapIfGreater(T[] keys, IComparer<T> comparer, int a, int b) {
            if (a == b || comparer.Compare(keys[a], keys[b]) <= 0)
                return;
            T key = keys[a];
            keys[a] = keys[b];
            keys[b] = key;
        }

        private static void Swap(T[] a, int i, int j) {
            if (i == j)
                return;
            T obj = a[i];
            a[i] = a[j];
            a[j] = obj;
        }
        
        public void QuickSort(IComparer<T> comparer) {
            QuickSort(array, 0, size - 1, comparer);
        }

        private static void QuickSort(T[] array, int startIndex, int endIndex, IComparer<T> comparer) {
            while (true) {
                int left = startIndex;
                int right = endIndex;
                int pivot = startIndex;
                startIndex++;

                while (endIndex >= startIndex) {
                    
                    int cmpStart_pivot = comparer.Compare(array[startIndex], array[pivot]);
                    int cmpEnd_pivot = comparer.Compare(array[endIndex], array[pivot]);

                    if (cmpStart_pivot >= 0 && cmpEnd_pivot < 0) {
                        T obj = array[startIndex];
                        array[startIndex] = array[endIndex];
                        array[endIndex] = obj;
                    }
                    else if (cmpStart_pivot >= 0) {
                        endIndex--;
                    }
                    else if (cmpEnd_pivot < 0) {
                        startIndex++;
                    }
                    else {
                        endIndex--;
                        startIndex++;
                    }
                }

                T tmp = array[pivot];
                array[pivot] = array[endIndex];
                array[endIndex] = tmp;
                pivot = endIndex;

                if (pivot > left) {
                    QuickSort(array, left, pivot, comparer);
                }

                if (right > pivot + 1) {
                    startIndex = pivot + 1;
                    endIndex = right;
                    continue;
                }

                break;
            }
        }

//         private static void QuickSort(T[] array, int startIndex, int endIndex, IComparer<T> comparer) {
//            while (true) {
//                int left = startIndex;
//                int right = endIndex;
//                int pivot = startIndex;
//                startIndex++;
//
//                while (endIndex >= startIndex) {
//                    int cmpStart = comparer.Compare(array[startIndex], array[pivot]);
//
//                    if (cmpStart >= 0 && comparer.Compare(array[endIndex], array[pivot]) < 0) {
//                        Swap(array, startIndex, endIndex);
//                    }
//                    else if (comparer.Compare(array[startIndex], array[pivot]) >= 0) {
//                        endIndex--;
//                    }
//                    else if (comparer.Compare(array[endIndex], array[pivot]) < 0) {
//                        startIndex++;
//                    }
//                    else {
//                        endIndex--;
//                        startIndex++;
//                    }
//                }
//
//                Swap(array, pivot, endIndex);
//                pivot = endIndex;
//
//                if (pivot > left) {
//                    QuickSort(array, left, pivot, comparer);
//                }
//
//                if (right > pivot + 1) {
//                    startIndex = pivot + 1;
//                    endIndex = right;
//                    continue;
//                }
//
//                break;
//            }
//        }

        private static void IntroSort(T[] keys, int lo, int hi, int depthLimit, IComparer<T> comparer) {
            int num1;
            for (; hi > lo; hi = num1 - 1) {
                int num2 = hi - lo + 1;
                if (num2 <= 16) {
                    if (num2 == 1)
                        break;
                    if (num2 == 2) {
                        SwapIfGreater(keys, comparer, lo, hi);
                        break;
                    }

                    if (num2 == 3) {
                        SwapIfGreater(keys, comparer, lo, hi - 1);
                        SwapIfGreater(keys, comparer, lo, hi);
                        SwapIfGreater(keys, comparer, hi - 1, hi);
                        break;
                    }

                    InsertionSort(keys, lo, hi, comparer);
                    break;
                }

                if (depthLimit == 0) {
                    Heapsort(keys, lo, hi, comparer);
                    break;
                }

                --depthLimit;
                num1 = PickPivotAndPartition(keys, lo, hi, comparer);
                IntroSort(keys, num1 + 1, hi, depthLimit, comparer);
            }
        }

        private static int PickPivotAndPartition(T[] keys, int lo, int hi, IComparer<T> comparer) {
            int index = lo + (hi - lo) / 2;
            SwapIfGreater(keys, comparer, lo, index);
            SwapIfGreater(keys, comparer, lo, hi);
            SwapIfGreater(keys, comparer, index, hi);
            T key = keys[index];
            Swap(keys, index, hi - 1);
            int i = lo;
            int j = hi - 1;
            while (i < j) {
                do { } while (comparer.Compare(keys[++i], key) < 0);

                do { } while (comparer.Compare(key, keys[--j]) < 0);

                if (i < j)
                    Swap(keys, i, j);
                else
                    break;
            }

            Swap(keys, i, hi - 1);
            return i;
        }

        private static void Heapsort(T[] keys, int lo, int hi, IComparer<T> comparer) {
            int n = hi - lo + 1;
            for (int i = n / 2; i >= 1; --i)
                DownHeap(keys, i, n, lo, comparer);
            for (int index = n; index > 1; --index) {
                Swap(keys, lo, lo + index - 1);
                DownHeap(keys, 1, index - 1, lo, comparer);
            }
        }

        private static void DownHeap(T[] keys, int i, int n, int lo, IComparer<T> comparer) {
            T key = keys[lo + i - 1];
            int num;
            for (; i <= n / 2; i = num) {
                num = 2 * i;
                if (num < n && comparer.Compare(keys[lo + num - 1], keys[lo + num]) < 0)
                    ++num;
                if (comparer.Compare(key, keys[lo + num - 1]) < 0)
                    keys[lo + i - 1] = keys[lo + num - 1];
                else
                    break;
            }

            keys[lo + i - 1] = key;
        }

        private static void InsertionSort(T[] keys, int lo, int hi, IComparer<T> comparer) {
            for (int index1 = lo; index1 < hi; ++index1) {
                int index2 = index1;
                T key;
                for (key = keys[index1 + 1]; index2 >= lo && comparer.Compare(key, keys[index2]) < 0; --index2)
                    keys[index2 + 1] = keys[index2];
                keys[index2 + 1] = key;
            }
        }

        public void QuickRelease() {
            size = 0;
            if (isInPool) return;
            isInPool = true;
            s_Pool.Add(this);
        }

        public void Release() {
            Clear();
            if (isInPool) return;
            isInPool = true;
            s_Pool.Add(this);
        }

        public static void Release(ref StructList<T> toPool) {
            toPool.Clear();
            if (toPool.isInPool) return;
            toPool.isInPool = true;
            s_Pool.Add(toPool);
            toPool = null;
        }

        public void Insert(int index, in T item) {
            if (size + 1 >= array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            size++;
            if (index < 0 || index > array.Length) {
                throw new IndexOutOfRangeException();
            }

            System.Array.Copy(array, index, array, index + 1, size - index);
            array[index] = item;
        }

        public T[] ToArray() {
            T[] retn = new T[size];
            System.Array.Copy(array, 0, retn, 0, size);
            return retn;
        }

        public void CopyToArrayUnchecked(T[] target) {
            System.Array.Copy(array, 0, target, 0, size);
        }

        public void RemoveAt(int index) {
            --size;
            System.Array.Copy(array, index + 1, array, index, size - index);
            array[size] = default;
        }
        
        public void RemoveAt(int index, out T retn) {
            --size;
            retn = array[index];
            System.Array.Copy(array, index + 1, array, index, size - index);
            array[size] = default;
        }

        public void SwapRemoveAt(int index) {
            array[index] = array[size - 1];
            array[size - 1] = default;
            size--;
        }

        public T SwapRemoveAtWithValue(int index) {
            T tmp = array[index];
            array[index] = array[size - 1];
            array[size - 1] = default;
            size--;
            return tmp;
        }

        public void InsertRange(int index, StructList<T> items) {
            EnsureAdditionalCapacity(items.size);
            if (index < size) {
                System.Array.Copy(array, index, array, index + items.size, size - index);
            }

            if (this == items) {
                System.Array.Copy(array, 0, array, index, index);
                System.Array.Copy(array, index + items.size, array, index * 2, size - index);
            }
            else {
                System.Array.Copy(items.array, 0, array, size, items.size);
            }
        }

        public StructList<T> Clone() {
            StructList<T> retn = new StructList<T>(size);
            System.Array.Copy(array, 0, retn.array, 0, size);
            retn.size = size;
            return retn;
        }

    }

}