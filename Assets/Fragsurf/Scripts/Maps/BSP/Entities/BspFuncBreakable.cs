using Fragsurf.Actors;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("func_breakable")]
    public class BspFuncBreakable : GenericBspEntityMonoBehaviour<FuncBreakable>, IResettable
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

            if(Entity.Health > 0)
            {
                Broken = true;
            }
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

