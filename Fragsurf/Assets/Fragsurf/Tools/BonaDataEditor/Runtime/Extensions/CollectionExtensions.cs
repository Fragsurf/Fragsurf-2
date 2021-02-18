using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fyrvall.DataEditor
{
    public static class CollectionExtensions
    {
        public const int INVALID_INDEX = -1;

        public static void Copy<T>(this T[] source, T[] destination, int startIndex, int endIndex)
        {
            for (int i = startIndex; i < endIndex; i++) {
                destination[i] = source[i];
            }
        }

        public static void AddRange<T>(this ICollection<T> collection, ICollection<T> other)
        {
            if (collection == null || other == null) {
                return;
            }

            foreach (var item in other) {
                collection.Add(item);
            }
        }

        public static T Add<T>(this T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            array = list.ToArray();
            return item;
        }

        public static void RemoveRange<T>(this IList<T> collection, IList<T> other)
        {
            foreach (var item in other) {
                collection.Remove(item);
            }
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> items, Action<T> function)
        {
            foreach (var item in items) {
                function(item);
            }
            return items;
        }

        public static IEnumerable<U> Map<T, U>(this IEnumerable<T> items, Func<T, U> function)
        {
            foreach (var item in items) {
                yield return function(item);
            }
        }

        public static IEnumerable<U> FlapMap<T, U>(this IEnumerable<T> items, Func<T, U> function)
        {
            foreach (var item in items) {
                var collection = item as IEnumerable<U>;
                foreach (var element in collection) {
                    yield return element;
                }
            }
        }

        public static int GetIndexOfObject<T>(this IList<T> haystack, T needle, int defaultIndex = INVALID_INDEX) where T : class, IComparable
        {
            for (int i = 0; i < haystack.Count; i++) {
                if (haystack[i].Equals(needle)) {
                    return i;
                }
            }

            return defaultIndex;
        }

        public static T Min<T>(this IEnumerable<T> values) where T : IComparable
        {
            if (values.Count() == 0) {
                return default(T);
            }

            T currentMax = values.First();

            foreach (var value in values) {
                if (value.CompareTo(currentMax) < 0) {
                    currentMax = value;
                }
            }

            return currentMax;
        }

        public static T Max<T>(this IEnumerable<T> values) where T : IComparable
        {
            if (values.Count() == 0) {
                return default(T);
            }

            var currentMax = values.First();

            foreach (var value in values) {
                if (value.CompareTo(currentMax) > 0) {
                    currentMax = value;
                }
            }

            return currentMax;
        }
    }
}