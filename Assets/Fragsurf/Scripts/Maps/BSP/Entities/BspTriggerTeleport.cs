using Fragsurf.BSP;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using System.Linq;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_teleport")]
    public class BspTriggerTeleport : BspTrigger<TriggerTeleport>
    {
        protected override void OnStartTouch(NetEntity entity)
        {
            if(!(entity is Human hu))
            {
                return;
            }
            var target = FindBspEntities(Entity.Target).FirstOrDefault();
            if (target != null)
            {
                entity.Origin = target.transform.position;
            }
        }
    }
}

