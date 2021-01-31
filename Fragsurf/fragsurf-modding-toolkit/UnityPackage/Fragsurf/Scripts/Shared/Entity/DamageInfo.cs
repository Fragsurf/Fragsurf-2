using UnityEngine;
using Fragsurf.Shared.Player;

namespace Fragsurf.Shared.Entity
{
    public enum DamageType
    {
        Normal,
        Fall,
        Bullet
    }

    public struct DamageInfo
    {
        public int WeaponId;
        public int AttackerEntityId;
        public int VictimEntityId;
        public int Amount;
        public bool ResultedInDeath;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public float Viewpunch;
        public HitboxArea HitArea;
        public DamageType DamageType;
    }
}
