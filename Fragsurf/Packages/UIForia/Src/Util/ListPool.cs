using System.Collections.Generic;
using System.Diagnostics;

namespace UIForia.Util {

    public static class ListPool<T> {

        private static readonly Stack<List<T>> s_ListPool = new Stack<List<T>>();
        private static readonly HashSet<List<T>> s_Contained = new HashSet<List<T>>();
        
        public static readonly IReadOnlyList<T> Empty = new List<T>(0);

        [DebuggerStepThrough]
        public static List<T> Get() {
            if (s_ListPool.Count > 0) {
                s_Contained.Remove(s_ListPool.Peek());
                return s_ListPool.Pop();
            }

            return new List<T>();
        }

        [DebuggerStepThrough]
        public static void Release(ref List<T> toRelease) {
            
            if (toRelease == null || Equals(toRelease, Empty) || s_Contained.Contains(toRelease)) {
                return;
            }

            s_Contained.Add(toRelease);
            s_ListPool.Push(toRelease);
            toRelease.Clear();
            toRelease = null;
            
        }

    }

}