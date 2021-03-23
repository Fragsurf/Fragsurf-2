using Fragsurf.BSP;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("func_buyzone")]
    public class BspFuncBuyZone : BspTrigger<FuncBrush>
    {

        protected override void OnStartTouch(NetEntity entity)
        {
        }

        protected override void OnEndTouch(NetEntity entity)
        {
        }

    }
}

