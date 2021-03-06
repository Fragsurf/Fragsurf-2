using Fragsurf.Client;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Client, typeof(Bunnyhop))]
    public class BunnyhopStrafeTrainer : FSClientScript
    {

        private int _tickInterval = 10;
        private int _realtimeSyncCount = 100;

        [ConVar("strafetrainer.enabled", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool Enabled { get; set; } = true;
        [ConVar("strafetrainer.interval", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public int TickInterval
        {
            get { return _tickInterval; }
            set { _tickInterval = Mathf.Clamp(value, 1, 20); }
        }
        [ConVar("strafetrainer.rtsyncbar", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool RealtimeSyncBar { get; set; }
        [ConVar("strafetrainer.rtsynccount", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public int RealtimeSyncCount
        {
            get => _realtimeSyncCount;
            set => _realtimeSyncCount = Mathf.Clamp(value, 1, 100);
        }

        private float _lastYaw;
        private int _tickCount;
        private int[] _percentages;
        private Modal_StrafeTrainer _modal;

        protected override void OnPlayerRunCommand(IPlayer player)
        {
            if (!_modal)
            {
                _modal = UGuiManager.Instance.Find<Modal_StrafeTrainer>();
                if (!_modal)
                {
                    return;
                }
            }

            if (!Enabled
                || !(player.Entity is Human hu)
                || !(hu.MovementController is DefaultMovementController move)
                || move.MoveType != MoveType.Walk)
            {
                if (_modal.IsOpen)
                {
                    _modal.Close();
                }
                return;
            }

            if (!_modal.IsOpen)
            {
                _modal.Open();
            }

            if(_percentages == null || _percentages.Length < TickInterval)
            {
                _percentages = new int[TickInterval];
            }

            //var perfectAngle = PerfectStrafeAngle(hu.HammerVelocity());
            var yawDiff = NormalizeAngle(_lastYaw - hu.Angles.y);
            var percentage = (int)(move.MoveData.GainCoefficient * 100);
            //var percentage = (int)(Mathf.Abs(yawDiff) / perfectAngle);

            var rtSync = 0;
            if(RealtimeSyncBar && hu.Timeline is BunnyhopTimeline bhop)
            {
                rtSync = bhop.GetRealtimeSync(RealtimeSyncCount);
            }

            if (_tickCount >= TickInterval)
            {
                var avgPercent = 0f;

                for (int i = 0; i < TickInterval; i++) // calculate average from the last ticks
                {
                    avgPercent += _percentages[i];
                    _percentages[i] = 0;
                }

                _modal.SetPercent((int)(avgPercent / TickInterval), rtSync, yawDiff <= 0);
                _tickCount = 0;
            }
            else
            {
                _percentages[_tickCount] = percentage;
                _tickCount++;
            }

            _lastYaw = hu.Angles.y;
        }

        float NormalizeAngle(float angle)
        {
            float newAngle = angle;
            while (newAngle <= -180.0) newAngle += 360f;
            while (newAngle > 180.0) newAngle -= 360f;
            return newAngle;
        }

        float PerfectStrafeAngle(float speed)
        {
            return Mathf.Rad2Deg * Mathf.Atan(Game.GameMovement.Config.AirCap / speed);
        }

    }
}

