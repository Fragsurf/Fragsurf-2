using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Actors
{
    public class FSMPickup : FSMTrigger
    {

        private int _remaining;
        private BaseEquippableData _data;

        [Header("FSMPickup Options")]

        public ItemNames Item;
        public bool GiveOnTouch;
        [Tooltip("0 = unlimited")]
        public int Quantity = 1;

        [NetProperty]
        public int Remaining
        {
            get => _remaining;
            set => SetRemaining(value);
        }

        protected override void _Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Remaining = Quantity;

            if (GameData.Instance.TryGetEquippable(Item.ToString(), out _data))
            {
                var wm = _data.WorldModelPrefab
                    ? GameObject.Instantiate(_data.WorldModelPrefab).gameObject
                    : GameObject.CreatePrimitive(PrimitiveType.Cube);
                if(wm.TryGetComponent(out Rigidbody rb))
                {
                    GameObject.Destroy(rb);
                }
                wm.transform.position = transform.position;
                wm.transform.SetParent(transform);
                foreach(var r in wm.GetComponentsInChildren<Renderer>())
                {
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.rendererPriority = 10;
                }
                foreach(var lg in wm.GetComponentsInChildren<LODGroup>())
                {
                    lg.ForceLOD(lg.lodCount - 1);
                }
            }

            gameObject.SetChildrenCollidersToConvexTrigger(true);

            if(!gameObject.TryGetComponent(out Rigidbody rb2))
            {
                rb2 = gameObject.AddComponent<Rigidbody>();
            }
            rb2.isKinematic = true;
        }

        protected override void _TriggerEnter(NetEntity entity)
        {
            if (!GiveOnTouch)
            {
                return;
            }
            TryGive(entity);
        }

        protected override void _TriggerInteract(NetEntity entity)
        {
            TryGive(entity, true);
        }

        private void TryGive(NetEntity entity, bool forced = false)
        {
            if (!_data 
                || !entity.Game.IsServer
                || !(entity is Human hu)
                || (!forced && hu.Equippables.HasItemInSlot(_data.Slot)))
            {
                return;
            }

            hu.Give(_data.Name);

            if (Quantity > 0)
            {
                Remaining--;
            }
        }

        private void SetRemaining(int newLimit)
        {
            _remaining = newLimit;
            if(_remaining <= 0)
            {
                gameObject.SetActive(false);
            }
        }

        // this will have to stay updated with the items in game
        // especially if there's a name change
        public enum ItemNames
        {
            // melee
            Knife,
            Axe,
            Bat,

            // light
            M1911,
            Revolver,

            // heavy
            AK47,
            Famas,
            MP5,
            Bolty,
            R870,
            DB
        }

    }
}

