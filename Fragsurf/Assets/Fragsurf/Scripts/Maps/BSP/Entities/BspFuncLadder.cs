using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;

namespace Fragsurf.BSP
{
    [EntityComponent("func_ladder")]
    public class BspFuncLadder : BspEntityMonoBehaviour
    {
        protected override void OnStart()
        {
            foreach(var collider in GetComponentsInChildren<Collider>())
            {
                collider.gameObject.tag = "Ladder";
            }
        }
    }
}

