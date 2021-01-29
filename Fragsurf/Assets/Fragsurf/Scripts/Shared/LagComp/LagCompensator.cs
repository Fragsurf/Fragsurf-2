using System;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;

namespace Fragsurf.Shared.LagComp
{
    public class LagCompensator : FSSharedScript
    {

        private GameState[] _gameStates = new GameState[128];
        private int _currentIndex = -1;
        private GameState _activeState;
        private bool _break;
        private int _snapshotNumber;
        private SimpleObjectPool<EntityState> _entStatePool = new SimpleObjectPool<EntityState>(32768, true);
        private bool _paused;

        public bool Enabled { get; set; } = true;

        protected override void _Initialize()
        {
            base._Initialize();

            DevConsole.RegisterVariable("net.testlag", "", () => _break, v => _break = v, this, ConVarFlags.Cheat);
        }

        protected override void OnPreGameUnloaded()
        {
            _paused = true;
            foreach (var gs in _gameStates)
            {
                if (gs == null)
                {
                    continue;
                }
                foreach (var es in gs.EntityStates)
                {
                    _entStatePool.Release(es);
                }
                gs.EntityStates.Clear();
            }
        }

        protected override void OnGameLoaded()
        {
            _paused = false;
        }

        protected override void _Tick()
        {
            if (!Enabled || _paused)
            {
                return;
            }

            // increment index until it reach max, then shift
            if (_currentIndex == _gameStates.Length - 1)
            {
                ShiftStateArrayLeft(0);
            }
            else
            {
                _currentIndex++;
            }

            Snapshot();
            _snapshotNumber++;
        }

        private void ShiftStateArrayLeft(int startIndex)
        {
            var first = _gameStates[0];
            for (int index = startIndex; index + 1 < _gameStates.Length; index++)
                _gameStates[index] = _gameStates[index + 1];
            _gameStates[_gameStates.Length - 1] = first;
        }

        public void Rewind(float latency)
        {
            if (!Enabled || _paused)
            {
                if (_activeState != null)
                {
                    Restore();
                }
                return;
            }

            if (_activeState != null)
            {
                Debug.LogError("Lag compensator was never restored!!");
                return;
            }

            latency += Game.EntityManager.InterpDelay;

            _activeState = GetState(Game.ElapsedTime - latency);

            if (_activeState != null)
            {
                Rewind(_activeState);
            }
        }

        public void Restore()
        {
            if (!Enabled || _paused)
            {
                return;
            }

            Restore(_activeState);
            _activeState = null;
        }

        private void Rewind(GameState gameState)
        {
            // cache current state, then rewind
            for (int i = 0; i < gameState.EntityStates.Count; i++)
            {
                var state = gameState.EntityStates[i];
                var ent = Game.EntityManager.FindEntity(state.EntityId);
                if (ent == null || ent.DisableLagCompensation || ent.EntityGameObject == null)
                {
                    continue;
                }

                state.CurrentOrigin = ent.EntityGameObject.Position;
                state.CurrentAngles = ent.EntityGameObject.Rotation;
                GetAnimState(ent.EntityGameObject, state.CurrentAnimState);
                gameState.EntityStates[i] = state;

                ent.EntityGameObject.Position = state.Origin;
                ent.EntityGameObject.Rotation = state.Angles;
                SetAnimatorState(ent.EntityGameObject, state.AnimState);
            }

            Physics.SyncTransforms();
        }

        private void Restore(GameState gameState)
        {
            // return to current state
            for (int i = 0; i < gameState.EntityStates.Count; i++)
            {
                var state = gameState.EntityStates[i];
                var ent = Game.EntityManager.FindEntity(state.EntityId);
                if (ent == null || ent.EntityGameObject == null || ent.DisableLagCompensation)
                {
                    continue;
                }

                ent.EntityGameObject.Position = state.CurrentOrigin;
                ent.EntityGameObject.Rotation = state.CurrentAngles;
                SetAnimatorState(ent.EntityGameObject, state.CurrentAnimState);
            }

            Physics.SyncTransforms();
        }

        private GameState GetStateByLatency(float latency)
        {
            for (int i = _gameStates.Length - 1; i >= 0; i--)
            {
                if (_gameStates[i].Time <= Game.ElapsedTime - latency)
                    return _gameStates[i];
            }
            return null;
        }

        private GameState GetState(double approximateTime)
        {
            for (int i = _gameStates.Length - 1; i >= 0; i--)
            {
                if (_gameStates[i].Time > approximateTime)
                {
                    continue;
                }
                if (i == _gameStates.Length - 1) return _gameStates[i];
                var choice1 = Math.Abs(_gameStates[i].Time - approximateTime);
                var choice2 = Math.Abs(_gameStates[i + 1].Time - approximateTime);
                return choice1 < choice2 ? _gameStates[i] : _gameStates[i + 1];
            }
            return null;
        }

        private void Snapshot()
        {
            var gameState = GetGameState();
            gameState.Time = Game.ElapsedTime;
            gameState.SnapshotNumber = _snapshotNumber;

            for (int i = 0; i < Game.EntityManager.Entities.Count; i++)
            {
                var entity = Game.EntityManager.Entities[i];

                if (entity == null
                    || entity.DisableLagCompensation
                    || !entity.EntityGameObject)
                {
                    continue;
                }

                var state = _entStatePool.Get();
                state.EntityId = entity.EntityId;
                state.Origin = entity.EntityGameObject.Position;
                state.Angles = entity.EntityGameObject.Rotation;
                GetAnimState(entity.EntityGameObject, state.AnimState);

                gameState.EntityStates.Add(state);
            }

            _gameStates[_currentIndex] = gameState;
        }

        private GameState GetGameState()
        {
            if (_gameStates[_currentIndex] == null)
                return new GameState();
            foreach (var s in _gameStates[_currentIndex].EntityStates)
            {
                _entStatePool.Release(s);
            }
            _gameStates[_currentIndex].EntityStates.Clear();
            return _gameStates[_currentIndex];
        }

        private void GetAnimState(EntityGameObject entObj, List<Vector3> state)
        {
            state.Clear();
            foreach (var hb in entObj.Hitboxes)
            {
                state.Add(hb.transform.position);
                state.Add(hb.transform.eulerAngles);
            }
        }

        private void SetAnimatorState(EntityGameObject entObj, List<Vector3> state)
        {
            var idx2 = 0;
            for (int i = 0; i < entObj.Hitboxes.Length; i++)
            {
                entObj.Hitboxes[i].transform.position = state[idx2];
                entObj.Hitboxes[i].transform.eulerAngles = state[idx2 + 1];
                idx2 += 2;
            }
        }

    }
}
