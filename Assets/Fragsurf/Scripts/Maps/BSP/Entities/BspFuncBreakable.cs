using Fragsurf.Actors;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("func_breakable")]
    public class BspFuncBreakable : BspEntityMonoBehaviour, IResettable
    {

        private bool _broken;

        [NetProperty]
        public bool Broken
        {
            get => _broken;
            set => SetBroken(value);
        }

        public override void Damage(DamageInfo dmgInfo)
        {
            base.Damage(dmgInfo);

            Broken = true;
        }

        private void SetBroken(bool broken)
        {
            gameObject.SetCollidersEnabled(!broken);

            foreach (var r in gameObject.GetComponentsInChildren<Renderer>())
            {
                r.enabled = !broken;
            }
        }

        public void OnReset()
        {
            Broken = false;
        }

    }
}

