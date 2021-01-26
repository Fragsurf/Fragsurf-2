using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [CreateAssetMenu(fileName = "GameData", menuName = "Fragsurf/Game Data")]
    public class DataSet : ScriptableObject
    {

        private static DataSet _instance;
        public static DataSet Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.Instantiate(Resources.Load<DataSet>("GameData")) as DataSet;
                }
                return _instance;
            }
        }

        [Header("Game")]
        public SceneReference MainMenu;
        public Camera GameCamera;

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
