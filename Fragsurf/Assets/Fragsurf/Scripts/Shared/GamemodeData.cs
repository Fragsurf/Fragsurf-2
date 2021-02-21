using Fragsurf.DataEditor;
using Fragsurf.UI;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared
{
    [DataEditor]
    [CreateAssetMenu(fileName = "New Gamemode", menuName = "Fragsurf/Gamemode")]
    public class GamemodeData : ScriptableObject
    {

        public string Name;
        public string Identifier;

        [ClassExtends(typeof(BaseGamemode))]
        public ClassTypeReference GamemodeType;

        public GameObject[] InstantiateOnLoad;

    }
}

