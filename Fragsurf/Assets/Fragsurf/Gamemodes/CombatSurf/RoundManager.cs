using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class RoundManager : FSSharedScript
    {

        [ConVar("rounds.enabled", "", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public bool RoundsEnabled { get; set; }
        [ConVar("rounds.duration", "Duration in seconds of each round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundDuration { get; set; } = 5;
        [ConVar("rounds.warmupduration", "Duration in seconds before game starts", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int WarmupDuration { get; set; } = 30;
        [ConVar("rounds.cooldownduration", "Duration in seconds from round end to next round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int CooldownDuration { get; set; } = 4;
        [ConVar("rounds.freezeduration", "Duration in seconds at the start of a round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int FreezeDuration { get; set; } = 4;
        [ConVar("rounds.limit", "How many rounds until the game ends", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundLimit { get; set; } = 15;

        public override bool HasNetProps => true;

        [NetProperty]
        public MatchStates MatchState { get; set; }
        [NetProperty]
        public RoundStates RoundState { get; set; }
        [NetProperty]
        public float Timer { get; set; }
        [NetProperty]
        public int CurrentRound { get; set; }

        protected override void _Tick()
        {
            if (Game.IsHost)
            {
                Timer -= Time.fixedDeltaTime;
                if (Timer <= 0)
                {
                    MoveToNextState();
                }
            }
        }

        private void MoveToNextState()
        {
            if(MatchState == MatchStates.Pre)
            {
                TryStartMatch();
            }
            else if(MatchState == MatchStates.Post)
            {
                MatchState = MatchStates.Pre;
            }
            else if(MatchState == MatchStates.Live)
            {
                switch (RoundState)
                {
                    case RoundStates.Freeze:
                        RoundState = RoundStates.Live;
                        Timer = RoundDuration;
                        OnRoundLive();
                        break;
                    case RoundStates.Live:
                        RoundState = RoundStates.Cooldown;
                        Timer = CooldownDuration;
                        OnRoundExpire();
                        break;
                    case RoundStates.Cooldown:
                        RoundState = RoundStates.Freeze;
                        Timer = FreezeDuration;
                        OnRoundEnd();
                        break;
                }
            }
        }

        private void OnRoundExpire()
        {
            ScoreRound(0);
        }

        private void OnRoundLive()
        {
            FreezePlayers(false);
        }

        private void OnRoundEnd()
        {
            CurrentRound++;
            if(CurrentRound >= RoundLimit)
            {
                OnMatchEnd();
                MatchState = MatchStates.Post;
                Timer = 20f;
                return;
            }
            CleanRound();
            FreezePlayers(true);
            RoundState = RoundStates.Freeze;
            Timer = FreezeDuration;
        }

        private void OnMatchEnd()
        {
            if(CurrentRound < RoundLimit)
            {
                // ended abruptly
            }
        }

        private void ScoreRound(int winner)
        {
            Debug.Log("Winner: " + winner);
        }

        private void TryStartMatch()
        {
            // start match when there's enough players and players on separate teams
            MatchState = MatchStates.Live;
            RoundState = RoundStates.Freeze;
        }

        private void CleanRound()
        {
            foreach(var player in Game.PlayerManager.Players)
            {
                if(player.Entity is Human hu && player.Team > 0)
                {
                    hu.Spawn(player.Team);
                    hu.Health = 100;
                    if(!hu.Equippables.HasItemInSlot(ItemSlot.Melee))
                    {
                        hu.Give("Knife");
                    }
                    if (!hu.Equippables.HasItemInSlot(ItemSlot.Light))
                    {
                        hu.Give("M1911");
                    }
                    foreach (var item in hu.Equippables.Items)
                    {
                        if(!(item.EquippableGameObject is GunEquippable gun))
                        {
                            continue;
                        }
                        gun.RoundsInClip = gun.GunData.RoundsPerClip;
                        gun.ExtraRounds = gun.GunData.RoundsPerClip * gun.GunData.MaxClips;
                    }
                }
            }
            // reset entities on ground etc
        }

        private void FreezePlayers(bool frozen)
        {
            foreach (var player in Game.PlayerManager.Players)
            {
                if (player.Entity is Human hu)
                {
                    hu.Frozen = frozen;
                }
            }
        }

    }

    public enum RoundStates
    {
        Freeze,
        Live,
        Cooldown
    }

    public enum MatchStates
    {
        Pre,
        Live,
        Post
    }

}

