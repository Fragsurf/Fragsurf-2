using Fragsurf.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop 
{
    [Inject(InjectRealm.Client, typeof(Bunnyhop))]
    public class BunnyhopSSJ : FSSharedScript
    {

        protected override void OnPlayerRunCommand(IPlayer player)
        {
            if(player.Entity == null
                || !(player.Entity is Human hu)
                || !(hu.Timeline is BunnyhopTimeline bhop)
                || !(hu.MovementController is DefaultMovementController move)
                || !move.MoveData.JustJumped)
            {
                return;
            }

            if(bhop.LastFrame.Jumps == 6 && Game.Get<SpectateController>().TargetHuman == hu)
            {
                Game.TextChat.PrintChat("[Timer]", $"SSJ: <color=yellow>{hu.HammerVelocity()}</color>");
            }
        }

    }
}


