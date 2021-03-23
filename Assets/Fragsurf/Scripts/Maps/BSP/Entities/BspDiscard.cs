using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;

namespace Fragsurf.BSP
{
    [EntityComponent("func_occluder", "func_hostage_rescue", "func_breakable", "func_breakable_surf", "func_precipitation")]
    public class BspDiscard : BspEntityMonoBehaviour
    {
        protected override void OnStart()
        {
            gameObject.DestroyComponentsInChildren<Collider>();
            gameObject.SetActive(false);
        }
    }
}

