using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_Killfeed : UGuiModal
    {

        private Modal_KillfeedEntry _template;

        private void Start()
        {
            _template = gameObject.GetComponentInChildren<Modal_KillfeedEntry>(true);
            _template.gameObject.SetActive(false);

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                cl.EntityManager.OnHumanDamaged += EntityManager_OnHumanDamaged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                cl.EntityManager.OnHumanDamaged -= EntityManager_OnHumanDamaged;
            }
        }

        private void EntityManager_OnHumanDamaged(Human hu, DamageInfo dmgInfo)
        {
            if (!dmgInfo.ResultedInDeath)
            {
                return;
            }
            _template.Prepend(new Modal_KillfeedEntry.Data()
            {
                DamageInfo = dmgInfo
            });
        }

    }
}

