using FMODUnity;
using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    [CreateAssetMenu(fileName = "Revolver Equippable", menuName = "Fragsurf/Revolver Equippable", order = 2)]
    public class RevolverEquippableData : GunEquippableData
    {

        [Header("Revolver Sounds")]
        [EventRef]
        public string BeginReloadSound;
        [EventRef]
        public string EndReloadSound;
        [EventRef]
        public string InsertRoundSound;

        public override Type ComponentType => typeof(RevolverEquippable);

    }
}

