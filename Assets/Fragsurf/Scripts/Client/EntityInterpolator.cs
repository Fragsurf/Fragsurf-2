using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Client
{
    public class EntityInterpolator : FSClientScript
    {

        private Dictionary<NetEntity, EntityInterpState> _history
            = new Dictionary<NetEntity, EntityInterpState>();

        protected override void OnEntityAdded(NetEntity entity)
        {
            _history.Add(entity, new EntityInterpState());
        }

        protected override void OnEntityDestroyed(NetEntity entity)
        {
            _history.Remove(entity);
        }

        protected override void _Destroy()
        {
            base._Destroy();

            _history.Clear();
        }

        protected override void _Update()
        {
            DoInterpolation();
        }

        protected override void OnEntityUpdated(NetEntity entity, int remoteTick, double remoteTime)
        {
            if (entity.EntityGameObject
                && entity.OriginOrRotationChanged)
            {
                //var rt = (Game.Network as ClientSocketManager).GetLocalTick(remoteTick);
                var time = (Game.Network as ClientSocketManager).GetLocalTime((float)remoteTime);

                var nextFrame = new EntityFrame()
                {
                    CurTime = time,
                    Position = entity.Origin,
                    Angles = entity.Angles
                };

                _history[entity].AddFrame(nextFrame);

                entity.OriginOrRotationChanged = false;
            }
        }

        private void DoInterpolation()
        {
            var interpTime = Game.EntityManager.InterpDelay;

            foreach (KeyValuePair<NetEntity, EntityInterpState> pair in _history)
            {
                var entity = pair.Key;
                var interp = pair.Value;

                if (entity.InterpolationMode != InterpolationMode.Network)
                {
                    continue;
                }

                var lt = Game.CurrentTick;
                var latency = interpTime + ((Game.Network as ClientSocketManager).AverageRoundtripTime / 2f);
                var renderTimestamp = Game.ElapsedTime - latency;

                if (interp.FrameCount > 1 && interp[0].CurTime > renderTimestamp)
                {
                    for (int i = 0; i < interp.FrameCount; i++)
                    {
                        if (interp[i].CurTime <= renderTimestamp)
                        {
                            var toFrame = interp[Mathf.Max(i - 1, 0)];
                            var fromFrame = interp[i];
                            var t = (float)(renderTimestamp - fromFrame.CurTime) / (float)(toFrame.CurTime - fromFrame.CurTime);
                            var lerpedPos = Vector3.Lerp(fromFrame.Position, toFrame.Position, t);
                            var lerpedAngle = Quaternion.Slerp(Quaternion.Euler(fromFrame.Angles), Quaternion.Euler(toFrame.Angles), t);
                            entity.EntityGameObject.Position = lerpedPos;
                            entity.EntityGameObject.Rotation = lerpedAngle.eulerAngles;
                            break;
                        }
                    }
                }
                else if (interp.FrameCount > 0)
                {
                    entity.EntityGameObject.Position = interp[0].Position;
                    entity.EntityGameObject.Rotation = interp[0].Angles;
                }
            }
        }

    }
}