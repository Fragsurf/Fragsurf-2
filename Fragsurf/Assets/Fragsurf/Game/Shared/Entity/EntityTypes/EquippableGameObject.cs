using FMODUnity;
using FMOD.Studio;
using Fragsurf.Movement;
using Fragsurf.Shared.Packets;
using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Shared.Entity
{
    public abstract class EquippableGameObject : EntityGameObject
    {

        public BaseEquippableData Data;

        public UnityEvent OnEquip = new UnityEvent();
        public UnityEvent OnUnequip = new UnityEvent();

        [HideInInspector]
        public Vector3 DropOrigin;
        [HideInInspector]
        public Vector3 DropAngles;
        [HideInInspector]
        public Vector3 DropForce;
        [HideInInspector]
        public Vector3 DropTorque;

        public bool HideViewModel;

        private bool _equipped;
        private float _equipTimer;
        private float _unequipTimer;
        private RaycastHit[] _hitBuffer = new RaycastHit[64];
        private Dictionary<int, bool> _actionDown = new Dictionary<int, bool>();

        public Equippable Equippable => Entity as Equippable;
        public GameObject WorldModel { get; private set; }
        public EquippableViewModel ViewModel { get; private set; }

        public override Vector3 Position 
        {
            get => WorldModel.transform.position; 
            set => WorldModel.transform.position = value; 
        }

        public override Vector3 Rotation
        {
            get => WorldModel.transform.eulerAngles;
            set => WorldModel.transform.eulerAngles = value;
        }

        public void Init(Equippable entity, BaseEquippableData data)
        {
            Entity = entity;
            Data = data;

            WorldModel = GameObject.Instantiate<GameObject>(Data.WorldModelPrefab);
            WorldModel.transform.SetParent(transform, true);
            WorldModel.gameObject.SetActive(false);
            WorldModel.transform.SetLayerRecursively(Entity.Game.ScopeLayer);
            WorldModel.transform.localPosition = Vector3.zero;
            WorldModel.transform.localRotation = Quaternion.identity;

            if(WorldModel.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }

            //if (!Entity.Game.IsHost
            //    && WorldModel.TryGetComponent(out Rigidbody rb))
            //{
            //    rb.isKinematic = true;
            //}

            ViewModel = GameObject.Instantiate<EquippableViewModel>(Data.ViewModelPrefab);
            ViewModel.transform.SetParent(transform, true);
            ViewModel.gameObject.SetActive(false);
            ViewModel.transform.SetLayerRecursively(Layers.Viewmodel);
            ViewModel.transform.localPosition = Vector3.zero;
            ViewModel.transform.localRotation = Quaternion.identity;

            var viewModelCamera = ViewModel.GetComponentInChildren<Camera>();
            viewModelCamera.clearFlags = CameraClearFlags.Depth;
            viewModelCamera.depth = 1;
            viewModelCamera.cullingMask = 1 << Layers.Viewmodel;
            viewModelCamera.tag = "ViewModelCamera";

            CleanRealm();

            _Init();
        }

        protected virtual void _Init() { }

        protected virtual void OnDisable()
        {
            _actionDown.Clear();
        }

        protected override void _Tick()
        {
            base._Tick();

            if (_equipTimer > 0)
            {
                _equipTimer -= Time.fixedDeltaTime;
            }
        }

        protected override void _Update()
        {
            if (Equippable.Human != null)
            {
                ViewModel.transform.position = Equippable.Human.Origin;
                ViewModel.transform.eulerAngles = Equippable.Human.Angles;

                if (_equipped)
                {
                    WorldModel.transform.position = Equippable.Human.HumanGameObject.HandAttachment.position;
                    WorldModel.transform.eulerAngles = Equippable.Human.HumanGameObject.HandAttachment.eulerAngles;
                }
            }

            SetVisibility();
        }
        
        public virtual void ProcessEquip()
        {
            if (_equipped)
            {
                return;
            }

            _equipped = true;
            _equipTimer = Data.TimeToEquip;
            _unequipTimer = 0f;

            if (!Entity.Game.IsHost && ViewModel)
            {
                SetVisibility();
                ViewModel.PlayAnimation("Equip", 0);
            }

            PlaySound(Data.EquipSound, true);

            WorldModel.SetCollidersEnabled(false);

            OnEquip?.Invoke();
        }

        public virtual void ProcessUnequip()
        {
            if (!_equipped)
            {
                return;
            }

            _equipped = false;
            _unequipTimer = Data.TimeToUnequip;

            if (!Entity.Game.IsHost && ViewModel)
            {
                SetVisibility();
                ViewModel.PlayAnimation("Unequip", 0);
            }

            //PlaySound(Data.UnequipSound, true);

            if (_instance.isValid())
            {
                _instance.stop(STOP_MODE.IMMEDIATE);
            }

            StopAllCoroutines();

            WorldModel.SetCollidersEnabled(true);

            OnUnequip?.Invoke();
        }

        public virtual void ProcessRunCommand(UserCmd.CmdFields userCmd)
        {
            if (_equipTimer > 0 || _unequipTimer > 0)
            {
                return;
            }

            var buttons = userCmd.Buttons;

            var action1 = buttons.HasFlag(InputActions.HandAction);
            var action2 = buttons.HasFlag(InputActions.HandAction2);
            var actionId = (byte)(action1 ? 0 : 1);

            if (action1 || action2)
            {
                if (IsActionDown(actionId))
                {
                    OnActionHold(actionId);
                }
                else
                {
                    OnActionDown(actionId);
                    _actionDown[actionId] = true;
                }
            }

            if (IsActionDown(0) && !action1)
            {
                OnActionRelease(0);
                _actionDown[0] = false;
            }
            else if (IsActionDown(1) && !action2)
            {
                OnActionRelease(1);
                _actionDown[1] = false;
            }
        }

        private void SetVisibility()
        {
            var isFirstPerson = Equippable.Human != null && Equippable.Human.IsFirstPerson;
            var viewModelVisible = !Entity.Game.IsHost
                && _equipped
                && Equippable.Human != null
                && isFirstPerson
                && !HideViewModel;
            var worldModelVisible = Equippable.Human == null || (_equipped && !isFirstPerson);

            ViewModel.gameObject.SetActive(viewModelVisible);
            WorldModel.gameObject.SetActive(worldModelVisible);

            if (worldModelVisible && 
                (DropForce != Vector3.zero 
                || DropTorque != Vector3.zero
                || DropOrigin != Vector3.zero))
            {
                if (DropOrigin != Vector3.zero)
                {
                    WorldModel.transform.position = DropOrigin;
                }

                WorldModel.transform.eulerAngles = DropAngles;

                if (WorldModel.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                    rb.AddForce(DropForce, ForceMode.Impulse);
                    rb.AddTorque(DropTorque, ForceMode.Impulse);
                }

                DropAngles = Vector3.zero;
                DropOrigin = Vector3.zero;
                DropForce = Vector3.zero;
                DropTorque = Vector3.zero;
            }
        }

        EventInstance _instance;
        public void PlaySound(string sound, bool usePersistentInstance = false)
        {
            if (Entity.Game.IsHost
                || string.IsNullOrWhiteSpace(sound)
                || Equippable.Human == null
                || Equippable.Human.HumanGameObject == null)
            {
                return;
            }

            EventInstance instance = RuntimeManager.CreateInstance(sound);
            if (usePersistentInstance)
            {
                if (_instance.isValid())
                {
                    _instance.stop(STOP_MODE.IMMEDIATE);
                }
                _instance = instance;
            }

            RuntimeManager.AttachInstanceToGameObject(instance, Equippable.Human.HumanGameObject.HeadAttachment, rigidBody: null);

            instance.start();
            instance.release();
        }

        protected virtual void ImpactEffect(RaycastHit hit)
        {
            var ray = new Ray(hit.point + hit.normal, -hit.normal);
            if (Physics.Raycast(ray, out RaycastHit hit2, hit.normal.magnitude + .01f, LayerMask.GetMask("TransparentFX")))
            {
                var matType = SurfaceMaterialType.Concrete;
                if (hit2.collider.TryGetComponent(out SurfaceMaterialIdentifier mi))
                {
                    matType = mi.MaterialType;
                }
                var effect = DataSet.Instance.GetImpactEffect(matType);
                if (effect != null)
                {
                    var decal = Entity.Game.Pool.Get(effect, 10f);
                    decal.transform.position = hit2.point;
                    decal.transform.forward = hit2.normal;
                }
            }
        }

        protected void RewindLagCompensator()
        {
            var testlag = DevConsole.GetVariable<bool>("net.testlag");

            if (!Entity.Game.IsHost)
            {
                if (testlag)
                {
                    var ownerRay = Equippable.Human.GetEyeRay();
                    Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.blue, 4);
                    DrawEntities(Color.green, 1.5f);
                }
                return;
            }

            var latency = Equippable.Game.PlayerManager.FindPlayer(Equippable.Human).Latency;
            Equippable.Human.DisableLagCompensation = true;
            Equippable.Game.LagCompensator.Rewind(latency);

            if (testlag)
            {
                var ownerRay = Equippable.Human.GetEyeRay();
                Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.magenta, 4);
                DrawEntities(Color.yellow);
            }

            Equippable.Human.DisableLagCompensation = false;
        }

        private void DrawEntities(Color color, float scale = 1)
        {
            foreach (NetEntity ent in Entity.Game.EntityManager.Entities)
            {
                if (ent.DisableLagCompensation)
                {
                    continue;
                }
                Debug.DrawLine(ent.EntityGameObject.Position, ent.EntityGameObject.Position + Vector3.up * scale, color, 6f);
            }
        }

        protected void RestoreLagCompensator()
        {
            if (!Entity.Game.IsHost)
                return;

            Equippable.Human.DisableLagCompensation = true;
            Entity.Game.LagCompensator.Restore();
            Equippable.Human.DisableLagCompensation = false;
        }

        protected bool TraceNearestHit(Ray ray, float radius, float maxDist, out RaycastHit hit)
        {
            RewindLagCompensator();
            Equippable.Human.HumanGameObject.SetLayersToIgnore();

            hit = default;
            int hitCount;

            if (radius > 0)
            {
                hitCount = Entity.Game.Physics.SpherecastAll(ray: ray,
                    radius: radius,
                    results: _hitBuffer,
                    maxDistance: maxDist,
                    qt: QueryTriggerInteraction.Collide);
            }
            else
            {
                hitCount = Entity.Game.Physics.RaycastAll(ray: ray,
                    results: _hitBuffer,
                    maxDistance: maxDist,
                    qt: QueryTriggerInteraction.Collide);
            }

            Equippable.Human.HumanGameObject.ResetLayers();
            RestoreLagCompensator();

            if (hitCount == 0)
            {
                return false;
            }

            var dist = float.MaxValue;
            var hitIsValid = false;

            for (int i = 0; i < hitCount; i++)
            {
                if (_hitBuffer[i].distance >= dist
                    || !Entity.Game.Physics.IsTraceable(_hitBuffer[i].collider))
                {
                    continue;
                }
                hit = _hitBuffer[i];
                dist = _hitBuffer[i].distance;
                hitIsValid = true;
            }

            // physx returns Vector3.zero if the cast starts inside the collider.
            // stupid hack to at least be close to the hit point
            if (hitIsValid && hit.point == Vector3.zero)
            {
                hit.point = hit.collider.ClosestPointOnBounds(ray.origin);
            }

            return hitIsValid;
        }

        protected bool TraceNearestHit(float radius, float maxDist, out RaycastHit hit)
        {
            var ray = Equippable.Human.GetEyeRay();
            return TraceNearestHit(ray, radius, maxDist, out hit);
        }

        private bool IsActionDown(int actionId)
        {
            return _actionDown.ContainsKey(actionId) && _actionDown[actionId];
        }

        protected abstract void OnActionDown(int actionId);
        protected abstract void OnActionHold(int actionId);
        protected abstract void OnActionRelease(int actionId);

    }
}

