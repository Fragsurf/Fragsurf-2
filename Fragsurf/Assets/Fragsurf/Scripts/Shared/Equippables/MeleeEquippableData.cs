using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    [DataEditor.DataEditor]
    [CreateAssetMenu(fileName = "Melee Equippable", menuName = "Fragsurf/Melee Equippable", order = 2)]
    public class MeleeEquippableData : BaseEquippableData
    {

        public override Type ComponentType => typeof(MeleeEquippable);

        [Header("Animations")]
        public string[] SwingAnimations = { "Swing" };

        [Header("Melee")]
        public bool CanRepeat = true;
        public float TimeToImpact = .25f;
        public float TimeToSwing = .45f;
        public int BaseDamage = 50;
        public float HitRange = 2f;
        public float HitRadius = .1f;

        [Header("Audio")]
        public AudioClip SwingSound;
        public AudioClip HitFleshSound;
        public AudioClip HitSolidSound;

    }
}

