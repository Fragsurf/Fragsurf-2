using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Client, typeof(CombatSurf))]
    public class RoundManagerJuice : FSSharedScript
    {

        private GameAudioSource _audioSrc;
        private CombatSurfData Data => Gamemode.Data as CombatSurfData;

        protected override void _Start()
        {
            var rm = Game.Get<RoundManager>();
            rm.OnMatchStart += Rm_OnMatchStart;
            rm.OnMatchEnd += Rm_OnMatchEnd; ;
            rm.OnRoundFreeze += Rm_OnRoundFreeze;
            rm.OnRoundEnd += Rm_OnRoundEnd;
            rm.OnRoundLive += Rm_OnRoundLive;
            _audioSrc = gameObject.AddComponent<GameAudioSource>();
            _audioSrc.Category = SoundCategory.Effects;
        }

        private void Rm_OnRoundLive(int roundNumber)
        {
            if (!Data)
            {
                return;
            }
            _audioSrc.PlayClip(Data.RoundLive);
            Game.Get<TextChat>().PrintChat("[Match]", "<color=#14ff92>The round is live!</color>");
        }

        private void Rm_OnRoundEnd(int roundNumber, int winningTeam)
        {
            if (!Data)
            {
                return;
            }

            string msg;

            if(Game.PlayerManager.LocalPlayer != null 
                && Game.PlayerManager.LocalPlayer.Team != 0)
            {
                if(Game.PlayerManager.LocalPlayer.Team == winningTeam)
                {
                    _audioSrc.PlayClip(Data.RoundEndWin);
                    msg = "<color=#4acfff>your team won!</color>";
                    var rm = UGuiManager.Instance.Find<Modal_RoundManager>();
                    if (rm)
                    {
                        rm.FlashScreenBorder(Color.green);
                    }
                }
                else
                {
                    _audioSrc.PlayClip(Data.RoundEndLose);
                    msg = "<color=#ff4a4a>your team lost!</color>";
                    var rm = UGuiManager.Instance.Find<Modal_RoundManager>();
                    if (rm)
                    {
                        rm.FlashScreenBorder(Color.red);
                    }
                }
            }
            else
            {
                _audioSrc.PlayClip(Data.RoundEndLose);
                msg = $"<color=#14ffef>Team {winningTeam}</color> won!";
            }

            Game.Get<TextChat>().PrintChat("[Match]", $"<color=#14ff92>The round has ended, {msg}</color>");
        }

        private void Rm_OnRoundFreeze(int roundNumber)
        {
            if (!Data)
            {
                return;
            }
            _audioSrc.PlayClip(Data.RoundFreeze);
            Game.Get<TextChat>().PrintChat("[Match]", $"<color=#14ff92>Round {roundNumber} will start soon.</color>");
        }

        private void Rm_OnMatchStart()
        {
            if (!Data)
            {
                return;
            }
            _audioSrc.PlayClip(Data.MatchStart);
            Game.Get<TextChat>().PrintChat("[Match]", $"<color=green>The match has started!</color>");
        }

        private void Rm_OnMatchEnd(int winningTeam)
        {
            if (!Data)
            {
                return;
            }
            _audioSrc.PlayClip(Data.MatchEnd);

            string msg;

            if (Game.PlayerManager.LocalPlayer != null
                && Game.PlayerManager.LocalPlayer.Team != 0)
            {
                msg = Game.PlayerManager.LocalPlayer.Team == winningTeam
                    ? "<color=#4acfff>your team won the match! ( ͡^ ͜ʖ ͡^)</color>"
                    : "<color=#ff4a4a>your team lost the match! ( ͡° ʖ̯ ͡°)</color> ";
            }
            else
            {
                msg = $"<color=#14ffef>Team {winningTeam}</color> won the match!";
            }

            Game.Get<TextChat>().PrintChat("[Match]", $"<color=#eb49fc>The match has ended, {msg}</color>");
        }

    }
}

