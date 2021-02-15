using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf.Actors
{
    public class FSMSpawnPoint : FSMActor
    {

        [Header("Spawn Point Options")]

        public int TeamNumber;

        protected override void _Awake()
        {
            EnableRenderers(false);
            gameObject.DestroyComponentsInChildren<Collider>();
        }

        protected override void _OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            DebugDraw.GizmoArrow(transform.position, transform.forward);
            DebugDraw.WorldLabel($"Spawn T{TeamNumber}", transform.position + Vector3.up, 12, Color.green, 30f);
        }

    }
}

