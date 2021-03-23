using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class FirstPersonCameraController : CameraController
    {

        private Vector3 _eyeOffset;
        private Vector3 _targetEyeOffset;
        private Vector3 _offsetVelocity;
        private float _defaultFov;

        protected override bool HideViewer => true;

        public FirstPersonCameraController(NetEntity viewer)
            : base(viewer)
        {

        }

        protected override void OnActivate()
        {
            _defaultFov = Camera.fieldOfView;
        }

        public override void Update()
        {
            if(Viewer == null || Camera == null)
            {
                return;
            }

            if(Viewer is Human hu && hu.Dead)
            {
                var targetPos = hu.HumanGameObject && hu.HumanGameObject.Ragdoll
                    ? hu.HumanGameObject.Ragdoll.Position
                    : hu.HumanGameObject != null ? hu.HumanGameObject.transform.position : hu.Origin;
                var targetRotation = Quaternion.LookRotation(targetPos - Camera.transform.position);
                // Smoothly rotate towards the target point.
                Camera.transform.rotation = Quaternion.Slerp(Camera.transform.rotation, targetRotation, .7f * Time.deltaTime);
                targetPos -= Camera.transform.forward * 5f;
                Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, targetPos, .25f * Time.deltaTime);
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

                var magnification = 1f;
                if(human.Equippables.Equipped != null
                    && human.Equippables.Equipped.EquippableGameObject is GunEquippable gun)
                {
                    magnification = gun.GetMagnification();
                }

                var diff = _defaultFov * magnification - _defaultFov;
                var fov = Mathf.Clamp(_defaultFov - diff, 1, 100);
                SensitivityModifier = fov / _defaultFov;
                Camera.fieldOfView = fov;
            }

            _eyeOffset = Vector3.SmoothDamp(_eyeOffset, _targetEyeOffset, ref _offsetVelocity, .07f);

            Camera.transform.position = targetOrigin + _eyeOffset;
            Camera.transform.eulerAngles = targetAngles;
        }
    }
}

