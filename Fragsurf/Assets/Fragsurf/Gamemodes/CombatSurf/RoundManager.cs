using Fragsurf.Actors;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool RoundsEnabled { get; set; } = true;
        [ConVar("rounds.duration", "Duration in seconds of each round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundDuration { get; set; } = 300;
        [ConVar("rounds.warmupduration", "Duration in seconds before game starts", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int WarmupDuration { get; set; } = 30;
        [ConVar("rounds.roundendduration", "Duration in seconds from round end to next round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundEndDuration { get; set; } = 3;
        [ConVar("rounds.freezeduration", "Duration in seconds at the start of a round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int FreezeDuration { get; set; } = 3;
        [ConVar("rounds.limit", "How many rounds until the game ends", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundLimit { get; set; } = 15;
        [ConVar("rounds.autostart", "", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public bool AutoStart { get; set; } = true;

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
        public RoundStates RoundState
        {
            get => _roundState;
            set => SetRoundState(value);
        }
        [NetProperty]
        public MatchStates MatchState
        {
            get => _matchState;
            set => SetMatchState(value);
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

        [ChatCommand("Restarts the match", "warmup", "startmatch")]
        public void ForceStartMatch(BasePlayer player)
        {
            if (!Game.IsHost || !Game.IsLocalServer)
            {
                return;
            }
            DoMatchWarmup();
        }

        [ChatCommand("Forces the match to end", "endmatch")]
        public void ForceEndMatch(BasePlayer player)
        {
            if (!Game.IsHost || !Game.IsLocalServer)
            {
                return;
            }
            DoMatchEnd();
        }

        [ChatCommand("Forces the round to end", "endround")]
        public void ForceEndRound(BasePlayer player)
        {
            if (!Game.IsHost || !Game.IsLocalServer)
            {
                return;
            }
            DoRoundEnd(DefaultWinner);
        }

        [ChatCommand("Changes your team (0-2), 0 = spec", "team")]
        public void ChatPickTeam(BasePlayer player, int teamNumber)
        {
            if (!Game.IsHost)
            {
                return;
            }
            Game.PlayerManager.SetPlayerTeam(player, (byte)Mathf.Clamp(teamNumber, 0, 2));
        }

        private void MoveToNextState()
        {
            switch (MatchState)
            {
                case MatchStates.None:
                    if (AutoStart && CanMatchAutoStart())
                    {
                        DoMatchWarmup();
                    }
                    break;
                case MatchStates.Warmup:
                    DoMatchLive();
                    break;
                case MatchStates.Post:
                    MatchState = MatchStates.None;
                    break;
                case MatchStates.Live:
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
                    break;
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
                DoMatchEnd();
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
            MatchState = MatchStates.Post;
            Timer = 20f;
            CleanRound();
            FreezePlayers(false);

            if (CurrentRound < RoundLimit)
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
            if(winningTeam > 0)
            {
                IncrementTeamScore(winningTeam);
            }
        }

        private bool CanMatchAutoStart()
        {
            int team1Players = 0;
            int team2Players = 0;

            foreach (var player in Game.PlayerManager.Players)
            {
                switch (player.Team)
                {
                    case 0:
                        continue;
                    case 1:
                        team1Players++;
                        break;
                    case 2:
                        team2Players++;
                        break;
                }
            }

            if(team1Players > 0 && team2Players > 0)
            {
                return true;
            }

            return false;
        }

        private void DoMatchLive()
        {
            MatchState = MatchStates.Live;
            CurrentRound = 1;

            for (int i = 0; i < 8; i++)
            {
                SetTeamScore(i, 0);
            }

            DoRoundFreeze();

            try
            {
                OnMatchStart?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void DoMatchWarmup()
        {
            MatchState = MatchStates.Warmup;
            Timer = WarmupDuration;
            CleanRound();
            FreezePlayers(false);
        }

        private void CleanRound()
        {
            if (!Game.IsHost)
            {
                return;
            }

            // Delete guns on ground and equip/respawn players
            for(int i = Game.EntityManager.Entities.Count - 1; i >= 0; i--)
            {
                var ent = Game.EntityManager.Entities[i];
                if(ent == null)
                {
                    continue;
                }
                if (ent is Equippable eq && eq.HumanId <= 0)
                {
                    ent.Delete();
                }
                else if (ent is Human hu)
                {
                    var owner = Game.PlayerManager.FindPlayer(hu.OwnerId);
                    if (owner == null || owner.Team == 0 || !hu.Enabled)
                    {
                        continue;
                    }

                    hu.Spawn(owner.Team);
                    hu.Health = 100;

                    if (!hu.Equippables.HasItemInSlot(ItemSlot.Melee))
                    {
                        hu.Give("Knife");
                    }

                    if (!hu.Equippables.HasItemInSlot(ItemSlot.Light))
                    {
                        hu.Give("M1911");
                    }

                    foreach (var item in hu.Equippables.Items)
                    {
                        if (!(item.EquippableGameObject is GunEquippable gun))
                        {
                            continue;
                        }
                        gun.RoundsInClip = gun.GunData.RoundsPerClip;
                        gun.ExtraRounds = gun.GunData.RoundsPerClip * gun.GunData.MaxClips;
                    }
                }
            }

            // Refresh actors (i.e. gun pickup can be reset)
            foreach(var actor in FindObjectsOfType<FSMActor>())
            {
                actor.Refresh();
            }
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
        None,
        Warmup,
        Live,
        Post
    }

}

