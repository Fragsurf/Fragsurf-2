using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Tricksurf : BaseGamemode
    {
        protected override void _Load(FSGameLoop game)
        {
            LockVars = true;

            if(game.IsServer)
            {
                game.LagCompensator.Enabled = false;
            }
        }

        protected override void _Unload(FSGameLoop game)
        {
        }

        public override void ExecuteGameConfig()
        {
            base.ExecuteGameConfig();

            DevConsole.SetVariable("mv.acceleration", 10f, true, true);
            DevConsole.SetVariable("mv.falldamage", false, true, true);
            DevConsole.SetVariable("mv.autobhop", true, true, true);
            DevConsole.SetVariable("mv.solidplayers", false, true, true);
            DevConsole.SetVariable("entity.nodamage", true, true, true);
            DevConsole.SetVariable("entity.decay", .01f, true, true);
        }
    }
}

