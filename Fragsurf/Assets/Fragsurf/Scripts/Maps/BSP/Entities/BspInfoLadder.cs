using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.BSP;
using Fragsurf.Utility;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("info_ladder")]
    public class BspInfoLadder : BspEntityMonoBehaviour
    {
        protected override void OnStart()
        {
            var ent = Entity as RInfoLadder;
            var min = new Vector3(ent.MinX, ent.MinZ, ent.MinY);
            var max = new Vector3(ent.MaxX, ent.MaxZ, ent.MaxY);
            var center = ((min + max) / 2f) * BspToUnity.Options.WorldScale;
            var sz = (max - min) * BspToUnity.Options.WorldScale;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "[Ladder]";
            cube.transform.position = center;
            cube.transform.localScale = sz;
            GameObject.Destroy(cube.GetComponent<Renderer>());
            cube.tag = "Ladder";
            cube.transform.SetParent(transform);
        }
    }
}
