using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    [DataEditor.DataEditor]
    [CreateAssetMenu(fileName = "Shotgun Equippable", menuName = "Fragsurf/Shotgun Equippable", order = 2)]
    public class ShotgunEquippableData : GunEquippableData
    {

        [Header("Shotgun Sounds")]
        public AudioClip InsertShellSound;
        public AudioClip PumpSound;

        [Header("Shotgun")]
        public int PelletCount = 5;
        public float ZeroDamageDistance = 20;
        public float SpreadMin = .01f;
        public float SpreadMax = .03f;
        public float PumpTime = .75f;
        public float PumpDelay = .5f;

        public override Type ComponentType => typeof(ShotgunEquippable);

    }
}

