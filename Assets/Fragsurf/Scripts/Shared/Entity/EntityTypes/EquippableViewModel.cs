using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class EquippableViewModel : MonoBehaviour
    {

        [SerializeField]
        private Animator _animator;

        public Animator Animator => _animator;
        public Camera Camera { get; private set; }

        private Vector3 _kick;
        private Vector3 _swayPosition;
        private Vector3 _originalPosition;
        private Vector3 _originalRotation;

        private void Awake()
        {
            if (!_animator)
            {
                _animator = GetComponentInChildren<Animator>();
            }

            if (_animator)
            {
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            Camera = GetComponentInChildren<Camera>();
            _originalPosition = _animator.transform.localPosition;
            _originalRotation = _animator.transform.localEulerAngles;
        }

        private void OnEnable()
        {
            _kick = Vector3.zero;
            _swayPosition = Vector3.zero;
            _animator.transform.localPosition = _originalPosition;
            _animator.transform.localEulerAngles = _originalRotation;
        }

        private void Update()
        {
            if (Camera && GameCamera.Instance)
            {
                Camera.fieldOfView = GameCamera.Instance.WeaponFieldOfView;
            }

            var cl = FSGameLoop.GetGameInstance(false);
            var sway = cl && cl.Get<SpectateController>().TargetHuman == Human.Local;
            if (sway)
            {
                // todo: more robust weapon sway
                var mdelta = new Vector3(-Input.GetAxis("Mouse X"), -Input.GetAxisRaw("Mouse Y"), 0) / 35f;
                _swayPosition += mdelta * Time.deltaTime;
                _swayPosition = Vector3.ClampMagnitude(_swayPosition, .06f);
                _swayPosition = Mathfx.Berp(_swayPosition, Vector3.zero, Time.deltaTime);
                _kick = Vector3.MoveTowards(_kick, Vector3.zero, 10f * Time.deltaTime);

                _animator.transform.localPosition = _originalPosition + _swayPosition + new Vector3(0, 0, _kick.z);
                _animator.transform.localEulerAngles = _originalRotation + new Vector3(_kick.x, 0, 0);
            }
        }

        public void Kick(float strength = 1f)
        {
            var kickAmt = new Vector3(-1.25f, 0, -.03f);
            _kick += kickAmt * strength;
        }

        public void PlayAnimation(string name, float crossfade = 0.08f)
        {
            if (!Animator)
            {
                return;
            }

            if (crossfade == 0)
            {
                Animator.Play(name);
            }
            else
            {
                Animator.CrossFadeInFixedTime(name, crossfade);
            }
        }

    }
}

