using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class GameData : ScriptableObject
    {

        private static GameData _instance;
        public static GameData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Instantiate(Resources.Load<GameData>("GameData")) as GameData;
                }
                return _instance;
            }
        } 

        [Header("Game")]
        public SceneReference MainMenu;

        [Header("Effects")]
        public GameObject BloodSplash;
        public GameObject WaterSplash;
        public GameObject FootstepSplash;

        [Header("Impact Effects")]
        public GameObject DefaultImpactEffect;
        public List<SurfaceMaterialImpactEffect> ImpactEffects;

        [Header("Equippables")]
        public BaseEquippableData[] Equippables;

        [Header("FMOD Audio")]
        [FMODUnity.EventRef]
        public string Footstep;
        [FMODUnity.EventRef]
        public string FallDamage;
        [FMODUnity.EventRef]
        public string DeathSound;

        public GameObject GetImpactEffect(SurfaceMaterialType materialType)
        {
            foreach(var effect in ImpactEffects)
            {
                if(effect.MaterialType == materialType)
                {
                    return effect.EffectPrefab;
                }
            }
            return DefaultImpactEffect;
        }

    }

    [Serializable]
    public class SurfaceMaterialImpactEffect
    {
        public SurfaceMaterialType MaterialType;
        public GameObject EffectPrefab;
    }

}
