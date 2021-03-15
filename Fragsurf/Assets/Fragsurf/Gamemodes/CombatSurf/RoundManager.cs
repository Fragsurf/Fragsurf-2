using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class RoundManager : FSSharedScript
    {

        public event Action OnMatchStart;
        public event Action<int> OnMatchEnd;
        public event Action<int> OnRoundStart;
        public event Action<int, int> OnRoundEnd;
        public event Action<int> OnRoundFreeze;
        public event Action<int> OnRoundExpired;

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
        [NetProperty]
        public int DefaultWinner { get; set; }

        protected override void _Tick()
        {
            if (Game.IsHost)
            {
                Timer -= Time.fixedDeltaTime;
                if (Timer <= 0)
                {
                    MoveToNextState();
                }
                if(/*team1won*/false)
                {

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
                        DoRoundLive();
                        break;
                    case RoundStates.Live:
                        RoundState = RoundStates.Cooldown;
                        Timer = CooldownDuration;
                        OnRoundExpired?.Invoke(CurrentRound);
                        DoRoundEnd(DefaultWinner);
                        break;
                    case RoundStates.Cooldown:
                        RoundState = RoundStates.Freeze;
                        Timer = FreezeDuration;
                        DoRoundFreeze();
                        break;
                }
            }
        }

        private void DoRoundEnd(int winningTeam)
        {
            ScoreRound(winningTeam);
            RoundState = RoundStates.Cooldown;
            Timer = CooldownDuration;

            OnRoundEnd?.Invoke(CurrentRound, winningTeam);

            CurrentRound++;

            if (CurrentRound >= RoundLimit)
            {
                MatchState = MatchStates.Post;
                Timer = 20f;
                CleanRound();
                FreezePlayers(false);
                DoMatchEnd();
                return;
            }
        }

        private void DoRoundLive()
        {
            FreezePlayers(false);
            OnRoundStart?.Invoke(CurrentRound);
        }

        private void DoRoundFreeze()
        {
            CleanRound();
            FreezePlayers(true);
            RoundState = RoundStates.Freeze;
            Timer = FreezeDuration;
            DefaultWinner = UnityEngine.Random.Range(1, 3);
            OnRoundFreeze?.Invoke(CurrentRound);
        }

        private void DoMatchEnd()
        {
            if(CurrentRound < RoundLimit)
            {
                // ended abruptly
            }
            OnMatchEnd?.Invoke(1);
        }

        public int GetTeamScore(int teamNumber) => (int)Game.Get<PlayerProps>().GetProp(-teamNumber, "Score");
        public void SetTeamScore(int teamNumber, int score) => Game.Get<PlayerProps>().SetProp(-teamNumber, "Score", score);
        public void IncrementTeamScore(int teamNumber) => Game.Get<PlayerProps>().IncrementProp(-teamNumber, "Score", 1);

        private void ScoreRound(int winningTeam)
        {
            if(winningTeam <= 0)
            {

            }
            else
            {
                IncrementTeamScore(winningTeam);
            }
        }

        private void TryStartMatch()
        {
            // start match when there's enough players and players on separate teams
            MatchState = MatchStates.Live;
            RoundState = RoundStates.Freeze;
            DoMatchStart();
        }

        private void DoMatchStart()
        {
            CurrentRound = 0;
            for (int i = 0; i < 8; i++)
            {
                SetTeamScore(i, 0);
            }
            OnMatchStart?.Invoke();
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

