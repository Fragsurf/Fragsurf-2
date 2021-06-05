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
        protected override void OnStartTouch(NetEntity entity)
        {
            if(!(entity is Human hu) || string.IsNullOrEmpty(Entity.Target))
            {
                return;
            }

            if (!string.IsNullOrEmpty(Entity.FilterName))
            {
                foreach(var f in FindBspEntities(Entity.FilterName))
                {
                    if(!(f is BspBaseFilter tf))
                    {
                        continue;
                    }
                    if(!tf.Passes(entity))
                    {
                        return;
                    }
                }
            }

            var target = FindBspEntities(Entity.Target).FirstOrDefault();
            if (target != null)
            {
                entity.Origin = target.transform.position;
            }
        }
    }
}

