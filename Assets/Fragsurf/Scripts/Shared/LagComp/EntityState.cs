using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.LagComp
{
    public class EntityState
    {
        public const int MAX_HITBOXES = 24;
        public int EntityId;
        public Vector3 Origin;
        public Vector3 Angles;
        public Vector3 CurrentOrigin;
        public Vector3 CurrentAngles;
        public List<Vector3> CurrentAnimState = new List<Vector3>(MAX_HITBOXES * 2);
        public List<Vector3> AnimState = new List<Vector3>(MAX_HITBOXES * 2);
    }
}