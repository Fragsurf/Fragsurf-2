using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;

namespace Fragsurf.BSP
{
    [EntityComponent("func_illusionary")]
    public class BspFuncIllusionary : BspEntityMonoBehaviour
    {
        protected override void OnStart()
        {
            gameObject.DestroyComponentsInChildren<Collider>();
        }
    }
}

