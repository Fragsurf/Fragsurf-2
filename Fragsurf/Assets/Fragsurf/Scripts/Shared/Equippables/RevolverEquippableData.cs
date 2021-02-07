using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    [CreateAssetMenu(fileName = "Revolver Equippable", menuName = "Fragsurf/Revolver Equippable", order = 2)]
    public class RevolverEquippableData : GunEquippableData
    {

        [Header("Revolver Sounds")]
        public AudioClip BeginReloadSound;
        public AudioClip EndReloadSound;
        public AudioClip InsertRoundSound;

        public override Type ComponentType => typeof(RevolverEquippable);

    }
}

