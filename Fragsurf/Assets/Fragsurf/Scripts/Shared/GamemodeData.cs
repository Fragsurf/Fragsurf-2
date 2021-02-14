using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [CreateAssetMenu(fileName = "New Gamemode", menuName = "Fragsurf/Gamemode")]
    public class GamemodeData : ScriptableObject
    {

        public string Name;
        public string Identifier;

        [ClassExtends(typeof(BaseGamemode))]
        public ClassTypeReference GamemodeType;

    }
}

