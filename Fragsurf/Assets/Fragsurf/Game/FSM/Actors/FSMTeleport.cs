using UnityEngine;
using Fragsurf.Movement;
using System.Linq;
using Fragsurf.Shared.Entity;

namespace Fragsurf.FSM.Actors
{
    public class FSMTeleport : FSMTrigger
    {

        [Header("Teleport Options")]

        public string TargetActorName;
        public bool DontRotate;
        public bool SetVelocityToZero;

        protected override void _TriggerEnter(NetEntity entity)
        {
            if (!(entity is Human hu)
                || !(hu.MovementController is ISurfControllable character)
                || character.MoveType == MoveType.Noclip)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(TargetActorName))
            {
                // todo: cache actors for efficient lookup
                var target = FindObjectsOfType<FSMActor>().FirstOrDefault(x => x.ActorName.Equals(TargetActorName));
                if (target)
                {
                    character.MoveData.Origin = target.transform.position;
                    if (!DontRotate)
                    {
                        character.MoveData.ViewAngles = target.transform.rotation.eulerAngles;
                    }
                }
            }

            if(SetVelocityToZero)
            {
                character.MoveData.Velocity = Vector3.zero;

                if (!DontRotate)
                {
                    character.MoveData.BaseVelocity = Vector3.zero;
                }
            }
        }

        protected override void _OnDrawGizmos()
        {
            base._OnDrawGizmos();

            Gizmos.color = Color.white;
            var targetPoint = transform.position;
            if (!string.IsNullOrWhiteSpace(TargetActorName))
            {
                var target = GameObject.FindObjectsOfType<FSMActor>().FirstOrDefault(x => string.Equals(x.ActorName, TargetActorName, System.StringComparison.OrdinalIgnoreCase));
                if (target)
                {
                    targetPoint = target.transform.position;
                }
            }
            Gizmos.DrawLine(transform.position, targetPoint);
        }

    }
}
