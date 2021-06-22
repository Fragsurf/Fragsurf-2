using Fragsurf.Actors;
using Fragsurf.Movement;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Fragsurf.Gamemodes.Bunnyhop 
{
    /// <summary>
    /// Credits: https://github.com/neko-pm/ssj/blob/master/scripting/ssj.sp
    /// </summary>
    [Inject(InjectRealm.Client, typeof(Bunnyhop))]
    public class BunnyhopSSJ : FSSharedScript
    {

        [ConVar("ssj.mode", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public SSJMode Mode { get; set; } = SSJMode.SixthJump;
        [ConVar("ssj.currentspeed", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool CurrentSpeed { get; set; } = true;
        [ConVar("ssj.speeddiff", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool SpeedDiff { get; set; } = true;
        [ConVar("ssj.heightdiff", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool HeightDiff { get; set; } = true;
        [ConVar("ssj.gainstats", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool GainStats { get; set; } = true;
        [ConVar("ssj.efficiency", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool Efficiency { get; set; } = true;
        [ConVar("ssj.strafesync", "", ConVarFlags.UserSetting | ConVarFlags.Gamemode)]
        public bool StrafeSync { get; set; } = true;

        private const int BhopTime = 15;

        private Dictionary<int, SSJData> _ssjDatas = new Dictionary<int, SSJData>();

        protected override void OnPlayerDisconnected(BasePlayer player)
        {
            _ssjDatas.Remove(player.ClientIndex);
        }

        protected override void OnPlayerRunCommand(BasePlayer player)
        {
            if(!(player.Entity is Human hu)
                || !(hu.Timeline is BunnyhopTimeline bhop)
                || !(hu.MovementController is CSMovementController move))
            {
                return;
            }

            if (!_ssjDatas.ContainsKey(player.ClientIndex))
            {
                _ssjDatas[player.ClientIndex] = new SSJData();
            }

            var data = _ssjDatas[player.ClientIndex];
            var touchingWall = IsTouchingWall(hu);

            if (move.GroundObject != null)
            {
                if(data.TicksOnGround > BhopTime)
                {
                    data.Jump = 0;
                    data.StrafeTick = 0;
                    data.SyncedTick = 0;
                    data.RawGain = 0;
                    data.Trajectory = 0;
                    data.TraveledDistance = Vector3.zero;
                }
                data.TicksOnGround++;
                if(move.MoveData.Buttons.HasFlag(InputActions.Jump) && data.TicksOnGround == 1)
                {
                    //if (bhop.InStartZone)
                    //{
                    //    data.Jump = 0;
                    //    data.StrafeTick = 0;
                    //    data.SyncedTick = 0;
                    //    data.RawGain = 0;
                    //    data.Trajectory = 0;
                    //    data.TraveledDistance = Vector3.zero;
                    //}
                    GetStats(bhop, data, hu, move, touchingWall);
                    data.TicksOnGround = 0;
                }
            }
            else
            {
                if(move.MoveType != MoveType.Noclip 
                    && move.MoveType != MoveType.Ladder 
                    && move.MoveType != MoveType.None
                    && move.MoveData.WaterDepth < Game.GameMovement.Config.WaterDepthToSwim)
                {
                    GetStats(bhop, data, hu, move, touchingWall);
                }
                data.TicksOnGround = 0;
            }

            if (touchingWall)
            {
                data.TouchTicks++;
            }
            else
            {
                data.TouchTicks = 0;
            }

            //if (bhop.LastFrame.Jumps == 6 && Game.Get<SpectateController>().TargetHuman == hu)
            //{
            //    Game.TextChat.PrintChat("[Timer]", $"SSJ: <color=yellow>{hu.HammerVelocity()}</color>");
            //}
        }

        private bool IsTouchingWall(Human hu)
        {
            return false;
        }

        private void GetStats(BunnyhopTimeline tl, SSJData data, Human hu, CSMovementController move, bool touchingWall)
        {
            var velocity = hu.Velocity;
            velocity.y = 0;
            data.StrafeTick++;
            data.RawGain += move.MoveData.GainCoefficient;
            data.SyncedTick += move.MoveData.GainCoefficient > 0 ? 1 : 0;
            data.Trajectory += velocity.magnitude * Time.fixedDeltaTime;
            data.TraveledDistance.x += velocity.x * Time.fixedDeltaTime;
            data.TraveledDistance.z += velocity.z * Time.fixedDeltaTime;

            if (move.MoveData.JustJumped)
            {
                if(data.Jump > 0 && data.StrafeTick <= 0)
                {
                    return;
                }

                data.Jump++;
                var print = Mode == SSJMode.EveryJump
                        || Mode == SSJMode.SixthJump && data.Jump == 6
                        || Mode == SSJMode.EverySixthJump && data.Jump % 6 == 0;

                if (print)
                {
                    if (IsFirstPerson(hu))
                    {
                        PrintStats(hu, data);
                    }

                    data.RawGain = 0f;
                    data.StrafeTick = 0;
                    data.SyncedTick = 0;
                    data.OldHeight = 0;
                    data.OldSpeed = 0;
                    data.Trajectory = 0;
                    data.TraveledDistance = Vector3.zero;
                }

                if ((data.Jump == 1 && Mode == SSJMode.SixthJump) 
                    || ((data.Jump % 6 == 1) && Mode == SSJMode.EverySixthJump)
                    || Mode == SSJMode.EveryJump)
                {
                    data.InitialHeight = hu.Origin.y;
                    data.InitialSpeed = velocity.magnitude;
                    data.TraveledDistance = Vector3.zero;
                }
            }
        }

        private static StringBuilder _sb = new StringBuilder(256);
        private void PrintStats(Human hu, SSJData data)
        {
            var coeffsum = data.RawGain;
            coeffsum /= data.StrafeTick;
            coeffsum *= 100f;

            var efficiency = 0f;
            var distance = data.TraveledDistance.magnitude;
            if (distance > data.Trajectory) distance = data.Trajectory;
            if(distance > 0)
            {
                efficiency = coeffsum * (distance / data.Trajectory);
            }

            coeffsum = Mathf.Floor(coeffsum * 100f + 0.5f) / 100f;
            efficiency = Mathf.Floor(efficiency * 100f + 0.5f) / 100f;

            var tracks = Game.Get<BunnyhopTracks>();

            _sb.Clear();
            _sb.AppendFormat("Jump: <color={0}>{1}</color>", tracks.MiscColor.HashRGBA(), data.Jump);

            if((Mode == SSJMode.SixthJump && data.Jump == 6) 
                || (Mode == SSJMode.EverySixthJump && data.Jump % 6 == 0)
                || Mode == SSJMode.EveryJump)
            {
                var vel = hu.Velocity;
                vel.y = 0;
                var velocity = vel.magnitude;

                if (CurrentSpeed)
                {
                    var curSpd = (int)(velocity / .0254f);
                    _sb.AppendFormat(" | Speed: <color={0}>{1}u/s</color>", tracks.SpeedColor.HashRGBA(), curSpd);
                }

                if(Mode != SSJMode.EveryJump
                    || (Mode == SSJMode.EveryJump && data.Jump > 1))
                {
                    if (SpeedDiff)
                    {
                        var spdDiff = (int)((velocity - data.InitialSpeed) / .0254f);
                        _sb.AppendFormat(" | Speed ^: <color={0}>{1}u/s</color>", tracks.SpeedColor.HashRGBA(), spdDiff);
                    }
                    if (HeightDiff)
                    {
                        var heightDiff = (int)((hu.Origin.y - data.InitialHeight) / .0254f);
                        _sb.AppendFormat(" | Height ^: <color={0}>{1}</color>", tracks.MiscColor.HashRGBA(), heightDiff);
                    }
                    if (GainStats)
                    {
                        _sb.AppendFormat(" | Gain: <color={0}>{1}%</color>", tracks.MiscColor.HashRGBA(), coeffsum.ToString("0"));
                    }
                    if (StrafeSync)
                    {
                        var ss = 100f * ((float)data.SyncedTick / data.StrafeTick);
                        _sb.AppendFormat(" | Sync: <color={0}>{1}%</color>", tracks.MiscColor.HashRGBA(), ss.ToString("0"));
                    }
                    if (Efficiency)
                    {
                        _sb.AppendFormat(" | Efficiency: <color={0}>{1}%</color>", tracks.MiscColor.HashRGBA(), efficiency.ToString("0"));
                    }

                    Game.Get<TextChat>().PrintChat("[SSJ]", $"<color={tracks.MessageColor.HashRGBA()}>{_sb}</color>");
                }
            }
        }

        private bool IsFirstPerson(Human hu)
        {
            return Game.Get<SpectateController>().TargetHuman == hu;
        }



        public enum SSJMode
        {
            Off,
            SixthJump,
            EveryJump,
            EverySixthJump
        }

        public class SSJData
        {
            public int TicksOnGround;
            public int TouchTicks;
            public int StrafeTick;
            public int SyncedTick;
            public int Jump;
            public float InitialSpeed;
            public float InitialHeight;
            public float OldHeight;
            public float OldSpeed;
            public float RawGain;
            public float Trajectory;
            public Vector3 TraveledDistance;
            public bool TouchingWall;
        }

    }
}


