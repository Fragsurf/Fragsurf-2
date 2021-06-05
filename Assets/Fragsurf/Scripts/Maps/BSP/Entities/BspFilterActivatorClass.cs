using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("filter_activator_class")]
    public class BspFilterActivatorClass : BspBaseFilter
    {

        public override bool Passes(NetEntity ent)
        {
            var fn = Entity as FilterActivatorClass;

            if (string.IsNullOrEmpty(fn.FilterClass))
            {
                return true;
            }

            var match = string.Equals(fn.FilterClass, ent.ClassName, System.StringComparison.OrdinalIgnoreCase);

            if (fn.AllowMatch)
            {
                return match;
            }

            return !match;
        }

    }
}

