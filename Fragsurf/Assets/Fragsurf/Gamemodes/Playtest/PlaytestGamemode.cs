using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Gamemodes.Playtest
{
    public class PlaytestGamemode : BaseGamemode
    {
        protected override void _Load(FSGameLoop game)
        {
            LockVars = false;

            Debug.Log("Playtest Gamemode Loaded");
        }

        protected override void _Unload(FSGameLoop game)
        {
            Debug.Log("Playtest Gamemode Unloaded");
        }
    }
}

