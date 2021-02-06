using Fragsurf.BSP;
using Fragsurf.FSM.Actors;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("trigger_push")]
    public class BspTriggerPush : BspTrigger<TriggerPush>
    {

        protected override void OnStart()
        {
            GameObject.Destroy(GetComponent<FSMTrigger>());
            var push = gameObject.AddComponent<FSMPush>();
            push.Speed = Entity.PushSpeed * BspToUnity.Options.WorldScale;
            push.Direction = Entity.PushDirection.TOUDirection().normalized;
        }

    }
}

