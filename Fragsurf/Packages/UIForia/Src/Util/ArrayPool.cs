using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIForia.Util {

    public static class ArrayPool<T> {

        private static readonly List<T[]> s_ArrayPool = new List<T[]>();

        // ReSharper disable once StaticMemberInGenericType
        private static int MaxPoolSize = 16;

        public static T[] Empty { get; } = new T[0];

        public static void SetMaxPoolSize(int poolSize) {
            MaxPoolSize = poolSize;
        }

        public static T[] GetMinSize(int minSize) {
            minSize = Mathf.Max(0, minSize);
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length >= minSize) {
                    T[] retn = s_ArrayPool[i];
                    s_ArrayPool.RemoveAt(i);
                    return retn;
                }
            }

            return new T[minSize];
        }

        public static T[] GetExactSize(int size) {
            size = Mathf.Max(0, size);
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length == size) {
                    T[] retn = s_ArrayPool[i];
                    s_ArrayPool.RemoveAt(i);
                    return retn;
                }
            }

            return new T[size];
        }

        public static void Resize(ref T[] array, int minSize) {
            minSize = Mathf.Max(0, minSize);
            T[] retn = null;
            
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length >= minSize) {
                    retn = s_ArrayPool[i];
                    s_ArrayPool[i] = array;
                    Array.Copy(array, 0, retn, 0, array.Length);
                    Array.Clear(array, 0, array.Length);
                    array = retn;
                    return;
                }
            }

            retn = new T[minSize];
            
            Array.Copy(array, 0, retn, 0, array.Length);

            if (s_ArrayPool.Count < 8) {
                Array.Clear(array, 0, array.Length);
                s_ArrayPool.Add(array);
            }
            else {
                int minLengthIndex = 0;
                int minLength = 0;
                
                for (int i = 0; i < s_ArrayPool.Count; i++) {
                    if (s_ArrayPool[i].Length < minLength) {
                        minLength = s_ArrayPool[i].Length;
                        minLength = i;
                    }
                }

                s_ArrayPool[minLengthIndex] = array;

            }

            array = retn;
        }

        public static void Release(ref T[] array) {
            if (array == null || array.Length == 0) return;
            Array.Clear(array, 0, array.Length);
            if (s_ArrayPool.Count == MaxPoolSize) {
                int minCount = int.MaxValue;
                int minIndex = 0;
                for (int i = 0; i < s_ArrayPool.Count; i++) {
                    if (s_ArrayPool[i].Length < minCount) {
                        minCount = s_ArrayPool[i].Length;
                        minIndex = i;
                    }
                }

                if (array.Length > minCount) {
                    s_ArrayPool[minIndex] = array;
                }
            }
            else {
                if (s_ArrayPool.Contains(array)) {
                    return;
                }

                s_ArrayPool.Add(array);
            }

            array = null;
        }

        public static T[] CopyFromList(IList<T> source) {
            T[] retn = GetMinSize(source.Count);
            for (int i = 0; i < source.Count; i++) {
                retn[i] = source[i];
            }

            return retn;
        }

        public static T[] Copy(T[] other) {
            T[] retn = GetExactSize(other.Length);
            for (int i = 0; i < retn.Length; i++) {
                retn[i] = other[i];
            }

            return retn;
        }

    }

}