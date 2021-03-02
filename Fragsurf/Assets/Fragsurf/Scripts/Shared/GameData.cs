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

        public GameObject GetImpactEffect(SurfaceType surfaceType)
        {
            return null;
        }

    }
}
