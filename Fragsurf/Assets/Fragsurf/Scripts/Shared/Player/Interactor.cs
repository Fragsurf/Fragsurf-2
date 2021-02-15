using UnityEngine;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Entity;
using Fragsurf.Movement;
using Fragsurf.Actors;

namespace Fragsurf.Shared.Player
{
    //public class Interactor
    //{
    //    public Interactor(NetEntity entity)
    //    {
    //        _entity = entity;
    //    }

    //    private NetEntity _entity;
    //    private RaycastHit[] _hitBuffer = new RaycastHit[24];
    //    private float _interactionDistance = 2.5f;

    //    public void RunCommand(UserCmd.CmdFields userCmd)
    //    {
    //        //if (!_human.Game.IsHost)
    //        //    return;

    //        if (userCmd.Buttons.HasFlag(InputActions.Interact))
    //        {
    //            CheckForInteractables();
    //        }
    //    }

    //    private void DrawEntities(Color color, float scale = 1)
    //    {
    //        foreach (NetEntity ent in _entity.Game.EntityManager.Entities)
    //        {
    //            if (ent.DisableLagCompensation)
    //            {
    //                continue;
    //            }
    //            Debug.DrawLine(ent.EntityGameObject.Position, ent.EntityGameObject.Position + Vector3.up * scale, color, 6f);
    //        }
    //    }

    //    private void CheckForInteractables()
    //    {
    //        var testlag = DevConsole.GetVariable<bool>("net.testlag");

    //        if (!_entity.Game.IsHost)
    //        {
    //            if (testlag)
    //            {
    //                var ownerRay = _entity.GetEyeRay();
    //                Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.blue, 4);
    //                DrawEntities(Color.green, 1.5f);
    //            }
    //            return;
    //        }

    //        var latency = _entity.TickTimeDiff;

    //        _entity.DisableLagCompensation = true;
    //        _entity.Game.LagCompensator.Rewind(latency);

    //        if (testlag)
    //        {
    //            var ownerRay = _entity.GetEyeRay();
    //            Debug.DrawRay(ownerRay.origin, ownerRay.direction * 32, Color.magenta, 4);
    //            DrawEntities(Color.yellow);
    //        }

    //        var ray = _entity.GetEyeRay();
    //        var ragdollLayer = Layers.Ragdoll;

    //        var hitCount = _entity.Game.Physics.RaycastAll(ray: ray,
    //            results: _hitBuffer,
    //            maxDistance: _interactionDistance,
    //            layerMask: (1 << _entity.Game.ScopeLayer) | (1 << ragdollLayer) | (1 << 0),
    //            qt: QueryTriggerInteraction.Collide);

    //        // important to call restore after tracing!!
    //        _entity.Game.LagCompensator.Restore();
    //        _entity.DisableLagCompensation = false;

    //        for (int i = 0; i < hitCount; i++)
    //        {
    //            var hit = _hitBuffer[i];
    //            var trigger = hit.collider.GetComponentInParent<FSMTrigger>();

    //            if (trigger != null)
    //            {
    //                trigger.OnInteract(_entity.EntityId, _entity.Game.IsHost);
    //                return;
    //            }

    //            var ent = _entity.Game.EntityManager.FindEntity(hit.collider.gameObject);

    //            if(!(ent != null && ent is IInteractable interactable))
    //            {
    //                interactable = hit.collider.GetComponentInParent<IInteractable>();
    //            }

    //            interactable?.OnInteract(_entity);
    //        }
    //    }

    //}
}