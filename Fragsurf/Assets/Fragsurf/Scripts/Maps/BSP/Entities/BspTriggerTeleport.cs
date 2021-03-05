using Fragsurf.BSP;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_teleport")]
    public class BspTriggerTeleport : BspTrigger<TriggerTeleport>
    {
        protected override void OnStartTouch(NetEntity entity)
        {
            if(!(entity is Human hu)
                || !(hu.MovementController is ISurfControllable character))
            {
                return;
            }
            var target = FindBspEntity(Entity.Target);
            if (target != null)
            {
                character.MoveData.Origin = target.transform.position;
                //entity.Origin = target.transform.position;
            }
        }
    }
}

