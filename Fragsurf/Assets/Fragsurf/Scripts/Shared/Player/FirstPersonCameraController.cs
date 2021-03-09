using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class FirstPersonCameraController : CameraController
    {

        private Vector3 _eyeOffset;
        private Vector3 _targetEyeOffset;
        private Vector3 _offsetVelocity;

        protected override bool HideViewer => true;

        public FirstPersonCameraController(NetEntity viewer)
            : base(viewer)
        {

        }

        public override void Update()
        {
            if(Viewer == null || Camera == null)
            {
                return;
            }

            var targetOrigin = Viewer.EntityGameObject 
                ? Viewer.EntityGameObject.Position
                : Viewer.Origin;
            var targetAngles = Viewer.EntityGameObject
                ? Viewer.EntityGameObject.Rotation
                : Viewer.Angles;

            _targetEyeOffset = Vector3.zero;

            if (Viewer is Human human)
            {
                _targetEyeOffset = human.Ducked 
                    ? human.HumanGameObject.DuckedEyeOffset
                    : human.HumanGameObject.EyeOffset;
                if (Viewer == Human.Local)
                {
                    targetAngles = Viewer.Angles;
                }
                targetAngles += human.TotalViewPunch();
            }

            _eyeOffset = Vector3.SmoothDamp(_eyeOffset, _targetEyeOffset, ref _offsetVelocity, .07f);

            Camera.transform.position = targetOrigin + _eyeOffset;
            Camera.transform.eulerAngles = targetAngles;
        }
    }
}

