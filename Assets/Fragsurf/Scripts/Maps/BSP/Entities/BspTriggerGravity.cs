using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;
using SourceUtils.ValveBsp.Entities;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_gravity")]
    public class BspTriggerGravity : BspTrigger<TriggerGravity>
    {
        protected override void OnStartTouch(NetEntity entity)
        {
            if (!(entity is Human hu)
                || !(hu.MovementController is CSMovementController move))
            {
                return;
            }
            move.MoveData.GravityFactor = Entity.Gravity;
        }
    }
}

