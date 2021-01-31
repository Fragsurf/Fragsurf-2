using System;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Maps;
using Fragsurf.Movement;

namespace Fragsurf.Client
{
    //public class FootstepController : FSClientScript
    //{

    //    private class FootstepData
    //    {
    //        public float Timer;
    //        public bool FirstStep;
    //        public Vector3 PrevVelocity;
    //    }

    //    private Dictionary<Player, FootstepData> _footstepData
    //        = new Dictionary<Player, FootstepData>();

    //    private const float footstepTimer = 0.48f;

    //    protected override void OnEntityAdded(NetEntity entity)
    //    {
    //        if (entity is Player hu)
    //        {
    //            _footstepData.Add(hu, new FootstepData());
    //        }
    //    }

    //    protected override void OnEntityDestroyed(NetEntity entity)
    //    {
    //        if (entity is Player hu)
    //        {
    //            _footstepData.Remove(hu);
    //        }
    //    }

    //    protected override void OnPlayerRunCommand(IPlayer player)
    //    {
    //        Player human = null;
    //        var data = _footstepData[human];
    //        var justGrounded = human.Movement.MoveData.JustGrounded;
    //        var curVelocity = human.Velocity;
    //        var prevVelocity = data.PrevVelocity;

    //        if (human.Dead || curVelocity.magnitude < 0.4f)
    //        {
    //            return;
    //        }

    //        data.Timer -= Time.fixedDeltaTime;

    //        if (data.Timer <= 0)
    //        {
    //            if (human.Movement.GroundObject != null || human.Movement.MoveType == MoveType.Ladder)
    //            {
    //                var surfaceMat = GetSurfaceMaterialType(human);
    //                var vol = Mathf.Min(1, curVelocity.magnitude / 5f);
    //                SoundManager.PlaySoundAttached(DataSet.Instance.Footstep, vol, human.HumanGameObject.HeadAttachment, "FootstepMaterial", (float)surfaceMat);
    //                data.Timer = footstepTimer;

    //                if (surfaceMat == SurfaceMaterialType.Water)
    //                {
    //                    if (DataSet.Instance.FootstepSplash != null)
    //                    {
    //                        var effect = Game.Pool.Get(DataSet.Instance.FootstepSplash, 1.5f);
    //                        effect.transform.position = human.HumanGameObject.FeetAttachment.position + new Vector3(0, 0.15f, 0);
    //                        effect.transform.forward = Vector3.up;
    //                    }
    //                }
    //            }
    //        }

    //        data.PrevVelocity = curVelocity;
    //    }

    //    private SurfaceMaterialType GetSurfaceMaterialType(Human human)
    //    {
    //        if (human.Movement.MoveType == MoveType.Ladder)
    //        {
    //            return SurfaceMaterialType.Ladder;
    //        }

    //        if (human.Movement.MoveData.InWater)
    //        {
    //            return SurfaceMaterialType.Water;
    //        }

    //        var parameterValue = SurfaceMaterialType.Concrete;

    //        if (Physics.BoxCast(center: human.Origin + Vector3.up,
    //            halfExtents: new Vector3(0.5f, 0.5f, 0.5f),
    //            orientation: Quaternion.identity,
    //            direction: Vector3.down,
    //            maxDistance: 3f,
    //            layerMask: 1 << LayerMask.NameToLayer("TransparentFX"),
    //            queryTriggerInteraction: QueryTriggerInteraction.Ignore,
    //            hitInfo: out RaycastHit hit))
    //        {
    //            if (hit.collider.TryGetComponent(out SurfaceMaterialIdentifier surfProp))
    //            {
    //                parameterValue = surfProp.MaterialType;
    //            }
    //        }

    //        return parameterValue;
    //    }

    //}
}

