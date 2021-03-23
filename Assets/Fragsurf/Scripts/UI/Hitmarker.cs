using Fragsurf.Shared;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    [RequireComponent(typeof(GameAudioSource))]
    public class Hitmarker : MonoBehaviour
    {

        [SerializeField]
        private GameAudioSource _audioSrc;
        [SerializeField]
        private AudioClip _alternativeClip;
        [SerializeField]
        private Color _alternativeColor;
        [SerializeField]
        private Image _hitmarker;
        [SerializeField]
        private float _fadeOutSpeed = 2f;

        private Color _originalColor;

        private void Awake()
        {
            _audioSrc = GetComponent<GameAudioSource>();
            _originalColor = _hitmarker.color;
            _hitmarker.color = new Color(0, 0, 0, 0);
        }

        private void Update()
        {
            if(_hitmarker.color.a > 0)
            {
                _hitmarker.color = Color.Lerp(_hitmarker.color, new Color(0, 0, 0, 0), _fadeOutSpeed * Time.deltaTime);
            }
        }

        public void Trigger()
        {
            if (_audioSrc)
            {
                _audioSrc.Play(true);
            }
            _hitmarker.color = _originalColor;
        }

        public void Trigger2()
        {
            if (_audioSrc)
            {
                _audioSrc.PlayClip(_alternativeClip, 1f, true);
            }
            _hitmarker.color = _alternativeColor;
        }

    }
}

