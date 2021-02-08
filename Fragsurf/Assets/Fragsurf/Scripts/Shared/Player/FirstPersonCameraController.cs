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

        public FirstPersonCameraController(NetEntity viewer, Camera camera)
            : base(viewer, camera)
        {

        }

        public override void Update()
        {
            if(Viewer == null || Camera == null)
            {
                Debug.Log(Viewer + ":" + Camera);
                return;
            }

            var targetOrigin = Viewer.EntityGameObject 
                ? Viewer.EntityGameObject.transform.position
                : Viewer.Origin;
            var targetAngles = Viewer.EntityGameObject
                ? Viewer.EntityGameObject.transform.eulerAngles
                : Viewer.Angles;

            _targetEyeOffset = Vector3.zero;

            if (Viewer is Human player)
            {
                _targetEyeOffset = player.HumanGameObject.EyeOffset;
                _targetEyeOffset.y += player.Ducked ? -.5f : 0;
                if (Viewer == Human.Local)
                {
                    targetAngles = Viewer.Angles;
                }
            }

            _eyeOffset = Vector3.SmoothDamp(_eyeOffset, _targetEyeOffset, ref _offsetVelocity, .07f);

            Camera.transform.position = targetOrigin + _eyeOffset;
            Camera.transform.eulerAngles = targetAngles;
        }
    }
}

