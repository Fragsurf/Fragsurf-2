using Fragsurf.Shared.Entity;
using System;
using UnityEngine;

namespace Fragsurf.Shared
{
    public enum EquippableAnimationLayer
    {
        Unequipped = 0,
        Pistol = 1,
        Rifle = 2,
        Knife = 3
    }
    public abstract class BaseEquippableData : ScriptableObject
    {
        [Header("Base Equippable")]
        public string Name;
        public ItemSlot Slot;
        public EquippableWorldModel WorldModelPrefab;
        public EquippableViewModel ViewModelPrefab;
        public EquippableAnimationLayer PlayerAnimationLayer;
        public AudioClip EquipSound;
        public AudioClip UnequipSound;

        public float TimeToEquip = .5f;
        public float TimeToUnequip = .5f;

        public virtual ImpactType ImpactType { get; } = ImpactType.Bullet;

        public abstract Type ComponentType { get; }

    }
}

