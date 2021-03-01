using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SurfaceConfigurator 
{
    [CreateAssetMenu(fileName = "Surface Database", menuName = "Fragsurf/Surface Database")]
    public class SurfaceDatabase : ScriptableObject
    {

        [Header("Surface Types")]
        public List<SurfaceTypeConfig> SurfaceTypeConfigs;

        [Header("Materials")]
        public List<SurfaceConfig> SurfaceConfigs;

        public SurfaceConfig FindOrCreateSurfaceConfig(Material material)
        {
            var result = SurfaceConfigs.Find(x => x.Material == material);
            if(result == null)
            {
                result = new SurfaceConfig()
                {
                    Material = material,
                    SurfaceType = SurfaceType.Concrete
                };
                SurfaceConfigs.Add(result);
            }
            return result;
        }

#if UNITY_EDITOR
        //public static SurfaceConfig Find(Material mat)
        //{
        //    var dbs = FindAssetsByType<SurfaceDatabase>();
        //    foreach(var db in dbs)
        //    {
        //        var cfg = db.FindOrCreateSurfaceConfig(mat);
        //        if (cfg != null)
        //        {
        //            return cfg;
        //        }
        //    }
        //    return null;
        //}
#endif

    }

    [Serializable]
    public class SurfaceConfig
    {
        public Material Material;
        public SurfaceType SurfaceType;
    }

    [Serializable]
    public class SurfaceTypeConfig
    {
        public SurfaceType SurfaceType;
        public GameObject ImpactEffect;
        public AudioClip FootstepSound;
    }

}


