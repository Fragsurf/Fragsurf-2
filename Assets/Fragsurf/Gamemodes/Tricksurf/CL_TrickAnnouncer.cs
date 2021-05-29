using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Tricksurf
{
    [Inject(InjectRealm.Client, typeof(Tricksurf))]
    public class CL_TrickAnnouncer : FSSharedScript
    {

        protected override void _Start()
        {
            base._Start();

            Game.Get<SH_Tricksurf>().OnTrickCompleted += OnTrickCompleted;
        }

        private void OnTrickCompleted(BasePlayer player, TrickCompletion completion)
        {
            var tname = completion.TrickName;
            if(completion.ComboCount > 1)
            {
                tname += $" (x{completion.ComboCount})";
            }
            var msg = $"<color=#71d9f0><b>{tname}</b></color> in <color=yellow>{completion.CompletionTime:0.000}s</color> <color=green><b>+{completion.Points} points</b></color>";
            Game.TextChat.PrintChat(player.DisplayName, msg);
        }
    }
}

