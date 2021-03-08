using System;
using UnityEngine;
using Fragsurf.Maps;
using Fragsurf.DataEditor;
using SurfaceConfigurator;

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

        public GameObject GetImpactEffect(SurfaceType surfaceType)
        {
            if (!Surfaces)
            {
                return null;
            }
            foreach(var cfg in Surfaces.SurfaceTypeConfigs)
            {
                if(cfg.SurfaceType == surfaceType)
                {
                    return cfg.ImpactEffects.Length > 0 
                        ? cfg.ImpactEffects[UnityEngine.Random.Range(0, cfg.ImpactEffects.Length)] 
                        : null;
                }
            }
            return null;
        }

    }
}
