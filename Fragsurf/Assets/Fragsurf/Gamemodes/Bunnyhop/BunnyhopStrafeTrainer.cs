using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    [Inject(InjectRealm.Client, typeof(Bunnyhop))]
    public class BunnyhopStrafeTrainer : FSClientScript
    {

        private int _tickInterval = 10;

        [ConVar("strafetrainer.enabled", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool Enabled { get; set; } = true;
        [ConVar("strafetrainer.interval", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public int TickInterval
        {
            get { return _tickInterval; }
            set { _tickInterval = Mathf.Clamp(value, 1, 20); }
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
                || move.GroundObject
                || move.MoveType != Movement.MoveType.Walk)
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

            if(_tickCount >= TickInterval)
            {
                var avgPercent = 0f;

                for (int i = 0; i < TickInterval; i++) // calculate average from the last ticks
                {
                    avgPercent += _percentages[i];
                    _percentages[i] = 0;
                }

                _modal.SetPercent((int)(avgPercent / TickInterval), yawDiff <= 0);
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

