using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.FSM.Actors
{
    public class FSMIllusionary : FSMActor
    {

        protected override void _Start()
        {
            DisableColliders(transform);
        }

        private void DisableColliders(Transform tr)
        {
            var collider = tr.GetComponent<Collider>();
            if(collider != null)
            {
                collider.enabled = false;
            }
            foreach(Transform transform in tr)
            {
                DisableColliders(transform);
            }
        }

    }
}

