using System.Collections.Generic;

namespace UIForia.Util {

    public class LRUCache<K, V> {

        private readonly int capacity;
        private readonly Dictionary<K, LinkedListNode<LRUCacheItem>> cacheMap;
        private readonly LinkedList<LRUCacheItem> lruList;

        public LRUCache(int capacity) {
            this.capacity = capacity;
            this.cacheMap = new Dictionary<K, LinkedListNode<LRUCacheItem>>(capacity);
            lruList = new LinkedList<LRUCacheItem>();
        }

        public V Get(K key) {
            LinkedListNode<LRUCacheItem> node;
            if (cacheMap.TryGetValue(key, out node)) {
                V value = node.Value.value;
                lruList.Remove(node);
                lruList.AddLast(node);
                return value;
            }

            return default(V);
        }
        
        public bool TryGet(K key, out V v) {
            LinkedListNode<LRUCacheItem> node;
            if (cacheMap.TryGetValue(key, out node)) {
                V value = node.Value.value;
                lruList.Remove(node);
                lruList.AddLast(node);
                v = value;
                return true;
            }

            v = default;
            return false;
        }

        public void Add(K key, V val) {
            
            if(cacheMap.TryGetValue(key, out LinkedListNode<LRUCacheItem> _)) {
                return;
            }
            
            if (cacheMap.Count >= capacity) {
                // Remove from LRUPriority
                LinkedListNode<LRUCacheItem> first = lruList.First;
                lruList.RemoveFirst();

                // Remove from cache
                cacheMap.Remove(first.Value.key);
            }

            LRUCacheItem cacheItem = new LRUCacheItem (key, val);
            LinkedListNode<LRUCacheItem> node = new LinkedListNode<LRUCacheItem>(cacheItem);
            lruList.AddLast(node);
            cacheMap.Add(key, node);
        }
        
        private struct LRUCacheItem {
            
            public readonly K key;
            public readonly V value;
            
            public LRUCacheItem(K k, V v) {
                key = k;
                value = v;
            }

        }
    }

    

}