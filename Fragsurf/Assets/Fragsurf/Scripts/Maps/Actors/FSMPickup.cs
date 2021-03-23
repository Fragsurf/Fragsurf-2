using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Actors
{
    public class FSMPickup : FSMTrigger
    {

        private int _given;
        private GameObject _wm;
        private BaseEquippableData _data;
        private GameAudioSource _src;

        [Header("FSMPickup Options")]

        public ItemNames Item;
        public bool GiveOnTouch;
        [Tooltip("0 = unlimited")]
        public int Quantity = 1;

        [NetProperty]
        public int Given
        {
            get => _given;
            set => SetGiven(value);
        }

        protected override void _Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if(!gameObject.TryGetComponent(out _src))
            {
                _src = gameObject.AddComponent<GameAudioSource>();
            }
            _src.Src.spatialBlend = 1f;
            _src.Category = SoundCategory.Equippable;

            if (GameData.Instance.TryGetEquippable(Item.ToString(), out _data))
            {
                _wm = _data.WorldModelPrefab
                    ? GameObject.Instantiate(_data.WorldModelPrefab).gameObject
                    : GameObject.CreatePrimitive(PrimitiveType.Cube);
                if(_wm.TryGetComponent(out Rigidbody rb))
                {
                    GameObject.Destroy(rb);
                }
                _wm.transform.position = transform.position;
                _wm.transform.SetParent(transform);
                foreach(var r in _wm.GetComponentsInChildren<Renderer>())
                {
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    r.rendererPriority = 10;
                }
                foreach(var lg in _wm.GetComponentsInChildren<LODGroup>())
                {
                    lg.ForceLOD(lg.lodCount);
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
                || !entity.Game.IsHost
                || !(entity is Human hu)
                || (!forced && hu.Equippables.HasItemInSlot(_data.Slot)))
            {
                return;
            }

            hu.Give(_data.Name);

            Given++;
        }

        private void SetGiven(int newGiven)
        {
            _given = newGiven;

            if(_src && _data && _data.EquipSound)
            {
                _src.PlayClip(_data.EquipSound);
            }

            if (Quantity > 0 && _given >= Quantity)
            {
                _wm.SetActive(false);
                GameObject.Destroy(gameObject, 1f);
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

