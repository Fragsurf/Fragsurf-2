using UnityEngine;
using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using SurfaceConfigurator;

namespace Fragsurf.Misc
{
    public class WaterSplashEffect : MonoBehaviour, IClientComponent
    {

        private static int _waterLayer = -1;
        private float _timer;

        private void Awake()
        {
            if (_waterLayer == -1)
            {
                _waterLayer = LayerMask.NameToLayer("Water");
            }
        }

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
            if (_timer <= 0 && other.gameObject.layer == _waterLayer)
            {
                _timer = 1f;
                var cl = FSGameLoop.GetGameInstance(false);
                if (!cl)
                {
                    return;
                }
                var waterPrefab = GameData.Instance.GetImpactEffect(SurfaceType.Water);
                if (!waterPrefab)
                {
                    return;
                }
                var point = other.ClosestPointOnBounds(transform.position);
                var effect = cl.Pool.Get(waterPrefab, 1.5f);
                effect.transform.position = point;
                effect.transform.forward = Vector3.up;
            }
        }

    }
}

