using System;
using Fragsurf.BSP;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("logic_timer")]
    public class BspLogicTimer : GenericBspEntityMonoBehaviour<LogicTimer>
    {

        private float _refireTimer;

        protected override void OnStart()
        {
            if (Entity.UseRandomTime == 1)
            {
                _refireTimer = (Entity.LowerRandomBound + Entity.UpperRandomBound) / 2f;
            }
            else if (Entity.RefireInterval > 0)
            {
                _refireTimer = Entity.RefireInterval;
            }
        }

        protected override void OnUpdate()
        {
            if(_refireTimer > 0)
            {
                _refireTimer -= Time.deltaTime;
                if(_refireTimer <= 0)
                {
                    OnTimer();
                }
            }
        }

        private void OnTimer()
        {
            Fire("OnTimer");

            if (Entity.UseRandomTime == 1)
            {
                _refireTimer = (Entity.LowerRandomBound + Entity.UpperRandomBound) / 2f;
            }
            else if(Entity.RefireInterval > 0)
            {
                _refireTimer = Entity.RefireInterval;
            }
        }

    }
}

