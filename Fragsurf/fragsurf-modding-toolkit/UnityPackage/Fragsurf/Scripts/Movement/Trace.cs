using UnityEngine;

namespace Fragsurf.Movement
{
    public enum TraceContents
    {
        Default = 1 << 0,
        Ladder = 1 << 1,
        Trigger = 1 << 2
    }
    public struct Trace
    {
        public Vector3 StartPos;
        public Vector3 EndPos;
        public float Fraction;
        public bool StartSolid;
        public Collider HitCollider;
        public Vector3 HitPoint;
        public Vector3 PlaneNormal;
        public float Distance;
        public TraceContents Contents;
        public bool IsShit;

        public static implicit operator Trace(RaycastHit hit)
        {
            var result = new Trace();
            //result.Fraction = hit.distance / maxDistance;
            result.HitCollider = hit.collider;
            result.HitPoint = hit.point;
            result.PlaneNormal = hit.normal;
            result.Distance = hit.distance;
            result.IsShit = Tracer.HitIsShit(hit);
            if (result.IsShit) result.Fraction = 0;

            if(hit.collider != null)
            {
                if (hit.collider.CompareTag("Ladder"))
                {
                    result.Contents |= TraceContents.Ladder;
                }
                else if (hit.collider.CompareTag("Trigger"))
                {
                    result.Contents |= TraceContents.Trigger;
                }
            }

            return result;
        }
    }
}
