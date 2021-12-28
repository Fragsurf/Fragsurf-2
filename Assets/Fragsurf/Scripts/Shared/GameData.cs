using System;
using UnityEngine;
using Fragsurf.DataEditor;

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
        public SceneReference EmptyScene;
        public SceneReference MainServer;
        public SceneReference StartupScene;
        public GamemodeData[] DefaultGamemodes;
        public BaseEquippableData[] Equippables;
        public AudioClip UnderwaterSound;
        public AudioClip ExitWaterSound;
        public AudioClip SwimSound;
        public AudioClip FootstepSound;

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
            clip = null;
            return false;
        }

        public bool TryGetImpactPrefab(ImpactType impactType, SurfaceType surfaceType, out GameObject prefab)
        {
            prefab = null;
            return false;
        }

        public bool TryGetImpactPrefab(SurfaceType surfaceType, out GameObject prefab) => (prefab = GetImpactEffect(surfaceType)) != null;

        private GameObject GetImpactEffect(SurfaceType surfaceType)
        {
            return null;
        }

    }

    public enum SurfaceType
    {
        Stone,
        Wood,
        Glass,
        Water,
        Flesh,
        Carpet,
        Metal,
        Concrete
    }

    public enum ImpactType
    {
        Bullet,
        Slash,
        Blunt
    }

}
