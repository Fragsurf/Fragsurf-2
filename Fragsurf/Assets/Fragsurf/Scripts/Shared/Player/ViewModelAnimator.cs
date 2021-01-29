using Fragsurf.UI;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class ViewModelAnimator : MonoBehaviour
    {
        public Camera ViewCamera;
        public Vector3 HipPosition;
        public Quaternion HipRotation;
        public Vector3 ADSPosition;
        public Quaternion ADSRotation;
        public Transform PositionTarget;
        public Vector3 Aimpunch;

        private float _swaySpeed = .02f;
        private float _maxSway = .018f;
        private float _swayRecovery = 6f;

        private bool _ads;
        private float _adsSpeed = 0.8f;
        private Vector3 _swayOffset;

        public Animator Animator { get; private set; }

        void Awake()
        {
            ViewCamera = GetComponentInChildren<Camera>();
            Animator = GetComponentInChildren<Animator>();

            if (Animator == null)
            {
                Debug.LogError("ViewModel is missing an animator!", gameObject);
            }
            else
            {
                Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                if (PositionTarget == null)
                {
                    PositionTarget = Animator.transform;
                }
            }
        }

        private void Update()
        {
            WeaponSway();
            AimDownSight();

            //if (ViewCamera)
            //{
            //    ViewCamera.fieldOfView = Camera.main.fieldOfView;
            //}
        }

        private void WeaponSway()
        {
            if(GameDocumentManager.Instance.HasCursor())
            {
                return;
            }

            _swayOffset = Vector3.Lerp(_swayOffset, Vector3.zero, _swayRecovery * Time.deltaTime);

            float factorX = -(Input.GetAxisRaw("Mouse X")) * _swaySpeed * Time.deltaTime;
            _swayOffset += new Vector3(factorX, 0, 0);
            _swayOffset = Vector3.ClampMagnitude(_swayOffset, _maxSway);
        }

        private void AimDownSight()
        {
            var posTarget = _ads ? ADSPosition : HipPosition;
            var rotTarget = _ads ? ADSRotation : HipRotation;
            var step = _adsSpeed * Time.deltaTime;

            PositionTarget.localPosition = Vector3.MoveTowards(PositionTarget.localPosition, posTarget + _swayOffset, step);
            PositionTarget.localRotation = Quaternion.RotateTowards(PositionTarget.localRotation, rotTarget, step * 24);
        }

        public void SetADS(bool ads, float adsSpeed = 0.8f)
        {
            if (_ads == ads)
            {
                return;
            }
            _ads = ads;
            _adsSpeed = adsSpeed;
        }

        public bool IsPlaying(string name)
        {
            return Animator.GetCurrentAnimatorStateInfo(0).IsName(name);
        }

        public float AnimationTime()
        {
            return Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }

        public void PlayAnimation(string name, float crossfade = 0.08f)
        {
            if (!gameObject.activeSelf || !Animator || !Animator.gameObject.activeSelf)
            {
                return;
            }
            if(crossfade == 0)
            {
                Animator.Play(name);
            }
            else
            {
                Animator.CrossFadeInFixedTime(name, crossfade);
            }
        }

        public void Equip()
        {
            Animator.Play("Equip");
        }

        public void Unequip()
        {
            Animator.Play("Unequip");
        }

    }

}
