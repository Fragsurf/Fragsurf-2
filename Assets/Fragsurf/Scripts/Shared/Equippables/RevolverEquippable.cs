using System.Collections;
using UnityEngine;

namespace Fragsurf.Shared
{
    public class RevolverEquippable : GunEquippable
    {

        public RevolverEquippableData RevolverData => GunData as RevolverEquippableData;
        protected override bool CustomReload => true;

        protected override void Fire()
        {
            ViewModel.PlayAnimation("Fire");
            base.Fire();
        }

        protected override IEnumerator ReloadOverride()
        {
            AudioSource.PlayClip(RevolverData.BeginReloadSound);
            ViewModel.PlayAnimation("BeginReload");
            yield return new WaitForSeconds(1.3f);
            while(RoundsInClip < GunData.RoundsPerClip
                && ExtraRounds > 0)
            {
                AudioSource.PlayClip(RevolverData.InsertRoundSound);
                ViewModel.PlayAnimation("InsertRound", .01f);
                yield return new WaitForSeconds(.867f);
                RoundsInClip++;
                ExtraRounds--;
            }
            AudioSource.PlayClip(RevolverData.EndReloadSound);
            ViewModel.PlayAnimation("EndReload");
            yield return new WaitForSeconds(.933f);
        }

    }
}

