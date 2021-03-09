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
            if(hit.collider.TryGetComponent(out HitboxBehaviour hb)
                && Entity.Game.EntityManager.TryFindEntity(hb.EntityId, out NetEntity ent)
                && ent is IDamageable dmg)
            {
                AudioSource.PlayClip(MeleeData.HitFleshSound);
                dmg.Damage(new DamageInfo()
                {
                    Amount = MeleeData.BaseDamage,
                    AttackerEntityId = Equippable.Human.EntityId,
                    DamageType = DamageType.Normal,
                    HitArea = hb.Area,
                    HitNormal = hit.normal,
                    HitPoint = hit.point,
                    VictimEntityId = hb.EntityId,
                    WeaponId = Entity.EntityId
                });
            }
            else
            {
                ImpactEffect(hit);
            }
        }

    }
}

