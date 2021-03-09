using Fragsurf.DataEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SurfaceConfigurator 
{
    [DataEditor]
    [CreateAssetMenu(fileName = "Surface Types", menuName = "Fragsurf/Surface Types")]
    public class SurfaceTypeDatabase : ScriptableObject
    {
        public List<SurfaceTypeConfig> SurfaceTypeConfigs;

        public SurfaceTypeConfig GetSurfaceTypeConfig(SurfaceType type)
        {
            foreach(var cfg in SurfaceTypeConfigs)
            {
                if(cfg.SurfaceType == type)
                {
                    return cfg;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class SurfaceTypeConfig
    {
        public SurfaceType SurfaceType;
        public GameObject[] BulletImpactEffects;
        public GameObject[] SlashImpactEffects;
        public GameObject[] BluntImpactEffects;
        public AudioClip[] FootstepSounds;
        public AudioClip[] PhysicsSounds;

        public AudioClip GetFootstepSound()
        {
            if(FootstepSounds.Length == 0)
            {
                return null;
            }
            return FootstepSounds[UnityEngine.Random.Range(0, FootstepSounds.Length)];
        }

        public AudioClip GetGunDropSound()
        {
            if (PhysicsSounds.Length == 0)
            {
                return null;
            }
            return PhysicsSounds[UnityEngine.Random.Range(0, PhysicsSounds.Length)];
        }

    }
}


