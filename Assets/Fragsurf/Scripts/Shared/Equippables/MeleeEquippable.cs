using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Fragsurf.Shared
{
    public class MeleeEquippable : EquippableGameObject
    {

        public UnityEvent OnSwing = new UnityEvent();

        private float _swingTimer;
        private int _swingAnimationIndex = 0;

        public MeleeEquippableData MeleeData => Data as MeleeEquippableData;

        protected override void OnActionDown(int actionId)
        {
            TrySwing(actionId);
        }

        protected override void OnActionHold(int actionId)
        {
            if (MeleeData.CanRepeat)
            {
                TrySwing(actionId);
            }
        }

        protected override void OnActionRelease(int actionId)
        {
        }

        protected override void _Tick()
        {
            base._Tick();

            if(_swingTimer > 0)
            {
                _swingTimer -= Time.fixedDeltaTime;
            }
        }

        private void TrySwing(int actionId)
        {
            if(_swingTimer > 0)
            {
                return;
            }
            StartCoroutine(Swing(actionId));
        }

        protected virtual IEnumerator Swing(int actionId)
        {
            _swingTimer = MeleeData.TimeToSwing;

            if (!Entity.Game.IsHost)
            {
                var swingAnim = "Swing";

                if (MeleeData.SwingAnimations.Length > 0)
                {
                    swingAnim = MeleeData.SwingAnimations[_swingAnimationIndex];
                    _swingAnimationIndex++;
                    if (_swingAnimationIndex >= MeleeData.SwingAnimations.Length)
                    {
                        _swingAnimationIndex = 0;
                    }
                }

                ViewModel.PlayAnimation(swingAnim);
                AudioSource.PlayClip(MeleeData.SwingSound);
            }

            yield return new WaitForSeconds(MeleeData.TimeToImpact);

            if (TraceNearestHit(MeleeData.HitRadius, MeleeData.HitRange, out RaycastHit hit))
            {
                ProcessHit(hit);
            }
        }

        protected virtual void ProcessHit(RaycastHit hit)
        {
            var dmgInfo = new DamageInfo()
            {
                Amount = MeleeData.BaseDamage,
                HitPoint = hit.point,
                HitNormal = hit.normal,
                DamageType = DamageType.Normal,
                HitArea = HitboxArea.None,
                WeaponId = Entity.EntityId,
                AttackerEntityId = Equippable.Human.EntityId,
                Server = Entity.Game.IsHost
            };

            if (hit.collider && hit.collider.gameObject.layer == Layers.Default)
            {
                hit.collider.GetComponentInParent<IDamageable>()?.Damage(dmgInfo);
            }

            if (hit.collider.TryGetComponent(out HitboxBehaviour hb)
                && Entity.Game.EntityManager.TryFindEntity(hb.EntityId, out NetEntity ent)
                && ent is IDamageable dmg)
            {
                AudioSource.PlayClip(MeleeData.HitFleshSound);
                dmgInfo.VictimEntityId = hb.EntityId;
                dmgInfo.HitArea = hb.Area;
                dmg.Damage(dmgInfo);
            }
            else
            {
                AudioSource.PlayClip(MeleeData.HitSolidSound);
                ImpactEffect(hit);
            }
        }

    }
}

