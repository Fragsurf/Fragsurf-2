using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;

namespace Fragsurf.Client 
{
    [Inject(InjectRealm.Client)]
    public class DamageEffects : FSSharedScript
    {

        private bl_HudDamageManager _screenDamage;
        private bl_IndicatorManager _damageIndicator;
        private Hitmarker _hitmarker;

        [ConVar("game.hitmarker", "", ConVarFlags.UserSetting)]
        public bool Hitmarker { get; set; } = true;

        protected override void _Start()
        {
            _screenDamage = GameObject.FindObjectOfType<bl_HudDamageManager>(true);
            _damageIndicator = GameObject.FindObjectOfType<bl_IndicatorManager>(true);
            _hitmarker = GameObject.FindObjectOfType<Hitmarker>(true);
        }

        protected override void OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (GameData.Instance.TryGetImpactPrefab(ImpactType.Bullet, SurfaceType.Flesh, out GameObject prefab))
            {
                var effect = Game.Pool.Get(prefab, 10f);
                if(dmgInfo.HitPoint != Vector3.zero)
                {
                    effect.transform.position = dmgInfo.HitPoint;
                }
                if(dmgInfo.HitNormal != Vector3.zero)
                {
                    effect.transform.forward = dmgInfo.HitNormal;
                }
            }

            if(hu.HumanGameObject)
            {
                hu.HumanGameObject.PlayDamageSound(dmgInfo);
            }

            var targetHu = Game.Get<SpectateController>().TargetHuman;
            if (Hitmarker
                && _hitmarker
                && targetHu != null 
                && targetHu.EntityId == dmgInfo.AttackerEntityId 
                && hu != targetHu)
            {
                if(dmgInfo.HitArea == Shared.Player.HitboxArea.Head)
                {
                    _hitmarker.Trigger2();
                }
                else
                {
                    _hitmarker.Trigger();
                }
            }

            if (hu != targetHu
                || !_screenDamage
                || !_damageIndicator)
            {
                return;
            }

            var attacker = Game.EntityManager.FindEntity<Human>(dmgInfo.AttackerEntityId);
            if(attacker == null)
            {
                return;
            }

            _damageIndicator.LocalPlayer = hu.HumanGameObject.ViewBody.transform;
            hu.Punch(dmgInfo.Viewpunch, Vector3.zero);

            // todo: delete this crap damage indicator and create my own
            if (dmgInfo.AttackerEntityId != dmgInfo.VictimEntityId)
            {
                var indicator = new bl_IndicatorInfo(attacker.Origin);
                indicator.Sender = attacker.EntityGameObject.gameObject;
                indicator.TimeToShow = 3;
                indicator.ShowDistance = true;
                bl_DamageDelegate.OnIndicator(indicator);
            }

            var blDmgInfo = new bl_DamageInfo(dmgInfo.Amount);
            blDmgInfo.Sender = attacker.EntityGameObject.gameObject;
            bl_DamageDelegate.OnDamage(blDmgInfo);
        }

    }
}


