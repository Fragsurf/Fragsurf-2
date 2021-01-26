using System.Collections.Generic;

namespace UIForia.Util {

    public static class QueuePool<T> {

        private static readonly ObjectPool<Queue<T>> s_QueuePool = new ObjectPool<Queue<T>>(null, l => l.Clear());

        public static Queue<T> Get() {
            return s_QueuePool.Get();
        }

        public static void Release(Queue<T> toRelease) {
            s_QueuePool.Release(toRelease);
        }

    }

}