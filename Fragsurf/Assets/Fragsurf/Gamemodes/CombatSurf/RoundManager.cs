using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using System;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class RoundManager : FSSharedScript
    {

        private MatchStates _matchState;
        private RoundStates _roundState;

        public event Action OnMatchStart;
        public event Action<int> OnMatchEnd;
        public event Action<int> OnRoundLive;
        public event Action<int, int> OnRoundEnd;
        public event Action<int> OnRoundFreeze;

        [ConVar("rounds.enabled", "", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public bool RoundsEnabled { get; set; }
        [ConVar("rounds.duration", "Duration in seconds of each round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundDuration { get; set; } = 5;
        [ConVar("rounds.warmupduration", "Duration in seconds before game starts", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int WarmupDuration { get; set; } = 30;
        [ConVar("rounds.roundendduration", "Duration in seconds from round end to next round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundEndDuration { get; set; } = 4;
        [ConVar("rounds.freezeduration", "Duration in seconds at the start of a round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int FreezeDuration { get; set; } = 4;
        [ConVar("rounds.limit", "How many rounds until the game ends", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundLimit { get; set; } = 15;

        public override bool HasNetProps => true;

        [NetProperty]
        public float Timer { get; set; }
        [NetProperty]
        public int CurrentRound { get; set; }
        [NetProperty]
        public int DefaultWinner { get; set; }
        [NetProperty]
        public int RoundWinner { get; set; }
        [NetProperty]
        public int MatchWinner { get; set; }
        [NetProperty]
        public MatchStates MatchState
        {
            get => _matchState;
            set => SetMatchState(value);
        }
        [NetProperty]
        public RoundStates RoundState
        {
            get => _roundState;
            set => SetRoundState(value);
        }

        private void SetMatchState(MatchStates state)
        {
            _matchState = state;

            if (!Game.IsHost)
            {
                switch (state)
                {
                    case MatchStates.Live:
                        try { OnMatchStart?.Invoke(); } catch (Exception e) { Debug.LogError(e); }
                        break;
                    case MatchStates.Post:
                        try { OnMatchEnd?.Invoke(MatchWinner); } catch(Exception e) { Debug.LogError(e); }
                        break;
                }
            }
        }

        private void SetRoundState(RoundStates state)
        {
            _roundState = state;

            if (!Game.IsHost)
            {
                switch (RoundState)
                {
                    case RoundStates.Live:
                        try { OnRoundLive?.Invoke(CurrentRound); } catch(Exception e) { Debug.LogError(e); }
                        break;
                    case RoundStates.End:
                        try { OnRoundEnd?.Invoke(CurrentRound, RoundWinner); } catch (Exception e) { Debug.LogError(e); }
                        break;
                    case RoundStates.Freeze:
                        try { OnRoundFreeze?.Invoke(CurrentRound); } catch(Exception e) { Debug.LogError(e); }
                        break;
                }
            }
        }

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
                        DoRoundLive();
                        break;
                    case RoundStates.Live:
                        DoRoundEnd(DefaultWinner);
                        break;
                    case RoundStates.End:
                        DoRoundFreeze();
                        break;
                }
            }
        }

        private void DoRoundEnd(int winningTeam)
        {
            if(CurrentRound > 0)
            {
                ScoreRound(winningTeam);
            }

            RoundWinner = winningTeam;
            RoundState = RoundStates.End;
            Timer = RoundEndDuration;

            try
            {
                OnRoundEnd?.Invoke(CurrentRound, winningTeam);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

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
            RoundState = RoundStates.Live;
            Timer = RoundDuration;
            FreezePlayers(false);

            try
            {
                OnRoundLive?.Invoke(CurrentRound);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void DoRoundFreeze()
        {
            CleanRound();
            FreezePlayers(true);
            RoundState = RoundStates.Freeze;
            Timer = FreezeDuration;
            DefaultWinner = UnityEngine.Random.Range(1, 3);

            try
            {
                OnRoundFreeze?.Invoke(CurrentRound);
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void DoMatchEnd()
        {
            if(CurrentRound < RoundLimit)
            {
                // ended abruptly
            }

            MatchWinner = 1;

            try
            {
                OnMatchEnd?.Invoke(MatchWinner);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
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

            try
            {
                OnMatchStart?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
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
        End
    }

    public enum MatchStates
    {
        Pre,
        Live,
        Post
    }

}

