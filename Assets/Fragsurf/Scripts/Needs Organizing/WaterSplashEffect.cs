using UnityEngine;
using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Misc
{
    public class WaterSplashEffect : MonoBehaviour, IClientComponent
    {

        private float _timer;

        private void OnDisable()
        {
            _timer = 0;
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_timer <= 0 && other.gameObject.layer == Layers.Water)
            {
                _timer = 1f;
                var cl = FSGameLoop.GetGameInstance(false);
                if (!cl)
                {
                    return;
                }
                if(GameData.Instance.TryGetImpactPrefab(SurfaceType.Water, out GameObject prefab))
                {
                    var point = other.ClosestPointOnBounds(transform.position);
                    var effect = cl.Pool.Get(prefab, 1.5f);
                    effect.transform.position = point;
                    effect.transform.forward = Vector3.up;
                }
            }
        }

    }
}

