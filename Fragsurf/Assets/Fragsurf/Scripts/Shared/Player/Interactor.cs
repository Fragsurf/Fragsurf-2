using UnityEngine;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Entity;
using Fragsurf.Movement;
using Fragsurf.Actors;

namespace Fragsurf.Shared.Player
{
    public class Interactor
    {
        public Interactor(Human hu)
        {
            _human = hu;
        }

        private Human _human;
        private RaycastHit[] _hitBuffer = new RaycastHit[24];
        private float _interactionDistance = 2.5f;

        public void RunCommand(UserCmd.CmdFields userCmd)
        {
            //if (!_human.Game.IsHost)
            //    return;

            if (userCmd.Buttons.HasFlag(InputActions.Interact))
            {
                CheckForInteractables();
            }
        }

        private void DrawEntities(Color color, float scale = 1)
        {
            foreach (NetEntity ent in _human.Game.EntityManager.Entities)
            {
                if (ent.DisableLagCompensation)
                {
                    continue;
                }
                Debug.DrawLine(ent.EntityGameObject.Position, ent.EntityGameObject.Position + Vector3.up * scale, color, 6f);
            }
        }

        private void CheckForInteractables()
        {
            var testlag = DevConsole.GetVariable<bool>("net.testlag");

            if (!_human.Game.IsHost)
            {
                if (testlag)
                {
                    var ownerRay = _human.GetEyeRay();
                    Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.blue, 4);
                    DrawEntities(Color.green, 1.5f);
                }
                return;
            }

            var latency = _human.Game.PlayerManager.FindPlayer(_human).LatencyMs / 1000f;

            _human.DisableLagCompensation = true;
            _human.Game.LagCompensator.Rewind(latency);

            if (testlag)
            {
                var ownerRay = _human.GetEyeRay();
                Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.magenta, 4);
                DrawEntities(Color.yellow);
            }

            var ray = _human.GetEyeRay();
            var ragdollLayer = Layers.Ragdoll;

            var hitCount = _human.Game.Physics.RaycastAll(ray: ray,
                results: _hitBuffer,
                maxDistance: _interactionDistance,
                layerMask: (1 << _human.Game.ScopeLayer) | (1 << ragdollLayer) | (1 << 0),
                qt: QueryTriggerInteraction.Collide);

            // important to call restore after tracing!!
            _human.Game.LagCompensator.Restore();
            _human.DisableLagCompensation = false;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = _hitBuffer[i];
                var trigger = hit.collider.GetComponentInParent<FSMTrigger>();

                if (trigger != null)
                {
                    trigger.OnInteract(_human.EntityId, _human.Game.IsHost);
                    return;
                }

                var ent = _human.Game.EntityManager.FindEntity(hit.collider.gameObject);
                if(ent is IInteractable interactable)
                {
                    interactable.OnInteract(_human);
                    break;
                }
            }
        }

    }
}