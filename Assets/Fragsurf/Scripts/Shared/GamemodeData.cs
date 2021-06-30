using Fragsurf.DataEditor;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using Fragsurf.Utility;
using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    [DataEditor]
    [CreateAssetMenu(fileName = "New Gamemode", menuName = "Fragsurf/Gamemode")]
    public class GamemodeData : ScriptableObject
    {

        [Header("Gamemode Config")]

        public string Name;
        public string[] MapPrefixes;

        [ClassExtends(typeof(BaseGamemode))]
        public ClassTypeReference GamemodeType;

        public GameObject HumanPrefab;

        public GameObject[] InstantiateOnLoad;

        public bool MapHasPrefix(string mapName)
        {
            foreach(var p in MapPrefixes)
            {
                if(mapName.StartsWith(p, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

    }
}

