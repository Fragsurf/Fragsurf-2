using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    [Inject(InjectRealm.Shared, typeof(CombatSurf))]
    public class CombatSurfGameFlow : FSSharedScript
    {

        private FlowState _gameState;
        private FlowState _roundState;

        [ConVar("rounds.enabled", "", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public bool RoundsEnabled { get; set; }
        [ConVar("rounds.duration", "Duration in seconds of each round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int RoundDuration { get; set; } = 300;
        [ConVar("rounds.warmupduration", "Duration in seconds before game starts", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int WarmupDuration { get; set; } = 30;
        [ConVar("rounds.cooldownduration", "Duration in seconds from round end to next round", ConVarFlags.Gamemode | ConVarFlags.Replicator)]
        public int CooldownDuration { get; set; } = 30;

        [ConVar("rounds.gamestate", "", ConVarFlags.Gamemode | ConVarFlags.Replicator | ConVarFlags.Poll)]
        public int GameState
        {
            get => (int)_gameState;
            set => SetGameState((FlowState)value);
        }

        [NetProperty]
        public int RoundState
        {
            get => (int)_roundState;
            set => SetRoundState((FlowState)value);
        }

        public override bool HasNetProps => true;

        protected override void _Update()
        {
            if(Game.IsHost && Input.GetKeyDown(KeyCode.T))
            {
                _gameState = FlowState.Live;
            }
        }

        private void SetGameState(FlowState state)
        {
            _gameState = state;
        }

        private void SetRoundState(FlowState state)
        {
            _roundState = state;
        }

        public enum FlowState : int
        {
            Pre = 1,
            Live = 2,
            Post = 3
        }

    }
}

