using Fragsurf.Shared.Packets;
using Fragsurf.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Fragsurf.Maps;
using SurfaceConfigurator;

namespace Fragsurf.Shared.Entity
{
    public class Equippable : NetEntity, IInteractable, IDamageable
    {

        public Equippable(FSGameLoop game)
            : base(game)
        {
        }

        private string _itemName;
        private int _humanId;
        private bool _equipped;

        [NetProperty]
        public string ItemName
        {
            get { return _itemName; }
            set { SetItemName(value); }
        }
        [NetProperty]
        public int HumanId
        {
            get { return _humanId; }
            set { SetHumanId(value); }
        }
        [NetProperty]
        public bool Equipped
        {
            get { return _equipped; }
            set { SetIsEquipped(value); }
        }
        [NetProperty]
        public int RandomAccumulator
        {
            get { return Random != null ? Random.NumberOfInvokes : 0; }
            set
            {
                if (!Game.IsServer && RandomAccumulator != value)
                {
                    Random = new StateRandom(EntityId, value);
                }
            }
        }
        public StateRandom Random { get; private set; }
        public EquippableGameObject EquippableGameObject => EntityGameObject as EquippableGameObject;
        public Human Human { get; private set; }

        public bool Dead => throw new System.NotImplementedException();

        protected override void OnInitialized()
        {
            Random = new StateRandom(EntityId);
            InterpolationMode = InterpolationMode.None;
        }

        protected override void OnDelete()
        {
            if (Human != null)
            {
                Human.Equippables.Remove(this);
            }

            base.OnDelete();
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (!Game.IsServer)
            {
                InterpolationMode = Human == null
                    ? InterpolationMode.Network
                    : InterpolationMode.None;
            }

            if(Human == null && _humanId > 0)
            {
                TryFindHuman(_humanId);
            }

            if(Human == null && EquippableGameObject)
            {
                Origin = EquippableGameObject.Position;
                Angles = EquippableGameObject.Rotation;
            }
        }

        private void TryFindHuman(int humanId)
        {
            // EquippableGameObject needs to exist
            if (!EquippableGameObject || humanId == -1)
            {
                return;
            }

            Human = Game.EntityManager.FindEntity<Human>(humanId);
            if(Human == null)
            {
                return;
            }

            DisableLagCompensation = true;
            Human.Equippables.Add(this);
            if (Human.Equippables.Equipped == null)
            {
                Equipped = true;
            }
        }

        private void SetItemName(string itemName)
        {
            _itemName = itemName;
            if (!GameData.Instance.TryGetEquippable(itemName, out BaseEquippableData data)) 
            {
                if (Game.IsServer)
                {
                    Delete();
                }
                Debug.LogError("Missing Equippable: " + itemName);
                return;
            }
            _itemName = data.Name;
            var obj = Game.NewGameObject();
            var ego = obj.AddComponent(data.ComponentType) as EquippableGameObject;
            EntityGameObject = ego;
            ego.Init(this, data);
            SetIsEquipped(_equipped);
        }

        private void SetHumanId(int humanId)
        {
            _humanId = humanId;

            if (Human != null)
            {
                if (Equipped)
                {
                    Equipped = false;
                }
                DisableLagCompensation = false;
                Human.Equippables.Remove(this);
                Human = null;
            }

            TryFindHuman(humanId);
        }

        private void SetIsEquipped(bool equipped)
        {
            _equipped = equipped;

            if (!EquippableGameObject)
            {
                return;
            }

            if (equipped)
            {
                if (Human == null)
                {
                    Debug.LogError("Attempted to equip when Human is null!");
                    return;
                }
                Human.Equippables.SetEquippedItem(this);
                EquippableGameObject.ProcessEquip();
            }
            else
            {
                EquippableGameObject.ProcessUnequip();
            }
        }

        public void RunCommand(UserCmd.CmdFields userCmd)
        {
            if (!EquippableGameObject || !_equipped)
            {
                return;
            }

            EquippableGameObject.ProcessRunCommand(userCmd);
        }

        public void Drop()
        {
            if(Human == null)
            {
                Debug.LogError("Attempt to drop item without Human");
                return;
            }

            Human.Equippables.Remove(this);

            if (Equipped)
            {
                var nextItem = Human.Equippables.GetNextItem(EquippableGameObject.Data.Slot) ?? Human.Equippables.GetNextItem();
                if (nextItem != null)
                {
                    nextItem.Equipped = true;
                }
            }

            var dropPos = Human.GetEyeRay().origin;
            var fwd = Quaternion.Euler(Human.Angles) * Vector3.forward;
            var right = Quaternion.Euler(Human.Angles) * Vector3.right;
            EquippableGameObject.DropOrigin = dropPos + fwd * .25f + right * .25f + Vector3.down * .25f;
            EquippableGameObject.DropAngles = Human.Angles;
            EquippableGameObject.DropForce = Human.Velocity.normalized + fwd * 1.5f;
            EquippableGameObject.DropTorque = fwd * 5f;

            HumanId = -1;
        }

        public void OnInteract(NetEntity interactee)
        {
            if(Human == null)
            {
                HumanId = interactee.EntityId;
            }
        }

        public void MouseEnter(int clientIndex)
        {

        }

        public void MouseExit(int clientIndex)
        {

        }

        public void Damage(DamageInfo dmgInfo)
        {
            if (EquippableGameObject.WorldModel.TryGetComponent(out Rigidbody rb))
            {
                var dir = dmgInfo.HitNormal;
                var attacker = Game.EntityManager.FindEntity(dmgInfo.AttackerEntityId);
                if (attacker != null)
                {
                    dir = (Origin - attacker.Origin).normalized;
                }
                rb.AddForceAtPosition(dir * 1.5f, dmgInfo.HitPoint, ForceMode.Impulse);

                if (!Game.IsServer
                    && GameData.Instance.TryGetImpactPrefab(ImpactType.Bullet, SurfaceType.Metal, out GameObject prefab))
                {
                    var effect = Game.Pool.Get(prefab, 1f);
                    effect.transform.position = dmgInfo.HitPoint;
                    effect.transform.forward = dmgInfo.HitNormal;
                }
            }
        }

    }
}

