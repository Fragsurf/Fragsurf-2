using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace UIForia.Extensions {

    public struct ListAccessor<T> {

        private static readonly Func<List<T>, T[]> arrayGetter;
        private static readonly Func<List<T>, T[], T[]> arraySetter;
        private static readonly Func<List<T>, int> sizeGetter;
        private static readonly Func<List<T>, int, int> sizeSetter;

        static ListAccessor() {
            arrayGetter = (Func<List<T>, T[]>) CreateFieldGetter("_items");
            arraySetter = (Func<List<T>, T[], T[]>) CreateFieldSetter(typeof(T[]), "_items");
            sizeGetter = (Func<List<T>, int>) CreateFieldGetter("_size");
            sizeSetter = (Func<List<T>, int, int>) CreateFieldSetter(typeof(int), "_size");
        }

        public static void SetArray(List<T> list, T[] array, int count) {
            if (count < 0) count = array.Length;
            arraySetter(list, array);
            sizeSetter(list, count);
        }

        public static void SetArray(List<T> list, T[] array) {
            arraySetter(list, array);
            sizeSetter(list, array.Length);
        }

        public static T[] GetArray(List<T> list) {
            return arrayGetter(list);
        }

        public static void SetCount(List<T> list, int count) {
            sizeSetter(list, count);
        }

        public static int GetSize(List<T> list) {
            return sizeGetter(list);
        }

        private static Delegate CreateFieldGetter(string fieldName) {
            ParameterExpression paramExpression = Expression.Parameter(typeof(List<T>));
            Expression fieldGetterExpression = Expression.Field(paramExpression, fieldName);
            return Expression.Lambda(fieldGetterExpression, paramExpression).Compile();
        }

        private static Delegate CreateFieldSetter(Type fieldType, string fieldName) {
            ParameterExpression paramExpression0 = Expression.Parameter(typeof(List<T>));
            ParameterExpression paramExpression1 = Expression.Parameter(fieldType, fieldName);
            MemberExpression fieldGetter = Expression.Field(paramExpression0, fieldName);

            return Expression.Lambda(
                Expression.Assign(fieldGetter, paramExpression1),
                paramExpression0,
                paramExpression1
            ).Compile();
        }

    }

    public static class ListExtensions {

        public static void SetArray<T>(this List<T> list, T[] array, int count) {
            ListAccessor<T>.SetArray(list, array, count);
        }

        public static T[] GetArray<T>(this List<T> list) {
            return ListAccessor<T>.GetArray(list);
        }

        public static void SetCount<T>(this List<T> list, int count) {
            ListAccessor<T>.SetCount(list, count);
        }
        
        public static T UnstableRemove<T>(this List<T> list, int index) {
            T value = list[list.Count - 1];
            list[index] = value;
            T retn = list[list.Count - 1];
            list.RemoveAt(list.Count - 1);
            return retn;
        }

        [DebuggerStepThrough]
        public static void EnsureCapacity<T>(this List<T> list, int minCapacity, T c = default(T)) {
            if (list.Capacity <= minCapacity) {
                Resize(list, minCapacity, c);
            }
        }

        [DebuggerStepThrough]
        public static T Find<T, U>(this List<T> list, U target, Func<T, U, bool> predicate) {
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i], target)) {
                    return list[i];
                }
            }

            return default(T);
        }

        public static int FindIndex<T, U>(this List<T> list, U target, Func<T, U, bool> predicate) {
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i], target)) {
                    return i;
                }
            }

            return -1;
        }

        public static T Find<T>(this List<T> list, T target) where T : class {
            for (int i = 0; i < list.Count; i++) {
                if ((target == list[i])) {
                    return list[i];
                }
            }

            return null;
        }

        [DebuggerStepThrough]
        public static T FindOrDefault<T>(this List<T> list, T defaultValue, Predicate<T> predicate) {
            int resultIndex = list.FindIndex(predicate);
            if (resultIndex == -1) return defaultValue;
            return list[resultIndex];
        }

        [DebuggerStepThrough]
        public static int FindByIndex<T, U>(this List<T> list, U target, Func<T, U, bool> predicate) {
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i], target)) {
                    return i;
                }
            }

            return -1;
        }

        [DebuggerStepThrough]
        public static List<T> FindAll<T, U>(this List<T> list, U target, Func<T, U, bool> predicate) {
            List<T> retn = new List<T>(4);
            for (int i = 0; i < list.Count; i++) {
                if (predicate(list[i], target)) {
                    retn.Add(list[i]);
                }
            }

            return retn;
        }

        [DebuggerStepThrough]
        public static List<U> Map<T, U>(this List<T> list, Func<T, U> mapFn) {
            List<U> retn = new List<U>(list.Count);
            for (int i = 0; i < list.Count; i++) {
                retn.Add(mapFn(list[i]));
            }

            return retn;
        }

        [DebuggerStepThrough]
        public static List<V> Map<T, U, V>(this List<T> list, U target, Func<T, U, V> mapFn) {
            List<V> retn = new List<V>(list.Count);
            for (int i = 0; i < list.Count; i++) {
                retn.Add(mapFn(list[i], target));
            }

            return retn;
        }

        [DebuggerStepThrough]
        public static U[] MapArray<T, U>(this List<T> list, Func<T, U> mapFn) {
            U[] retn = new U[list.Count];
            for (int i = 0; i < list.Count; i++) {
                retn[i] = mapFn(list[i]);
            }

            return retn;
        }

        [DebuggerStepThrough]
        public static V[] MapArray<T, U, V>(this List<T> list, U target, Func<T, U, V> mapFn) {
            V[] retn = new V[list.Count];
            for (int i = 0; i < list.Count; i++) {
                retn[i] = mapFn(list[i], target);
            }

            return retn;
        }

        [DebuggerStepThrough]
        public static bool Contains<T, U>(this List<T> list, U value, Func<T, U, bool> predicate) {
            return list.FindByIndex(value, predicate) != -1;
        }

        [DebuggerStepThrough]
        public static void Resize<T>(this List<T> list, int sz, T c = default(T)) {
            int cur = list.Count;
            if (sz < cur)
                list.RemoveRange(sz, cur - sz);
            else if (sz > cur)
                list.AddRange(Enumerable.Repeat(c, sz - cur));
        }

        /// <summary>
        /// Moves an item within this list to another index, shifting other items as needed.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="oldIndex"></param>
        /// <param name="insertIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool MoveToIndex<T>(this List<T> list, int oldIndex, int insertIndex) {
            if (insertIndex == -1) {
                insertIndex = list.Count - 1;
            }

            if ((uint) oldIndex >= list.Count) return false;
            if (insertIndex > oldIndex) insertIndex--;
            if ((uint) insertIndex >= list.Count) return false;
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(insertIndex, item);
            return true;
        }

        /// <summary>
        /// Moves an item within this list to another index, shifting other items as needed.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        /// <param name="insertIndex"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        public static bool MoveToIndex<T>(this List<T> list, T item, int insertIndex) {
            return MoveToIndex(list, list.IndexOf(item), insertIndex);
        }

        [DebuggerStepThrough]
        public static T First<T>(this List<T> list) {
            return list.Count > 0 ? list[0] : default(T);
        }

        [DebuggerStepThrough]
        public static T Last<T>(this List<T> list) {
            return list.Count > 0 ? list[list.Count - 1] : default(T);
        }

        /// <summary>
        /// Insert a value into an IList{T} that is presumed to be already sorted such that sort
        /// ordering is preserved
        /// </summary>
        /// <param name="list">List to insert into</param>
        /// <param name="value">Value to insert</param>
        /// <typeparam name="T">Type of element to insert and type of elements in the list</typeparam>
        [DebuggerStepThrough]
        public static void InsertIntoSortedList<T>(this IList<T> list, T value)
            where T : IComparable<T> {
            InsertIntoSortedList(list, value, (a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// Insert a value into an IList{T} that is presumed to be already sorted such that sort
        /// ordering is preserved
        /// </summary>
        /// <param name="list">List to insert into</param>
        /// <param name="value">Value to insert</param>
        /// <param name="comparison">Comparison to determine sort order with</param>
        /// <typeparam name="T">Type of element to insert and type of elements in the list</typeparam>
        [DebuggerStepThrough]
        public static void InsertIntoSortedList<T>(this IList<T> list, T value, Comparison<T> comparison) {
            int startIndex = 0;
            int endIndex = list.Count;
            while (endIndex > startIndex) {
                int windowSize = endIndex - startIndex;
                int middleIndex = startIndex + (windowSize / 2);
                T middleValue = list[middleIndex];
                int compareToResult = comparison(middleValue, value);
                if (compareToResult == 0) {
                    list.Insert(middleIndex, value);
                    return;
                }
                else if (compareToResult < 0) {
                    startIndex = middleIndex + 1;
                }
                else {
                    endIndex = middleIndex;
                }
            }

            list.Insert(startIndex, value);
        }

        public static void AddOrInsert<T>(this List<T> list, T item, int index) {
            if (index == -1) {
                list.Add(item);
            }
            else {
                list.Insert(index, item);
            }
        }

        /// <summary>
        /// Insert a value into an IList that is presumed to be already sorted such that sort ordering is preserved
        /// </summary>
        /// <param name="list">List to insert into</param>
        /// <param name="value">Value to insert</param>
        [DebuggerStepThrough]
        public static void InsertIntoSortedList(this IList list, IComparable value) {
            InsertIntoSortedList(list, value, (a, b) => a.CompareTo(b));
        }

        /// <summary>
        /// Insert a value into an IList that is presumed to be already sorted such that sort ordering is preserved
        /// </summary>
        /// <param name="list">List to insert into</param>
        /// <param name="value">Value to insert</param>
        /// <param name="comparison">Comparison to determine sort order with</param>
        [DebuggerStepThrough]
        public static void InsertIntoSortedList(this IList list, IComparable value, Comparison<IComparable> comparison) {
            int startIndex = 0;
            int endIndex = list.Count;
            while (endIndex > startIndex) {
                int windowSize = endIndex - startIndex;
                int middleIndex = startIndex + (windowSize / 2);
                IComparable middleValue = (IComparable) list[middleIndex];
                int compareToResult = comparison(middleValue, value);
                if (compareToResult == 0) {
                    list.Insert(middleIndex, value);
                    return;
                }
                else if (compareToResult < 0) {
                    startIndex = middleIndex + 1;
                }
                else {
                    endIndex = middleIndex;
                }
            }

            list.Insert(startIndex, value);
        }

        public static T RemoveAndReturn<T>(this List<T> list, T item) {
            int index = list.IndexOf(item);
            if (index == -1) return default(T);
            T retn = list[index];
            list.RemoveAt(index);
            return retn;
        }

        public static T RemoveAndReturnAtIndex<T>(this List<T> list, int index) {
            if (index < 0 || index >= list.Count) return default(T);
            T retn = list[index];
            list.RemoveAt(index);
            return retn;
        }

    }

}