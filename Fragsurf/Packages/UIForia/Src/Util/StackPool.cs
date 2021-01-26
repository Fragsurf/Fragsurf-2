using System.Collections.Generic;

namespace UIForia.Util {

    public static class StackPool<T> {

        private static readonly ObjectPool<Stack<T>> s_StackPool = new ObjectPool<Stack<T>>(null, l => l.Clear());

        public static Stack<T> Get() {
            return s_StackPool.Get();
        }

        public static void Release(Stack<T> toRelease) {
            s_StackPool.Release(toRelease);
        }

    }

}