using System;
using System.Diagnostics;
using UIForia.Compilers;
using UnityEngine;

namespace UIForia.Util {

    public class LightStack<T> {

        public T[] array;
        public int size;
        private bool isPooled;

        public int Count => size;
        public T[] Stack => array;

        public T Previous => PeekAtUnchecked(size - 1);

        [ThreadStatic] private static LightList<LightStack<T>> s_Pool;

        [DebuggerStepThrough]
        public LightStack(int capacity = 8) {
            capacity = Mathf.Max(1, capacity);
            this.array = new T[capacity];
            this.size = 0;
            this.isPooled = false;
        }

        [DebuggerStepThrough]
        public void EnsureCapacity(int capacity) {
            if (array.Length <= capacity) {
                Array.Resize(ref array, capacity * 2);
            }
        }

        [DebuggerStepThrough]
        public void EnsureAdditionalCapacity(int capacity) {
            if (size + capacity >= array.Length) {
                Array.Resize(ref array, size + capacity);
            }
        }

        [DebuggerStepThrough]
        public void Push(T item) {
            if (size + 1 >= array.Length) {
                Array.Resize(ref array, (size + 1) * 2);
            }

            array[size++] = item;
        }

        [DebuggerStepThrough]
        public T PeekAtUnchecked(int idx) {
            return array[idx];
        }

        [DebuggerStepThrough]
        public T PeekRelativeUnchecked(int cnt) {
            return array[size - cnt];
        }

        [DebuggerStepThrough]
        public T Pop() {
            if (size == 0) return default;
            T obj = array[--size];
            array[size] = default;
            return obj;
        }

        [DebuggerStepThrough]
        public void PushUnchecked(T item) {
            array[size++] = item;
        }

        [DebuggerStepThrough]
        public T PopUnchecked() {
            T obj = array[--size];
            array[size] = default;
            return obj;
        }

        [DebuggerStepThrough]
        public void Clear() {
            Array.Clear(array, 0, size);
            size = 0;
        }


        [DebuggerStepThrough]
        public static LightStack<T> Get() {
            s_Pool = s_Pool ?? new LightList<LightStack<T>>(4);
            LightStack<T> retn = s_Pool.Count > 0 ? s_Pool.RemoveLast() : new LightStack<T>();
            retn.isPooled = false;
            return retn;
        }

        public void Release() {
            Array.Clear(array, 0, size);
            size = 0;
            if (isPooled) return;
            isPooled = true;
            s_Pool = s_Pool ?? new LightList<LightStack<T>>(4);
            s_Pool.Add(this);
        }

        [DebuggerStepThrough]
        public static void Release(ref LightStack<T> toPool) {
            Array.Clear(toPool.array, 0, toPool.size);
            toPool.size = 0;
            if (toPool.isPooled) return;
            toPool.isPooled = true;
            s_Pool = s_Pool ?? new LightList<LightStack<T>>(4);
            s_Pool.Add(toPool);
        }

        [DebuggerStepThrough]
        public T Peek() {
            if (size > 0) {
                return array[size - 1];
            }

            return default;
        }

        [DebuggerStepThrough]
        public T PeekUnchecked() {
            return array[size - 1];
        }

        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (ReferenceEquals(array[i], item)) {
                    return true;
                }
            }

            return false;
        }

        public LightStack<T> Clone(LightStack<T> cloneTarget = null) {
            cloneTarget = cloneTarget ?? new LightStack<T>(size);
            cloneTarget.EnsureCapacity(size);
            Array.Copy(array, 0, cloneTarget.array, 0, size);
            cloneTarget.size = size;
            return cloneTarget;
        }

        public T[] ToArray() {
            T[] cloneTarget = new T[size];
            Array.Copy(array, 0, cloneTarget, 0, size);
            return cloneTarget;
        }

        public void RemoveWhere<U>(U closureData, Func<U, T, bool> callback) {
            for (int i = 0; i < size; i++) {
                if (callback(closureData, array[i])) {
                    RemoveAt(i);
                    return;
                }
            }
        }
        
        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size) return;
            if (index == size - 1) {
                array[--size] = default;
            }
            else {
                for (int j = index; j < size - 1; j++) {
                    array[j] = array[j + 1];
                }

                array[--size] = default(T);
            }
        }

    }

}