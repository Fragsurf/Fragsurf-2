using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GameObjectPool : FSSharedScript
    {

        private List<PoolTimer> _available = new List<PoolTimer>();
        private List<PoolTimer> _inUse = new List<PoolTimer>();
        private GameObject _poolContainer;

        protected override void _Initialize()
        {
            _poolContainer = Game.NewGameObject();
            _poolContainer.name = "[Object Pool]";
        }

        protected override void _Destroy()
        {
            _available.Clear();
            _inUse.Clear();
            GameObject.Destroy(_poolContainer);
        }

        protected override void _Tick()
        {
            for(int i = _inUse.Count - 1; i >= 0; i--)
            {
                var obj = _inUse[i];
                if(!obj.Instance)
                {
                    _inUse.Remove(obj);
                    continue;
                }
                obj.Duration -= Time.fixedDeltaTime;
                if(obj.Duration <= 0)
                {
                    obj.Instance.SetActive(false);
                    _available.Add(obj);
                    _inUse.Remove(obj);
                }
            }
        }

        public GameObject Get(GameObject prefab, float duration)
        {
            var instance = PoolOrSpawnObject(prefab, duration).Instance;
            instance.SetActive(true);
            return instance;
        }

        private PoolTimer PoolOrSpawnObject(GameObject prefab, float duration)
        {
            foreach(var obj in _available)
            {
                if(obj.Prefab == prefab)
                {
                    obj.Duration = duration;
                    _available.Remove(obj);
                    _inUse.Add(obj);
                    return obj;
                }
            }

            var result = new PoolTimer()
            {
                Duration = duration,
                Prefab = prefab,
                Instance = GameObject.Instantiate(prefab, _poolContainer.transform, true)
            };

            _inUse.Add(result);

            return result;
        }

        private class PoolTimer
        {
            public GameObject Prefab;
            public GameObject Instance;
            public float Duration;
        }

    }
}

