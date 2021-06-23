using Fragsurf.Shared.Entity;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class FirstPersonCameraController : CameraController
    {

        private Vector3 _eyeOffset;
        private Vector3 _targetEyeOffset;
        private Vector3 _offsetVelocity;

        public bool HeadBob { get; set; }
        protected override bool HideViewer => true;

        public FirstPersonCameraController(NetEntity viewer)
            : base(viewer)
        {

        }

        public override void Update()
        {
            if (Viewer == null || Camera == null)
            {
                return;
            }

            if (Viewer is Human hu && hu.Dead)
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
                if (human.Equippables.Equipped != null
                    && human.Equippables.Equipped.EquippableGameObject is GunEquippable gun)
                {
                    magnification = gun.GetMagnification();
                }

                var defFov = GameCamera.Instance.FieldOfView;
                var diff = defFov * magnification - defFov;
                var fov = Mathf.Clamp(defFov - diff, 1, 100);
                SensitivityModifier = fov / defFov;
                Camera.fieldOfView = fov;

                if(!human.Game.IsHost 
                    && HeadBob
                    && DevConsole.GetVariable<bool>("cam.bob"))
                {
                    var sprinting = human.MovementController.Sprinting;
                    var bobSpeed = sprinting ? HeadBobSprintSpeed : HeadBobSpeed;
                    var bobAmount = sprinting ? HeadBobSprintAmount : HeadBobAmount;

                    if (human.MovementController.Grounded && human.Velocity.magnitude > 0)
                    {
                        _bobTimer += Time.deltaTime * bobSpeed;
                        _bobPos = new Vector2(Mathf.Sin(_bobTimer) * bobAmount * .2f, Mathf.Sin(_bobTimer) * bobAmount);
                    }
                    else
                    {
                        _bobTimer = 0;
                        _bobPos = Vector2.Lerp(_bobPos, Vector2.zero, Time.deltaTime * bobSpeed);
                    }

                    targetOrigin += new Vector3(_bobPos.x, _bobPos.y, 0);

                    if (human.MovementController.JustGrounded)
                    {
                        var fallAmt = Mathf.Lerp(0, 8, Mathf.Abs(_prevVelocity.y) / 20);
                        human.Punch(new Vector3(fallAmt, 0, 0), Vector3.zero);
                    }
                    //else if (human.MovementController.JustJumped)
                    //{
                    //    human.Punch(new Vector3(-3, 0, 0), Vector3.zero);
                    //}

                    _prevVelocity = human.Velocity;
                }
            }

            _eyeOffset = Vector3.SmoothDamp(_eyeOffset, _targetEyeOffset, ref _offsetVelocity, .07f);

            Camera.transform.position = targetOrigin + _eyeOffset;
            Camera.transform.eulerAngles = targetAngles;
        }

        private Vector3 _prevVelocity;
        private float _bobTimer = 0;
        private Vector2 _bobPos;
        public float HeadBobSpeed = 10f;
        public float HeadBobSprintSpeed = 15f;
        public float HeadBobAmount = 0.06f;
        public float HeadBobSprintAmount = 0.1f;


    }
}

