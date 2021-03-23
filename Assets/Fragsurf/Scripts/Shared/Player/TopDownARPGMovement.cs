using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using UnityEngine;
using UnityEngine.AI;

namespace Fragsurf.Shared.Player
{
    public class TopDownARPGMovement : MovementController
    {

        public override bool ShowsCursor => true;
        public override bool MouseControlsRotation => false;

        private NavMeshPath _path = new NavMeshPath();
        private int _currentWaypoint;

        public TopDownARPGMovement(Human human)
            : base(human)
        {
            var groundRay = new Ray(human.Origin, Vector3.down);
            if (Physics.Raycast(groundRay, out RaycastHit hit, 10, 1 << 0))
            {
                Human.Origin = GetPointOnMesh(hit.point);
            }
        }

        private Vector3 GetPointOnMesh(Vector3 input)
        {
            if(NavMesh.SamplePosition(input, out NavMeshHit hit, 1f, 1 << 0))
            {
                return hit.position;
            }
            return input;
        }

        public override void ExecuteMovement(UserCmd.CmdFields cmd)
        {
            var moving = false;
            var curPos = GetPointOnMesh(Human.Origin);

            if (cmd.Buttons.HasFlag(Movement.InputActions.HandAction))
            {
                _currentWaypoint = 1;
                NavMesh.CalculatePath(curPos, cmd.MousePosition, 1 << 0, _path);
            }

            if(_path.status != NavMeshPathStatus.PathInvalid
                && _currentWaypoint < _path.corners.Length)
            {
                var dir = (_path.corners[_currentWaypoint] - curPos).normalized;
                var moveVector = dir * 5.5f * Time.fixedDeltaTime;
                moving = true;
                curPos += moveVector;

                Human.Origin = curPos;
                Human.Angles = Quaternion.LookRotation(dir).eulerAngles;

                var dist = Vector3.Distance(curPos, _path.corners[_currentWaypoint]);
                if (dist <= .1f)
                {
                    _currentWaypoint++;
                }
            }

            Human.Velocity = moving 
                ? Human.HumanGameObject.ViewBody.transform.forward * 10
                : Vector3.zero;
        }

    }
}

