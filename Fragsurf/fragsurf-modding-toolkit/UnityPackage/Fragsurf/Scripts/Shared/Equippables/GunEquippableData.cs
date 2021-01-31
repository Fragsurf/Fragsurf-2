using FMODUnity;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared
{
    [CreateAssetMenu(fileName = "Gun Equippable", menuName = "Fragsurf/Gun Equippable", order = 1)]
    public class GunEquippableData : BaseEquippableData
    {
        [Header("Audio")]
        [EventRef]
        public string FireSound;
        [EventRef]
        public string FireTailSound;
        [EventRef]
        public string ReloadSound;
        [EventRef]
        public string BoltActionSound;
        [EventRef]
        public string ScopeSound;
        [EventRef]
        public string DryFireSound;
        [Header("Ammo")]
        public int RoundsPerClip = 30;
        public int MaxClips = 3;
        public float BulletRadius = .05f;
        public float BulletRange = 512;
        [Header("Firing")]
        public GunFiringMode FiringMode = GunFiringMode.SingleShot;
        public Vector3 AimPunchMin = new Vector3(-.1f, -1, -1);
        public Vector3 AimPunchMax = new Vector3(-1f, 1, 1);
        public int RoundsPerMinute = 400;
        public float ReloadTime = 4f;
        public float BoltActionTime = .65f;
        public float ViewModelKickStrength = 1f;
        public bool FiringInterruptsReload = false;
        [Header("Scope")]
        public float[] ZoomLevels;
        public Texture2D ScopeTexture;
        [Header("Damage")]
        public int BaseDamage = 20;
        public List<AreaDamageDefinition> DamagePercents = new List<AreaDamageDefinition>()
        {
            new AreaDamageDefinition(HitboxArea.Head, 500),
            new AreaDamageDefinition(HitboxArea.Neck, 300),
            new AreaDamageDefinition(HitboxArea.UpperChest, 100),
            new AreaDamageDefinition(HitboxArea.LowerChest, 75),
            new AreaDamageDefinition(HitboxArea.UpperArm, 50),
            new AreaDamageDefinition(HitboxArea.LowerArm, 50),
            new AreaDamageDefinition(HitboxArea.Hand, 35),
            new AreaDamageDefinition(HitboxArea.UpperLeg, 65),
            new AreaDamageDefinition(HitboxArea.LowerLeg, 55),
            new AreaDamageDefinition(HitboxArea.Foot, 30)
        };

        public override Type ComponentType => typeof(GunEquippable);

        public float GetDamageMultiplier(HitboxArea hitbox)
        {
            foreach(var hb in DamagePercents)
            {
                if(hb.Area == hitbox)
                {
                    return hb.DamagePercent / 100f;
                }
            }
            return 1f;
        }
    }

    public enum GunFiringMode
    {
        SingleShot,
        BurstFire,
        FullyAutomatic,
        BoltAction
    }

}

