using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;
using Fragsurf.Movement;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_push")]
    public class BspTriggerPush : BspTrigger<TriggerPush>
    {

        private bool _once;
        private Vector3 _direction;
        private float _speed;

        protected override void OnStart()
        {
            //GameObject.Destroy(GetComponent<FSMTrigger>());
            //var push = gameObject.AddComponent<FSMPush>();
            _speed = Entity.PushSpeed * BspToUnity.Options.WorldScale;
            _direction = Entity.PushDirection.TOUDirection().normalized;
        }

        protected override void OnStartTouch(NetEntity entity)
        {
            if (entity is Human hu
                && hu.MovementController is ISurfControllable surfer)
            {
                if (surfer.MoveType == MoveType.Noclip || hu.Frozen)
                {
                    return;
                }

                if (_once)
                {
                    var pushAmount = _direction.normalized * _speed;
                    hu.BaseVelocity = pushAmount;
                }
            }
        }

        protected override void OnTouch(NetEntity entity)
        {
            if (entity is Human hu && hu.MovementController is ISurfControllable surfer)
            {
                if (_once || surfer.MoveType == MoveType.Noclip || hu.Frozen)
                {
                    return;
                }

                var vecPush = _direction.normalized * _speed;
                if (surfer.MoveData.Momentum && !surfer.GroundObject)
                {
                    vecPush += hu.BaseVelocity;
                }
                hu.BaseVelocity = vecPush;
                surfer.MoveData.Momentum = true;
            }
        }

    }
}

