using UnityEngine;

namespace Fragsurf.Shared
{
    public class PhysicsManager : FSSharedScript
    {

        [ConVar("game.physx", "Simulates PhysX", ConVarFlags.Replicator)]
        public bool PhysX { get; set; } = true;

        protected override void OnGameLoaded()
        {
            Physics.autoSyncTransforms = false;
            Physics.autoSimulation = false;
        }

        protected override void _Tick()
        {
            if(Game.IsLocalServer)
            {
                return;
            }

            if(PhysX)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                Physics.SyncTransforms();
            }
        }

        public RaycastHit Raycast(Ray ray, float maxDistance, QueryTriggerInteraction qt = QueryTriggerInteraction.Ignore, int layerMask = -1)
        {
            if (layerMask == -1)
            {
                layerMask = (1 << 0) | (1 << Game.ScopeLayer);
            }

            Physics.Raycast(ray: ray,
                hitInfo: out RaycastHit hit,
                maxDistance: maxDistance,
                layerMask: layerMask,
                queryTriggerInteraction: qt);

            return hit;
        }

        public int RaycastAll(Ray ray, RaycastHit[] results, float maxDistance, QueryTriggerInteraction qt = QueryTriggerInteraction.Ignore, int layerMask = -1)
        {
            if(layerMask == -1)
            {
                layerMask = (1 << 0) | (1 << Game.ScopeLayer);
            }

            return Physics.RaycastNonAlloc(ray: ray, 
                results: results, 
                maxDistance: maxDistance, 
                layerMask: layerMask, 
                queryTriggerInteraction: qt);
        }

        public int SpherecastAll(Ray ray, float radius, RaycastHit[] results, float maxDistance, QueryTriggerInteraction qt = QueryTriggerInteraction.Ignore, int layerMask = -1)
        {
            if (layerMask == -1)
            {
                layerMask = (1 << 0) | (1 << Game.ScopeLayer);
            }

            return Physics.SphereCastNonAlloc(ray: ray, 
                radius: radius, 
                results: results, 
                maxDistance: maxDistance, 
                layerMask: layerMask, 
                queryTriggerInteraction: qt);
        }

        public bool IsTraceable(Collider collider)
        {
            if (collider.CompareTag("IgnoreItemTrace")
                || collider.CompareTag("Trigger")
                || collider.CompareTag("Player"))
            {
                return false;
            }
            return true;
        }

    }
}

