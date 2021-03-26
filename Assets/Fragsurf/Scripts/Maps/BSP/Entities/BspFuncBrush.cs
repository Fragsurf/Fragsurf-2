using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("func_brush")]
    public class BspFuncBrush : GenericBspEntityMonoBehaviour<FuncBrush>
    {
        protected override void OnStart()
        {
            gameObject.SetCollidersEnabled(Entity.Solid);
        }
    }
}
