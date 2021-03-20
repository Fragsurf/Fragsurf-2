using Fragsurf.Misc;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Shared
{
    public class GunEquippable : EquippableGameObject
    {
        public GunEquippableData GunData => Data as GunEquippableData;

        public UnityEvent OnFire = new UnityEvent();

        private int _zoomLevel;
        private bool _reloading;
        private float _fireTimer;
        private GunEffectTrigger[] _gunEffectTriggers;
        private GameAudioSource _fireSrc;
        private GameAudioSource _foley;
        private GameAudioSource _tailSrc;

        [NetProperty]
        public int ZoomLevel
        {
            get { return _zoomLevel; }
            set { SetZoomLevel(value); }
        }
        [NetProperty]
        public int RoundsInClip { get; set; }
        [NetProperty]
        public int ExtraRounds { get; set; }
        protected virtual bool CustomReload { get; set; }

        protected override void _Init()
        {
            ExtraRounds = Mathf.Max((GunData.MaxClips * GunData.RoundsPerClip) - GunData.RoundsPerClip, GunData.RoundsPerClip);
            RoundsInClip = GunData.RoundsPerClip;

            _gunEffectTriggers = GetComponentsInChildren<GunEffectTrigger>(true);

            _fireSrc = CreateAudioSource(SoundCategory.Equippable, 0.5f, GunData.FireMaxDistance, GunData.FireRolloff);
            _fireSrc.Src.spatialize = true;
            _tailSrc = CreateAudioSource(SoundCategory.Equippable, 0.5f, GunData.FireMaxDistance, GunData.FireRolloff);
            _foley = CreateAudioSource(SoundCategory.Equippable, 0f, 20f, GunData.FolleyRolloff);
            AudioSource = _foley;
        }

        protected override void _Update()
        {
            base._Update();

#if UNITY_EDITOR
            if (Equippable.Equipped && Input.GetKeyDown(KeyCode.U))
            {
                ViewModel.PlayAnimation("Idle");
                ViewModel.PlayAnimation("Reload");
                AudioSource.Src.Stop();
                AudioSource.PlayClip(GunData.ReloadSound);
            }
#endif
        }

        protected override void OnActionDown(int actionId)
        {
            if(actionId == 0)
            {
                TryFire();
            }
            else
            {
                TryAds();
            }
        }

        protected override void OnActionHold(int actionId)
        {
            if(actionId == 0 && GunData.FiringMode == GunFiringMode.FullyAutomatic)
            {
                TryFire();
            }
        }

        protected override void OnActionRelease(int actionId)
        {
            _tryFireRequiresRelease = false;
        }

        protected override void _Tick()
        {
            base._Tick();

            if (_fireTimer > 0)
            {
                _fireTimer -= Time.fixedDeltaTime;
            }
        }

        private void TryAds()
        {
            if(_fireTimer > 0 || _reloading || GunData.ZoomLevels.Length == 0)
            {
                return;
            }

            SetZoomLevel(_zoomLevel + 1);
        }

        private void SetZoomLevel(int zoom)
        {
            if(zoom >= GunData.ZoomLevels.Length)
            {
                zoom = 0;
            }
            else if(zoom < 0)
            {
                zoom = 0;
            }

            _zoomLevel = zoom;

            _foley.PlayClip(GunData.ScopeSound);

            var vmLayer = _zoomLevel == 0
                ? Layers.Viewmodel
                : Layers.Invisible;

            ViewModel.transform.SetLayerRecursively(vmLayer);
        }

        private bool _tryFireRequiresRelease;
        private void TryFire()
        {
            if (_fireTimer > 0 || _tryFireRequiresRelease)
            {
                return;
            }

            if (_reloading)
            {
                if (GunData.FiringInterruptsReload && RoundsInClip > 0)
                {
                    StopCoroutine(_reloadCoroutine);
                    _reloading = false;
                }
                else
                {
                    return;
                }
            }

            if (RoundsInClip == 0)
            {
                _tryFireRequiresRelease = true;
                ViewModel.PlayAnimation("DryFire");
                _foley.PlayClip(GunData.DryFireSound);
                TryReload();
                return;
            }

            _fireTimer = 1f / (GunData.RoundsPerMinute / 60f);
            RoundsInClip--;

            Fire();

            if(RoundsInClip == 0)
            {
                TryReload();
            }
        }

        private void TryReload()
        {
            if(_reloading || ExtraRounds <= 0 || _fireTimer > 0 || RoundsInClip >= GunData.RoundsPerClip)
            {
                return;
            }
            _reloadCoroutine = Reload();
            StartCoroutine(_reloadCoroutine);
        }

        public override void ProcessRunCommand(UserCmd.CmdFields userCmd)
        {
            base.ProcessRunCommand(userCmd);

            if (userCmd.Buttons.HasFlag(InputActions.Reload))
            {
                TryReload();
            }
        }

        private IEnumerator _reloadCoroutine;

        private IEnumerator Reload()
        {
            _reloading = true;
            if (CustomReload)
            {
                _reloadCoroutine = ReloadOverride();
                yield return _reloadCoroutine;
            }
            else
            {
                SetZoomLevel(0);
                ViewModel.PlayAnimation("Reload");
                _foley.PlayClip(GunData.ReloadSound, 1f, true);
                yield return new WaitForSeconds(GunData.ReloadTime);
                var delta = Mathf.Min(ExtraRounds, GunData.RoundsPerClip - RoundsInClip);
                ExtraRounds -= delta;
                RoundsInClip += delta;
                if (GunData.FiringMode == GunFiringMode.BoltAction)
                {
                    _foley.PlayClip(GunData.BoltActionSound);
                    ViewModel.PlayAnimation("BoltAction");
                    yield return new WaitForSeconds(GunData.BoltActionTime);
                }
            }
            _reloading = false;
        }

        protected virtual IEnumerator ReloadOverride()
        {
            yield return 0;
        }

        protected virtual void FireEffects(RaycastHit hit = default)
        {
            var punch = new Vector3(Equippable.Random.Range(GunData.AimPunchMin.x, GunData.AimPunchMax.x),
                Equippable.Random.Range(GunData.AimPunchMin.y, GunData.AimPunchMax.y),
                Equippable.Random.Range(GunData.AimPunchMin.z, GunData.AimPunchMax.z));
            Equippable.Human?.Punch(punch, Vector3.zero);

            _fireSrc.PlayClip(GunData.FireSound);

            if (Entity.Game.IsServer)
            {
                return;
            }

            if (Equippable.Human != null && Equippable.Human.IsFirstPerson)
            {
                _tailSrc.PlayClip(GunData.FireTailSound, 1.0f, true);
            }

            ViewModel.PlayAnimation("Fire");
            ViewModel.Kick(GunData.ViewModelKickStrength);

            if (_gunEffectTriggers != null)
            {
                foreach (var ge in _gunEffectTriggers)
                {
                    ge.Trigger(this, hit);
                }
            }
        }

        protected virtual void Fire()
        {
            SetZoomLevel(0);

            if (TraceNearestHit(GunData.BulletRadius, GunData.BulletRange, out RaycastHit hit))
            {
                ProcessHit(hit);
            }

            FireEffects(hit);

            if (GunData.FiringMode == GunFiringMode.BoltAction)
            {
                _foley.PlayClip(GunData.BoltActionSound);
                ViewModel.PlayAnimation("BoltAction");
            }

            OnFire?.Invoke();
        }

        protected virtual void ProcessHit(RaycastHit hit)
        {
            var ent = Entity.Game.EntityManager.FindEntity(hit.collider.gameObject);
            if(ent == null && !Entity.Game.IsServer)
            {
                ImpactEffect(hit);
                return;
            }

            if(ent is IDamageable dmg)
            {
                var hb = hit.collider.GetComponent<HitboxBehaviour>();
                var dmgMultiplier = hb ? GunData.GetDamageMultiplier(hb.Area) : 1f;
                var dmgAmount = (int)(GunData.BaseDamage * dmgMultiplier);
                dmg.Damage(new DamageInfo()
                {
                    Amount = dmgAmount,
                    HitPoint = hit.point,
                    HitNormal = hit.normal,
                    DamageType = DamageType.Bullet,
                    HitArea = hb != null ? hb.Area : HitboxArea.None,
                    WeaponId = Entity.EntityId,
                    AttackerEntityId = Equippable.HumanId,
                    VictimEntityId = ent.EntityId
                });
            }
        }

        public override void ProcessUnequip()
        {
            base.ProcessUnequip();

            SetZoomLevel(0);

            _reloading = false;
        }

    }
}

