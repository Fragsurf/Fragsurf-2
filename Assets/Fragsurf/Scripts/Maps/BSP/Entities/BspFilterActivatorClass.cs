using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("filter_activator_class")]
    public class BspFilterActivatorClass : BspBaseFilter
    {

        public override bool Passes(NetEntity ent)
        {
            var fn = Entity as FilterActivatorClass;

            if (string.IsNullOrEmpty(fn.ClassName))
            {
                return true;
            }

            var match = string.Equals(fn.ClassName, ent.EntityName, System.StringComparison.OrdinalIgnoreCase);

            if (fn.AllowMatch)
            {
                return match;
            }

            return !match;
        }

    }
}

