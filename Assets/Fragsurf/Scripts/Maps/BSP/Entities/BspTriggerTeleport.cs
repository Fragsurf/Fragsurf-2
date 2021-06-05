using Fragsurf.BSP;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using System;
using System.Linq;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_teleport")]
    public class BspTriggerTeleport : BspTrigger<TriggerTeleport>
    {
        protected override void OnTouch(NetEntity entity)
        {
            if(!(entity is Human hu) || string.IsNullOrEmpty(Entity.Target))
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

