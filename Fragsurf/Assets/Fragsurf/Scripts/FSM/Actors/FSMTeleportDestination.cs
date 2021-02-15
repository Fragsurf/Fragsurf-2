using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Actors
{
    public class FSMTeleportDestination : FSMActor
    {
        protected override void _Start()
        {
            EnableRenderers(false);
            gameObject.DestroyComponentsInChildren<Collider>();
        }

        protected override void _OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            DebugDraw.GizmoArrow(transform.position, transform.forward);
            DebugDraw.WorldLabel($"Teleport Destination", transform.position + Vector3.up, 12, Color.green, 30f);
        }

    }
}

