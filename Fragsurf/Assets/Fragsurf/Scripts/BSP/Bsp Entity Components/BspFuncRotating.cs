using UnityEngine;
using Fragsurf.BSP;
using SourceUtils.ValveBsp.Entities;

namespace Fragsurf.BSP
{
    [EntityComponent("func_rotating")]
    public class BspFuncRotating : GenericBspEntityMonoBehaviour<FuncRotating>
    {

        private Vector3 _rotateAxis = Vector3.up;

        protected override void OnStart()
        {
            // set _rotateAxis...
        }

        protected override void OnUpdate()
        {
            transform.Rotate(_rotateAxis, Entity.MaxSpeed  * Time.deltaTime);
        }

    }
}

