using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Movement
{
    public class SurfPhysics
    {

        public enum StrictCollisionAxis
        {
            None,
            Horizontal,
            Vertical
        }

        ///// Fields /////

        public static int GroundLayerMask = (1 << 0);
        public static int LadderLayerMask = (1 << 1);
        private static Collider[] _colliders = new Collider[MaxCollisions];
        private static Vector3[] _planes = new Vector3[MaxClipPlanes];
        private const int MaxCollisions = 128;
        private const int MaxClipPlanes = 5;
        private const int NumBumps = 4;

        public const float SurfSlope = 0.7f;

        private struct CollisionData
        {
            public float Distance;
            public Vector3 Direction;
        }

        ///// Methods /////

        public static void ResolveCollisions(ISurfControllable controller, StrictCollisionAxis axis = StrictCollisionAxis.None)
        {
            var staticOrigin = controller.MoveData.Origin + new Vector3(0, controller.Collider.bounds.extents.y, 0);
            var numOverlaps = Physics.OverlapBoxNonAlloc(staticOrigin, controller.Collider.bounds.extents, _colliders,
                controller.Orientation, GroundLayerMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < numOverlaps; i++)
            {
                if (Physics.ComputePenetration(controller.Collider, controller.MoveData.Origin,
                    controller.Orientation, _colliders[i], _colliders[i].transform.position,
                    _colliders[i].transform.rotation, out Vector3 direction, out float distance))
                {
                    // don't resolve if moving away from it
                    if (Vector3.Dot(direction, controller.MoveData.Velocity.normalized) > 0)
                    {
                        continue;
                    }

                    var penetrationVec = direction * distance;
                    var velocityVec = -Vector3.Project(controller.MoveData.Velocity, -direction);

                    if (axis != StrictCollisionAxis.None)
                    {
                        var horizAdjustment = (Mathf.Abs(penetrationVec.x) + Mathf.Abs(penetrationVec.z)) > Mathf.Abs(penetrationVec.y);
                        if ((axis == StrictCollisionAxis.Horizontal && horizAdjustment)
                            || (axis == StrictCollisionAxis.Vertical && !horizAdjustment))
                        {
                            controller.MoveData.Origin += penetrationVec;
                            if (controller.MoveData.Surfing)
                            {
                                ClipVelocity(controller.MoveData.Velocity, direction, ref controller.MoveData.Velocity, 1.0f);
                            }
                            else
                            {
                                controller.MoveData.Velocity += velocityVec;
                            }
                        }
                        continue;
                    }

                    controller.MoveData.Origin += penetrationVec;
                    staticOrigin += penetrationVec;
                    if (controller.MoveData.Surfing)
                    {
                        ClipVelocity(controller.MoveData.Velocity, direction, ref controller.MoveData.Velocity, distance);
                    }
                    else
                    {
                        controller.MoveData.Velocity += velocityVec;
                    }
                }
            }
        }

        public static void Friction(ref Vector3 velocity, float stopSpeed, float friction, float deltaTime)
        {
            var speed = velocity.magnitude;

            if (speed < 0.0001905f)
            {
                return;
            }

            var drop = 0f;

            // apply ground friction
            var control = (speed < stopSpeed) ? stopSpeed : speed;
            drop += control * friction * deltaTime;

            // scale the velocity
            var newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;

            if (newspeed != speed)
            {
                newspeed /= speed;
                velocity *= newspeed;
            }
        }

        public static Vector3 AirAccelerate(Vector3 velocity, Vector3 wishdir, float wishspeed, float accel, float airCap, float deltaTime, out float gainCoefficient)
        {
            gainCoefficient = 0f;
            var wishSpd = wishspeed;

            // Cap speed
            if (wishSpd > airCap)
            {
                wishSpd = airCap;
            }

            // Determine veer amount
            var currentspeed = Vector3.Dot(velocity, wishdir);

            // See how much to add
            var addspeed = wishSpd - currentspeed;

            // If not adding any, done.
            if (addspeed <= 0)
            {
                return Vector3.zero;
            }

            // Determine acceleration speed after acceleration
            var accelspeed = accel * wishspeed * deltaTime;

            // Cap it
            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }

            if (currentspeed < airCap)
            {
                gainCoefficient = (wishSpd - Mathf.Abs(currentspeed)) / wishSpd;
            }

            return accelspeed * wishdir;
        }

        public static Vector3 Accelerate(Vector3 currentVelocity, Vector3 wishdir, float wishspeed, float accel, float deltaTime, float surfaceFriction)
        {
            // See if we are changing direction a bit
            var currentspeed = Vector3.Dot(currentVelocity, wishdir);

            // Reduce wishspeed by the amount of veer.
            var addspeed = wishspeed - currentspeed;

            // If not going to add any speed, done.
            if (addspeed <= 0)
            {
                return Vector3.zero;
            }

            // Determine amount of accleration.
            var accelspeed = accel * deltaTime * wishspeed * surfaceFriction;

            // Cap at addspeed
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            return accelspeed * wishdir;
        }

        private static RaycastHit[] _hits = new RaycastHit[32];

        public static void Reflect(ISurfControllable surfer, float deltaTime, Vector3 hitNormal)
        {
            var workingVel = surfer.MoveData.JustGrounded
                ? surfer.MoveData.PreGroundedVelocity
                : surfer.MoveData.Velocity;
            workingVel += surfer.MoveData.BaseVelocity;

            if (hitNormal.y <= SurfSlope
                || hitNormal.y >= 1)
            {
                return;
            }

            ClipVelocity(workingVel, hitNormal, ref workingVel, 1f);

            workingVel -= surfer.MoveData.BaseVelocity;
            surfer.MoveData.Velocity = workingVel;
        }

        public static void Reflect(ISurfControllable surfer, float deltaTime)
        {
            var workingVel = surfer.MoveData.JustGrounded
                ? surfer.MoveData.PreGroundedVelocity
                : surfer.MoveData.Velocity;
            workingVel += surfer.MoveData.BaseVelocity;
            var clipped = false;

            //if (contactOffset != 0)
            //{
            //    var longSide = Mathf.Sqrt(contactOffset * contactOffset + contactOffset * contactOffset);
            //    distance += longSide;
            //    extents *= (1f - contactOffset);
            //}

            //var center = surfer.MoveData.Origin + new Vector3(0, surfer.Collider.bounds.extents.y, 0);
            //var end = center + (Vector3.down * 0.2f) + (workingVel * deltaTime);
            //var dist = Vector3.Distance(center, end);
            //var dir = (end - center).normalized;

            var center = surfer.MoveData.Origin + new Vector3(0, surfer.Collider.bounds.extents.y + 0.1f, 0);
            var dist = 0.2f;
            var dir = (Vector3.down + (workingVel * deltaTime)).normalized;
            //var dir = (Vector3.down + (workingVel * deltaTime)).normalized;

            var hitCount = Physics.BoxCastNonAlloc(results: _hits,
                center: center,
                direction: dir,
                orientation: surfer.Orientation,
                maxDistance: dist,
                halfExtents: surfer.Collider.bounds.extents,
                layerMask: GroundLayerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (Tracer.HitIsShit(_hits[i])
                    || _hits[i].collider == null)
                {
                    continue;
                }

                if (_hits[i].normal.y <= SurfSlope
                    || _hits[i].normal.y >= 1)
                {
                    continue;
                }

                var slopeDir = Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, _hits[i].normal));
                var dot = Vector3.Dot(workingVel.normalized, slopeDir);
                var goingAgainstSlope = dot > 0;

                if (!goingAgainstSlope)
                {
                    continue;
                }

                ClipVelocity(workingVel, _hits[i].normal, ref workingVel, 1f);
                clipped = true;
            }

            workingVel -= surfer.MoveData.BaseVelocity;

            if (clipped)
            {
                surfer.MoveData.Velocity = workingVel;
            }

            //var oldVelocityMagnitude2d = new Vector2(surfer.MoveData.Velocity.x, surfer.MoveData.Velocity.z).magnitude;

            //

            //var newVelocityMagnitude2d = new Vector2(surfer.MoveData.Velocity.x, surfer.MoveData.Velocity.z).magnitude;
            //float fLateralStoppingAmount = oldVelocityMagnitude2d - newVelocityMagnitude2d;
            //Debug.Log(fLateralStoppingAmount);
        }

        public static void ClipVelocity(Vector3 input, Vector3 normal, ref Vector3 output, float overbounce, bool debug = false)
        {
            // Determine how far along plane to slide based on incoming direction.
            var backoff = Vector3.Dot(input, normal) * overbounce;

            for (int i = 0; i < 3; i++)
            {
                var change = normal[i] * backoff;
                output[i] = input[i] - change;
            }

            // iterate once to make sure we aren't still moving through the plane
            float adjust = Vector3.Dot(output, normal);
            if (adjust < 0.0f)
            {
                output -= (normal * adjust);
            }

            if (debug) Debug.Log(input + ":" + normal + ":" + output);
        }

        public static float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f) angle = 360 + angle;
            if (angle > 180f) return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }

    }
}
