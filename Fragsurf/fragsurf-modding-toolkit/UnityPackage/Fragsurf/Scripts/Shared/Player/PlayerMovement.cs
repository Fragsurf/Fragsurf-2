using UnityEngine;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Entity;
using Fragsurf.Movement;

namespace Fragsurf.Shared.Player
{
    //public class PlayerMovement : ISurfControllable
    //{
    //    public PlayerMovement(Human human)
    //    {
    //        _human = human;
    //        MoveData = new MoveData();
    //    }
    //    private Human _human;

    //    public static float FallDamageVelocity = -12;

    //    #region ISurfControllable

    //    public Quaternion Orientation => Quaternion.identity;
    //    public Vector3 Forward { get; private set; }
    //    public Vector3 Right { get; private set; }
    //    public Vector3 Up { get; private set; }
    //    public GameObject GroundObject { get; set; }
    //    public Vector3 StandingExtents { get; set; }
    //    public BoxCollider Collider => _human.HumanGameObject.BoundsCollider;
    //    public MoveType MoveType { get; set; } = MoveType.Walk;
    //    public MoveData MoveData { get; }

    //    #endregion

    //    public void RunCommand(UserCmd.CmdFields userCmd)
    //    {
    //        MoveData.Buttons = userCmd.Buttons;
    //        MoveData.ViewAngles = userCmd.Angles;
    //        MoveData.ForwardMove = userCmd.ForwardMove;
    //        MoveData.SideMove = userCmd.SideMove;
    //        MoveData.UpMove = userCmd.UpMove;

    //        ProcessMovement();

    //        MoveData.OldButtons = MoveData.Buttons;
    //    }

    //    private void ProcessMovement()
    //    {
    //        if (_human.Dead)
    //        {
    //            return;
    //        }

    //        var wasGrounded = GroundObject != null;
    //        var prevVelocity = MoveData.Velocity;

    //        Right = _human.Right;
    //        Forward = _human.YawForward;
    //        Up = _human.Up;

    //        _human.Game.GameMovement.Move(_human);
    //        _human.Game.Physics.DetectTouchingTriggers(_human);

    //        // only update the entity if its predicted (client player) or host
    //        if (_human.Game.IsHost || _human.IsLocalOwner)
    //        {
    //            _human.Velocity = MoveData.Velocity;
    //            _human.BaseVelocity = MoveData.BaseVelocity;
    //            _human.Origin = MoveData.Origin;
    //        }

    //        if (!MoveData.LimitedExecution)
    //        {
    //            // fall damage/viewpunch
    //            if (GroundObject != null
    //                && !wasGrounded
    //                && prevVelocity.y <= FallDamageVelocity)
    //            {
    //                if (_human.Movement.MoveData.InWater && _human.Game.GameMovement.Config.WaterStopsFallDamage)
    //                {
    //                    return;
    //                }
    //                var damage = CalculateFallDamage(prevVelocity.y);
    //                var viewPunch = new Vector3(0, -prevVelocity.y, prevVelocity.y).normalized;
    //                _human.Punch(viewPunch, Vector3.zero);
    //                _human.Damage(damage, DamageType.Fall);
    //            }
    //        }
    //    }

    //    private int CalculateFallDamage(float yVelocity)
    //    {
    //        yVelocity *= -1;
    //        int result = (int)yVelocity;

    //        if (yVelocity < 20)
    //            return result;

    //        if (yVelocity < 30)
    //            return (int)(result * 2.75f);

    //        if (yVelocity < 40)
    //            return (int)(result * 8f);

    //        return result * 5;
    //    }

    //}
}