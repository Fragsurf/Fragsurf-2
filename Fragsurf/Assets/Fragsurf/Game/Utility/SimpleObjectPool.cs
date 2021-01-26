using System.Collections.Concurrent;

namespace Fragsurf.Utility
{
    public class SimpleObjectPool<T> where T : new()
    {

        public SimpleObjectPool(int max = 10, bool populate = false)
        {
            _max = max;

            if (populate)
            {
                for (int i = 0; i < max; i++)
                {
                    _items.Add(new T());
                }
            }
        }

        private readonly ConcurrentBag<T> _items = new ConcurrentBag<T>();
        private int _counter = 0;
        private int _max;

        public void Release(T item)
        {
            if (_counter < _max)
            {
                _items.Add(item);
                _counter++;
            }
        }

        public T Get()
        {
            if (_items.TryTake(out T item))
            {
                _counter--;
                return item;
            }
            else
            {
                T obj = new T();
                _items.Add(obj);
                _counter++;
                return obj;
            }
        }
    }
}
