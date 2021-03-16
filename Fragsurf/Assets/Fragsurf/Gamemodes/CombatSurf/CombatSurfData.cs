using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class CombatSurfData : GamemodeData
    {

        [Header("Round Audio")]
        public AudioClip MatchStart;
        public AudioClip MatchEnd;
        public AudioClip RoundLive;
        public AudioClip RoundEndLose;
        public AudioClip RoundEndWin;
        public AudioClip RoundFreeze;
        public AudioClip SpawnProtectedEnded;

    }
}

