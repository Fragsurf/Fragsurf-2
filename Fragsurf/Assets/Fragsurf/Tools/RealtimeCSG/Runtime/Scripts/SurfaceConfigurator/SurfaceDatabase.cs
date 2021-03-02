using Fragsurf.DataEditor;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SurfaceConfigurator 
{
    [DataEditor]
    [CreateAssetMenu(fileName = "Surface Database", menuName = "Fragsurf/Surface Database")]
    public class SurfaceDatabase : ScriptableObject
    {

        public List<SurfaceConfig> SurfaceConfigs;

        public SurfaceConfig FindSurfaceConfig(Material material)
        {
            return SurfaceConfigs.Find(x => x.Material == material);
        }

        public SurfaceConfig FindOrCreateSurfaceConfig(Material material)
        {
            var result = FindSurfaceConfig(material);
            if(result == null)
            {
                result = new SurfaceConfig()
                {
                    Material = material,
                    SurfaceType = SurfaceType.Concrete
                };
                SurfaceConfigs.Add(result);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
#endif
            }
            return result;
        }

    }

    [Serializable]
    public class SurfaceConfig
    {
        public Material Material;
        public SurfaceType SurfaceType;
    }

}


