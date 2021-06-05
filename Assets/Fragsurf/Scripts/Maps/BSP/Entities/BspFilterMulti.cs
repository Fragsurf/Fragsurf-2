using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using System.Linq;

namespace Fragsurf.BSP
{
    [EntityComponent("filter_multi")]
    public class BspFilterMulti : BspBaseFilter
    {

        public override bool Passes(NetEntity ent)
        {
            var fm = Entity as FilterMulti;

            var pass1 = Test(fm.Filter01, ent);
            var check1 = !string.IsNullOrEmpty(fm.Filter01);
            var pass2 = Test(fm.Filter02, ent);
            var check2 = !string.IsNullOrEmpty(fm.Filter02);
            var pass3 = Test(fm.Filter03, ent);
            var check3 = !string.IsNullOrEmpty(fm.Filter03);
            var pass4 = Test(fm.Filter04, ent);
            var check4 = !string.IsNullOrEmpty(fm.Filter04);

            var allpass = true;
            var anypass = false;

            if(check1) { if (!pass1) { allpass = false; } else { anypass = true; } }
            if(check2) { if (!pass2) { allpass = false; } else { anypass = true; } }
            if(check3) { if (!pass3) { allpass = false; } else { anypass = true; } }
            if(check4) { if (!pass4) { allpass = false; } else { anypass = true; } }

            if (fm.FilterType == 0)
            {
                return allpass;
            }

            return anypass;
        }

        private bool Test(string filterName, NetEntity ent)
        {
            if (string.IsNullOrEmpty(filterName))
            {
                return true;
            }

            foreach (var f in FindBspEntities(filterName))
            {
                if (!(f is BspBaseFilter tf))
                {
                    continue;
                }
                return tf.Passes(ent);
            }

            return true;
        }

    }
}

