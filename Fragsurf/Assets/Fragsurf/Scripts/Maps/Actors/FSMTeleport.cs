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
            if (!(entity is Human hu))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(TargetActor))
            {
                // todo: cache actors for efficient lookup
                var target = FindObjectsOfType<FSMActor>().FirstOrDefault(x => x.ActorName.Equals(TargetActor, System.StringComparison.OrdinalIgnoreCase));
                if (target)
                {
                    entity.Origin = target.transform.position;

                    if (!MaintainAngles)
                    {
                        entity.Angles = target.transform.rotation.eulerAngles;
                    }

                    if (ClearVelocity)
                    {
                        hu.Velocity = Vector3.zero;
                    }
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
