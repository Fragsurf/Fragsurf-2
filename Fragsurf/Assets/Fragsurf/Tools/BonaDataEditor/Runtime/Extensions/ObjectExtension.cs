using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fyrvall.DataEditor
{
    public static class ObjectExtension
    {
        public static T To<T>(this object o)
        {
            return (T)o;
        }

        public static T ToOrDefault<T>(this object o)
        {
            try {
                return (T)o;
            } catch (Exception) {
                return default(T);
            }
        }

        public static bool In<T>(this T item, params T[] items)
        {
            if (items == null) {
                throw new ArgumentNullException();
            }

            return items.Contains(item);
        }

        public static bool NotIn<T>(this T item, params T[] items)
        {
            if (items == null) {
                throw new ArgumentNullException();
            }

            return !items.Contains(item);
        }

        public static U Transform<T, U>(this T item, Func<T, U> function)
        {
            return function(item);
        }

        public static T Do<T>(this T item, Action<T> function)
        {
            function(item);
            return item;
        }

        public static bool IsInstanceOf(this UnityEngine.Object o, System.Type type)
        {
            if (o == null) {
                return false;
            }

            return type.IsAssignableFrom(o.GetType());
        }

        public static T Todo<T>(this object o, String message = "Not implemented")
        {
            throw new NotImplementedException(message);
        }

        public static T Let<T>(this T value, Action<T> fn)
        {
            if (value != null) {
                fn(value);
            }

            return value;
        }
    }
}