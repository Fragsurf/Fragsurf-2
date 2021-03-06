using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;

namespace Fragsurf.BSP
{
    [EntityComponent("func_occluder", "func_hostage_rescue", "func_buyzone", "func_breakable", "func_breakable_surf")]
    public class BspDiscard : BspEntityMonoBehaviour
    {
        protected override void OnStart()
        {
            gameObject.DestroyComponentsInChildren<Collider>();
            gameObject.SetActive(false);
        }
    }
}

