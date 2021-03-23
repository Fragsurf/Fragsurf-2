using UnityEngine;
using Fragsurf.Movement;
using System.Linq;
using Fragsurf.Shared.Entity;
using UnityEngine.Serialization;

namespace Fragsurf.Actors
{
    public class FSMTeleport : FSMTrigger
    {

        [Header("Teleport Options")]

        public string TargetActor;
        public bool MaintainAngles;
        public bool ClearVelocity;

        protected override void _TriggerEnter(NetEntity entity)
        {
            if (!(entity is Human hu)
                || !(hu.MovementController is ISurfControllable character)
                || character.MoveType == MoveType.Noclip)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(TargetActor))
            {
                // todo: cache actors for efficient lookup
                var target = FindObjectsOfType<FSMActor>().FirstOrDefault(x => x.ActorName.Equals(TargetActor, System.StringComparison.OrdinalIgnoreCase));
                if (target)
                {
                    character.MoveData.Origin = target.transform.position;
                    if (!MaintainAngles)
                    {
                        character.MoveData.ViewAngles = target.transform.rotation.eulerAngles;
                    }
                }
            }

            if(ClearVelocity)
            {
                character.MoveData.Velocity = Vector3.zero;

                if (!MaintainAngles)
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
            if (!string.IsNullOrWhiteSpace(TargetActor))
            {
                var target = GameObject.FindObjectsOfType<FSMActor>().FirstOrDefault(x => string.Equals(x.ActorName, TargetActor, System.StringComparison.OrdinalIgnoreCase));
                if (target)
                {
                    targetPoint = target.transform.position;
                }
            }
            Gizmos.DrawLine(transform.position, targetPoint);
        }

    }
}
