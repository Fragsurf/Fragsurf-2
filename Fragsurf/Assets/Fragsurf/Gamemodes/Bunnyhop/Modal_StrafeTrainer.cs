using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
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

        protected override void OnOpen()
        {
            _leftFill.fillAmount = 0;
            _rightFill.fillAmount = 0;
        }

        public void SetPercent(int percentage, bool right)
        {
            var fill = percentage / 100f;
            var targetImage = right ? _rightFill : _leftFill;
            var otherImage = right ? _leftFill : _rightFill;

            targetImage.fillAmount = fill;
            targetImage.color = fill > 1f 
                ? Color.red 
                : (fill > .65f ? Color.green : Color.yellow);

            otherImage.fillAmount = fill <= 0 ? Mathf.Abs(fill) : 0f;
            otherImage.color = Color.red;
        }

    }
}

