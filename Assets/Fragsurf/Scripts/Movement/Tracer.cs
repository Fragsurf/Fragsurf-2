using UnityEngine;

namespace Fragsurf.Movement
{
    public class Tracer
    {
        private static RaycastHit[] _results = new RaycastHit[32];

        public static Trace TraceRay(Vector3 origin, Vector3 direction, float maxDistance, int layerMask)
        {
            var result = new Trace()
            {
                StartPos = origin,
                EndPos = origin + direction * maxDistance,
                Fraction = 1
            };

            if (!Physics.Raycast(origin: origin, 
                direction: direction, 
                hitInfo: out RaycastHit hit,
                maxDistance: maxDistance, 
                layerMask: layerMask))
            {
                return result;
            }

            result = hit;
            result.Fraction = hit.distance / maxDistance;

            return result;
        }

        public static Trace TraceBox(BoxCollider collider, Vector3 origin, Vector3 end, int layerMask, bool shrink = true)
        {
            var bounds = collider.bounds;
            var trOrigin = origin + new Vector3(0, bounds.extents.y, 0);
            var trEnd = end + new Vector3(0, bounds.extents.y, 0);
            var trace = TraceBox(trOrigin, trEnd, bounds.extents, shrink ? collider.contactOffset : 0, layerMask);
            trace.EndPos = Vector3.Lerp(origin, end, trace.Fraction);
            return trace;
        }

        public static int TraceBoxNonAlloc(BoxCollider collider, Vector3 origin, Vector3 end, Trace[] results, int layerMask, bool shrink = true)
        {
            var bounds = collider.bounds;
            var trOrigin = origin + new Vector3(0, bounds.extents.y, 0);
            var trEnd = end + new Vector3(0, bounds.extents.y, 0);
            var hits = TraceBoxNonAlloc(trOrigin, trEnd, bounds.extents, results, shrink ? collider.contactOffset : 0, layerMask);
            for(int i = 0; i < hits; i++)
            {
                results[i].EndPos = Vector3.Lerp(origin, end, results[i].Fraction);
            }
            return hits;
        }

        public static Trace TraceBoxForTag(BoxCollider collider, Vector3 origin, Vector3 end, int layerMask, string tag)
        {
            var bounds = collider.bounds;
            origin += new Vector3(0, bounds.extents.y, 0);
            end += new Vector3(0, bounds.extents.y, 0);
            return TraceBoxForTag(origin, end, bounds.extents, collider.contactOffset, layerMask, tag);
        }

        public static Trace TraceBox(Vector3 center, Vector3 destination, Vector3 extents, float contactOffset, int layerMask)
        {
            var result = new Trace()
            {
                StartPos = center,
                EndPos = destination,
                Fraction = 1
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - center).normalized;
            var maxDistance = Vector3.Distance(center, destination) + longSide;
            extents *= (1f - contactOffset);

            if (Physics.BoxCast(center: center,
                halfExtents: extents,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                hitInfo: out RaycastHit hit,
                layerMask: layerMask))
            {
                result = hit;
                result.Fraction = Mathf.Approximately(hit.distance, maxDistance) ? 1.0f : hit.distance / maxDistance;
                result.EndPos = Vector3.Lerp(center, destination, result.Fraction);
            }

            return result;
        }

        public static int TraceBoxNonAlloc(Vector3 center, Vector3 destination, Vector3 extents, Trace[] results, float contactOffset, int layerMask)
        {
            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - center).normalized;
            var maxDistance = Vector3.Distance(center, destination) + longSide;
            extents *= (1f - contactOffset);

            var hits = Physics.BoxCastNonAlloc(center: center,
                halfExtents: extents,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                results: _results,
                layerMask: layerMask);

            for (int i = 0; i < hits; i++)
            {
                Trace trace = _results[i];
                trace.Fraction = _results[i].distance / maxDistance;
                trace.StartPos = center;
                trace.EndPos = Vector3.Lerp(center, destination, trace.Fraction);
            }

            return hits;
        }

        public static Trace TraceBoxForTag(Vector3 center, Vector3 destination, Vector3 extents, float contactOffset, int layerMask, string tag)
        {
            var result = new Trace()
            {
                StartPos = center,
                EndPos = destination,
                Fraction = 1
            };

            var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            var direction = (destination - center).normalized;
            var maxDistance = Vector3.Distance(center, destination) + longSide;
            extents *= (1f - contactOffset);

            var count = Physics.BoxCastNonAlloc(center: center,
                halfExtents: extents,
                direction: direction,
                results: _results,
                orientation: Quaternion.identity,
                maxDistance: maxDistance,
                layerMask: layerMask);

            for(int i = 0; i < count; i++)
            {
                if (!_results[i].collider.CompareTag(tag))
                    continue;
                result = _results[i];
                result.Fraction = _results[i].distance / maxDistance;
                result.EndPos = Vector3.Lerp(center, destination, result.Fraction);
                break;
            }

            return result;
        }

        public static bool HitIsShit(RaycastHit hit)
        {
            return hit.distance == 0 && hit.point == Vector3.zero && hit.normal != Vector3.zero;
        }

    }
}