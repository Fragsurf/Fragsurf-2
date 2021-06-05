using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("filter_activator_name")]
    public class BspFilterActivatorName : BspBaseFilter
    {

        public override bool Passes(NetEntity ent)
        {
            var fn = Entity as FilterActivatorName;

            if (string.IsNullOrEmpty(fn.FilterName))
            {
                return true;
            }

            var match = string.Equals(fn.FilterName, ent.EntityName, System.StringComparison.OrdinalIgnoreCase);

            if (fn.AllowMatch)
            {
                return match;
            }

            return !match;
        }

    }
}

