using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_StrafeTrainer : UGuiModal
    {

        [SerializeField]
        private Image _leftFill;
        [SerializeField]
        private Image _rightFill;
        [SerializeField]
        private Image _syncFill;
        [SerializeField]
        private Image _syncRoot;
        [SerializeField]
        private TMP_Text _syncNumber;

        private float _leftFillAmount;
        private float _rightFillAmount;
        private float _syncFillAmount;
        private Color _leftFillColor;
        private Color _rightFillColor;

        private bool _right;
        private float _damageFillAmount;

        protected override void OnOpen()
        {
            _leftFill.fillAmount = 0;
            _rightFill.fillAmount = 0;
        }

        public void Update()
        {
            var spd = 64 * Time.deltaTime;
            _leftFill.fillAmount = Mathf.Lerp(_leftFill.fillAmount, _leftFillAmount, spd);
            _rightFill.fillAmount = Mathf.Lerp(_rightFill.fillAmount, _rightFillAmount, spd);
            _leftFill.color = Color.Lerp(_leftFill.color, _leftFillColor, spd);
            _rightFill.color = Color.Lerp(_rightFill.color, _rightFillColor, spd);

            if(_right && _leftFillAmount == 0)
            {
                _leftFill.fillAmount = 0;
            }
            else if(!_right && _rightFillAmount == 0)
            {
                _rightFill.fillAmount = 0;
            }

            _syncFill.fillAmount = Mathf.Lerp(_syncFill.fillAmount, _syncFillAmount, spd);
        }

        public void SetPercent(int gainPercent, int syncPercent, bool right)
        {
            if(syncPercent == 0)
            {
                _syncRoot.gameObject.SetActive(false);
                _syncFill.fillAmount = 0;
            }
            else
            {
                _syncRoot.gameObject.SetActive(true);
                _syncFillAmount = syncPercent / 100f;
                _syncNumber.text = $"{syncPercent}%";
            }
            //_syncFill.color = syncPercent < 40 ? Color.red : Color.yellow;

            //if(syncPercent >= 70)
            //{
            //    _syncFill.color = Color.cyan;
            //}
            //else if (syncPercent >= 80)
            //{
            //    _syncFill.color = Color.green;
            //}
            //else if (syncPercent > 92)
            //{
            //    _syncFill.color = Color.magenta;
            //}

            var fill = gainPercent / 100f;

            if(right != _right)
            {
                _damageFillAmount = 0;
                _right = right;
            }

            if(fill <= 0)
            {
                _damageFillAmount = Mathf.Max(Mathf.Abs(fill), _damageFillAmount);
            }

            if (right)
            {
                _rightFillAmount = fill;
                _rightFillColor = fill > 1f
                    ? Color.red
                    : (fill > .65f ? Color.green : Color.yellow);
                _leftFillAmount = /*fill <= 0 ? Mathf.Abs(fill) : 0f*/_damageFillAmount;
                _leftFillColor = Color.red;
            }
            else
            {
                _leftFillAmount = fill;
                _leftFillColor = fill > 1f
                    ? Color.red
                    : (fill > .65f ? Color.green : Color.yellow);
                _rightFillAmount = /*fill <= 0 ? Mathf.Abs(fill) : 0f*/_damageFillAmount;
                _rightFillColor = Color.red;
            }
        }

    }
}

