using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HighlightPlus {


    public partial class HighlightEffect : MonoBehaviour {

        float hitInitialIntensity;
        float hitStartTime;
        float hitFadeOutDuration;
        Color hitColor;
        bool hitActive;

        /// <summary>
        /// Performs a hit effect using desired color, fade out duration and optionally initial intensity (0-1)
        /// </summary>
        public void HitFX(Color color, float fadeOutDuration, float initialIntensity = 1f) {
            hitInitialIntensity = initialIntensity;
            hitFadeOutDuration = fadeOutDuration;
            hitColor = color;
            hitStartTime = Time.time;
            hitActive = true;
            if (overlay == 0) {
                overlay = hitInitialIntensity;
                UpdateMaterialProperties();
            }
        }


        /// <summary>
        /// Initiates the target FX on demand using predefined configuration (see targetFX... properties)
        /// </summary>
        public void TargetFX() {
            targetFxStartTime = Time.time;
            if (!targetFX) {
                targetFX = true;
                UpdateMaterialProperties();
            }
        }
    }
}