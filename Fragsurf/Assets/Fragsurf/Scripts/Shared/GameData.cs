using System;
using UnityEngine;
using Fragsurf.Maps;
using Fragsurf.DataEditor;
using SurfaceConfigurator;
using System.Xml;

namespace Fragsurf.Shared
{
    [DataEditor]
    public class GameData : ScriptableObject
    {

        private static GameData _instance;
        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Instantiate(Resources.Load<GameData>("GameData"));
                }
                return _instance;
            }
        }

        public SceneReference MainMenu;
        public GamemodeData[] DefaultGamemodes;
        public BaseEquippableData[] Equippables;
        public SurfaceTypeDatabase Surfaces;
        public AudioClip UnderwaterSound;
        public AudioClip ExitWaterSound;
        public AudioClip SwimSound;

        public bool TryGetEquippable(string name, out BaseEquippableData equippable)
        {
            foreach(var eq in Equippables)
            {
                if (!eq)
                {
                    continue;
                }
                if(eq.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    equippable = eq;
                    return true;
                }
            }
            equippable = null;
            return false;
        }

        public bool TryGetPhysicsSound(SurfaceType surfaceType, out AudioClip clip)
        {
            var cfg = GetSurfaceConfig(surfaceType);
            if (cfg == null)
            {
                clip = null;
                return false;
            }
            clip = cfg.PhysicsSounds.Length > 0
                ? cfg.PhysicsSounds[UnityEngine.Random.Range(0, cfg.PhysicsSounds.Length)]
                : null;
            return clip != null;
        }

        public bool TryGetImpactPrefab(SurfaceType surfaceType, out GameObject prefab) => (prefab = GetImpactEffect(surfaceType)) != null;

        private GameObject GetImpactEffect(SurfaceType surfaceType)
        {
            var cfg = GetSurfaceConfig(surfaceType);
            if (cfg == null)
            {
                return null;
            }
            return cfg.BulletImpactEffects.Length > 0
                ? cfg.BulletImpactEffects[UnityEngine.Random.Range(0, cfg.BulletImpactEffects.Length)]
                : null;
        }

        private SurfaceTypeConfig GetSurfaceConfig(SurfaceType surfaceType)
        {
            if(Surfaces == null)
            {
                return null;
            }
            foreach(var cfg in Surfaces.SurfaceTypeConfigs)
            {
                if(cfg.SurfaceType == surfaceType)
                {
                    return cfg;
                }
            }
            return null;
        }

    }
}
