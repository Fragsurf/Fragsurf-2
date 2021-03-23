using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Packets;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared
{
    public struct EntityFrame
    {
        public EntityFrame(NetEntity ent)
        {
            CurTime = 0;
            UserCmdFields = new UserCmd.CmdFields();
            Position = ent.Origin;
            Angles = ent.Angles;
        }
        public double CurTime;
        public UserCmd.CmdFields UserCmdFields;
        public Vector3 Position;
        public Vector3 Angles;

        public static EntityFrame Interpolate(EntityFrame from, EntityFrame to, float alpha)
        {
            var result = new EntityFrame()
            {
                Position = Vector3.Lerp(from.Position, to.Position, alpha),
                Angles = Quaternion.Slerp(Quaternion.Euler(from.Angles), Quaternion.Euler(to.Angles), alpha).eulerAngles
            };
            return result;
        }
    }

    public class FrameInterpolator : FSSharedScript
    {

        private struct FrameStates
        {
            public EntityFrame From;
            public EntityFrame To;
        }

        private Dictionary<NetEntity, FrameStates> _frameStates = new Dictionary<NetEntity, FrameStates>();

        protected override void OnEntityAdded(NetEntity entity)
        {
            _frameStates.Add(entity, new FrameStates());
        }

        protected override void OnEntityDestroyed(NetEntity entity)
        {
            _frameStates.Remove(entity);
        }

        protected override void _Tick()
        {
            foreach (var ent in Game.EntityManager.Entities)
            {
                if (ent.EntityGameObject
                    && ent.InterpolationMode == InterpolationMode.Frame)
                {
                    var newFromFrame = _frameStates[ent].To;
                    var newToFrame = new EntityFrame(ent);
                    _frameStates[ent] = new FrameStates()
                    {
                        From = newFromFrame,
                        To = newToFrame
                    };
                }
            }
        }

        protected override void _Update()
        {
            foreach (var ent in Game.EntityManager.Entities)
            {
                if (ent.EntityGameObject == null
                    || ent.InterpolationMode == InterpolationMode.None)
                {
                    continue;
                }

                switch (ent.InterpolationMode)
                {
                    case InterpolationMode.Frame:
                        var lerped = EntityFrame.Interpolate(_frameStates[ent].From, _frameStates[ent].To, (float)Game.Alpha);
                        ent.EntityGameObject.Position = lerped.Position;
                        ent.EntityGameObject.Rotation = lerped.Angles;
                        break;
                    case InterpolationMode.Snap:
                        ent.EntityGameObject.Position = ent.Origin;
                        ent.EntityGameObject.Rotation = ent.Angles;
                        break;
                }

            }
        }

    }
}
