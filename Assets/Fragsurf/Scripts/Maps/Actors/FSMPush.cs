using UnityEngine;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Actors
{
    public class FSMPush : FSMTrigger
    {

        [Header("Push Options")]

        public float Speed;
        public Vector3 Direction;
        public bool Once;

        protected override void _TriggerEnter(NetEntity entity)
        {
            if(entity is Human hu 
                && hu.MovementController is ISurfControllable surfer)
            {
                if (surfer.MoveType == MoveType.Noclip || hu.Frozen)
                {
                    return;
                }

                if (Once)
                {
                    var pushAmount = Direction * Speed;
                    hu.BaseVelocity = pushAmount;
                }
            }
        }

        protected override void _TriggerStay(NetEntity entity)
        {
            if (entity is Human hu && hu.MovementController is ISurfControllable surfer)
            {
                if (Once || surfer.MoveType == MoveType.Noclip || hu.Frozen)
                {
                    return;
                }

                var vecPush = Direction.normalized * Speed;
                if (surfer.MoveData.Momentum && !surfer.GroundObject)
                {
                    vecPush += surfer.MoveData.BaseVelocity;
                }
                hu.BaseVelocity = vecPush;
                surfer.MoveData.Momentum = true;
            }
        }

        protected override void _OnDrawGizmos()
        {
            base._OnDrawGizmos();

            var collider = GetComponent<Collider>();
            if(collider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(collider.bounds.center, 0.5f);
                DebugDraw.GizmoArrow(collider.bounds.center, Direction * Speed);
            }
        }

    }
}
