using UnityEngine;

namespace Fragsurf.Movement
{
    public class SurfController
    {

        private ISurfControllable _surfer;
        private MovementConfig _config;
        private float _deltaTime;
        private static RaycastHit[] _hitCache = new RaycastHit[32];

        public const float HammerScale = .0254f;

        public void CalculateMovement(ISurfControllable surfer, MovementConfig config, float deltaTime)
        {
            // cache instead of passing around parameters
            _surfer = surfer;
            _config = config;
            _deltaTime = deltaTime;

            _surfer.MoveData.JustGrounded = false;
            _surfer.MoveData.JustJumped = false;

            if (surfer.MoveData.LimitedExecution)
            {
                CalculateSyncMovement();
            }
            else
            {
                CheckParameters();
                CalculateFullMovement();
            }

            _surfer = null;
            _config = null;
        }

        private void CheckParameters()
        {
            var spd = (_surfer.MoveData.ForwardMove * _surfer.MoveData.ForwardMove) +
              (_surfer.MoveData.SideMove * _surfer.MoveData.SideMove) +
              (_surfer.MoveData.UpMove * _surfer.MoveData.UpMove);

            spd = Mathf.Sqrt(spd);
            if ((spd != 0.0) && (spd > _config.MaxSpeed))
            {
                float fRatio = _config.MaxSpeed / spd;
                _surfer.MoveData.ForwardMove *= fRatio;
                _surfer.MoveData.SideMove *= fRatio;
                _surfer.MoveData.UpMove *= fRatio;
            }
            //frozen?
        }

        private void CalculateSyncMovement()
        {
            _surfer.MoveData.Style = GetMoveStyle(_surfer.MoveData.ViewAngles, _surfer.MoveData.Velocity);

            switch (_surfer.MoveType)
            {
                case MoveType.Walk:
                    //CheckGrounded();
                    CheckDuck();
                    break;
            }
        }

        private void CalculateFullMovement()
        {
            _surfer.MoveData.Style = GetMoveStyle(_surfer.MoveData.ViewAngles, _surfer.MoveData.Velocity);

            if (_surfer.MoveType != MoveType.Noclip)
            {
                if (!LadderMove()
                    && _surfer.MoveType == MoveType.Ladder)
                {
                    _surfer.MoveType = MoveType.Walk;
                }
            }

            switch (_surfer.MoveType)
            {
                case MoveType.Walk:
                    ApplyMomentum();
                    ApplyGravity();
                    CheckDuck();
                    CheckJump();
                    CalculateWalkVelocity();
                    CheckSteps();
                    ClampVelocity();
                    IncrementOrigin(_surfer.MoveData.AbsVelocity * _deltaTime);
                    break;
                case MoveType.Ladder:
                    IncrementOrigin(_surfer.MoveData.Velocity * _deltaTime);
                    break;
                case MoveType.Swim:
                    CalculateWalkVelocity(0.3f);
                    WaterMove();
                    ApplyMomentum();
                    ClampVelocity();
                    IncrementOrigin(_surfer.MoveData.AbsVelocity * _deltaTime);
                    break;
                case MoveType.Noclip:
                    CalculateNoclipVelocity();
                    CheckGrounded();
                    IncrementOrigin(_surfer.MoveData.Velocity * _deltaTime);
                    return;
            }

            if (_surfer.MoveType != MoveType.Swim)
            {
                CheckGrounded();
            }

            CheckWater();
        }

        private void ClampVelocity()
        {
            for (int i = 0; i < 3; i++)
            {
                _surfer.MoveData.Velocity[i] = Mathf.Clamp(_surfer.MoveData.Velocity[i], -_config.MaxVelocity, _config.MaxVelocity);
            }
        }

        private bool LadderMove()
        {
            Vector3 wishDir = Vector3.zero;

            if (_surfer.MoveType == MoveType.Ladder)
            {
                wishDir = -_surfer.MoveData.LadderNormal;
            }
            else
            {
                if (_surfer.MoveData.ForwardMove != 0 || _surfer.MoveData.SideMove != 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        wishDir[i] = _surfer.Forward[i] * _surfer.MoveData.ForwardMove + _surfer.Right[i] * _surfer.MoveData.SideMove;
                    }
                    wishDir = wishDir.normalized;
                }
                else
                {
                    return false;
                }
            }

            var start = _surfer.MoveData.Origin;
            start.y += _surfer.Collider.bounds.extents.y;
            var end = VectorExtensions.VectorMa(start, _config.LadderDistance, wishDir);
            var trace = Tracer.TraceBoxForTag(_surfer.Collider, start, end, ~0, "Ladder");
            if (trace.HitCollider == null)
            {
                return false;
            }

            _surfer.MoveType = MoveType.Ladder;
            _surfer.MoveData.LadderNormal = trace.PlaneNormal;

            var climbSpeed = _config.MaxClimbSpeed;

            if (_surfer.MoveData.Buttons.HasFlag(InputActions.Jump))
            {
                _surfer.MoveType = MoveType.Walk;
                _surfer.MoveData.Velocity = trace.PlaneNormal * _config.JumpOffLadderSpeed;
            }
            else
            {
                float forwardSpeed = 0, rightSpeed = 0;

                if (_surfer.MoveData.Buttons.HasFlag(InputActions.MoveBack))
                    forwardSpeed -= climbSpeed;
                if (_surfer.MoveData.Buttons.HasFlag(InputActions.MoveForward))
                    forwardSpeed += climbSpeed;
                if (_surfer.MoveData.Buttons.HasFlag(InputActions.MoveLeft))
                    rightSpeed -= climbSpeed;
                if (_surfer.MoveData.Buttons.HasFlag(InputActions.MoveRight))
                    rightSpeed += climbSpeed;

                if (forwardSpeed != 0 || rightSpeed != 0)
                {
                    var fwd = Quaternion.Euler(_surfer.MoveData.ViewAngles) * Vector3.forward;
                    var right = Quaternion.Euler(_surfer.MoveData.ViewAngles) * Vector3.right;
                    Vector3 velocity, perp, cross, lateral, tmp;
                    velocity = fwd * forwardSpeed + rightSpeed * right;
                    tmp = Vector3.zero;
                    tmp[1] = HammerScale;
                    perp = Vector3.Cross(tmp, trace.PlaneNormal).normalized;
                    float normal = Vector3.Dot(velocity, trace.PlaneNormal);
                    cross = trace.PlaneNormal * normal;
                    lateral = velocity - cross;
                    tmp = Vector3.Cross(trace.PlaneNormal, perp);

                    // if cstrike.dll
                    float tmpDist = Vector3.Dot(tmp, lateral);
                    float perpDist = Vector3.Dot(perp, lateral);
                    var angleVec = perp * perpDist;
                    angleVec += cross;
                    angleVec.Normalize();
                    float angleDot = Vector3.Dot(angleVec, trace.PlaneNormal);
                    if (angleDot < _config.LadderAngle)
                    {
                        lateral = (tmp * tmpDist) + (perp * _config.LadderDampen * perpDist);
                    }
                    // endif

                    _surfer.MoveData.Velocity = lateral + -normal * tmp;

                    if (_surfer.GroundObject != null && normal > 0)  // On ground moving away from the ladder
                    {
                        _surfer.MoveData.Velocity += _config.MaxClimbSpeed * trace.PlaneNormal;
                    }
                }
                else
                {
                    _surfer.MoveData.Velocity = Vector3.zero;
                }
            }

            return true;
        }

        private void WaterMove()
        {
            GetWishValues(_surfer, _config, out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed);

            if(_surfer.MoveData.ForwardMove != 0)
            {
                wishVel.y = (Quaternion.Euler(_surfer.MoveData.ViewAngles) * Vector3.forward).y * _config.WaterSwimSpeed;
            }

            if (_surfer.MoveData.Buttons.HasFlag(InputActions.Jump))
            {
                _surfer.MoveData.Velocity.y = _config.WaterJumpPower;
                wishVel.y += _config.MaxSpeed;
            }
            else
            {
                wishVel.y -= _config.WaterSinkSpeed;
            }

            if(_surfer.MoveData.WaterDepth <= _config.WaterDepthToJumpOut
                && _surfer.MoveData.Buttons.HasFlag(InputActions.Jump))
            {
                var extents = _surfer.StandingExtents;
                extents.y = .1f;
                extents.x *= 1.1f;
                extents.z *= 1.1f;
                if (Physics.CheckBox(_surfer.MoveData.Origin, extents, Quaternion.identity, 1 << 0, QueryTriggerInteraction.Ignore))
                {
                    _surfer.MoveData.Velocity.y += _config.WaterJumpOutPower;
                }
            }

            wishDir = wishVel;
            wishSpeed = wishDir.magnitude;
            wishDir.Normalize();

            if (wishSpeed >= _config.MaxSpeed)
            {
                wishVel *= _config.MaxSpeed / wishSpeed;
                wishSpeed = _config.MaxSpeed;
            }

            wishSpeed *= 0.5f;

            // water friction
            var speed = _surfer.MoveData.Velocity.magnitude;
            var newspeed = 0f;
            if (speed > 0)
            {
                var surfaceFriction = _surfer.MoveData.Buttons.HasFlag(InputActions.Jump) ? 0f : 1f;
                newspeed = speed - _deltaTime * speed * _config.WaterFriction * surfaceFriction;
                if (newspeed < 0.1f)
                {
                    newspeed = 0f;
                }
                _surfer.MoveData.Velocity *= newspeed / speed;
            }
            else
            {
                speed = 0f;
            }

            // water acceleration
            var addspeed = 0f;
            var accelspeed = 0f;
            if (wishSpeed > 0.1f)
            {
                addspeed = wishSpeed - newspeed;
                if (addspeed > 0)
                {
                    wishVel.Normalize();
                    accelspeed = _config.Accelerate * wishSpeed * _deltaTime * _surfer.MoveData.SurfaceFriction;
                    if (accelspeed > addspeed)
                    {
                        accelspeed = addspeed;
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        float deltaSpeed = accelspeed * wishVel[i];
                        _surfer.MoveData.Velocity[i] += deltaSpeed;
                    }
                }
            }
        }

        private void ApplyMomentum()
        {
            if (!_surfer.MoveData.Momentum)
            {
                if (_surfer.MoveData.MomentumModifier != Vector3.zero)
                {
                    var v = _surfer.MoveData.Velocity;
                    var bv = _surfer.MoveData.BaseVelocity;
                    var n = _surfer.MoveData.MomentumModifier;
                    var bvi = Vector3.ProjectOnPlane(bv, n).normalized * bv.magnitude;
                    var vi = Vector3.ProjectOnPlane(v, n).normalized * v.magnitude;
                    _surfer.MoveData.BaseVelocity = bvi * (bvi.magnitude / bv.magnitude);
                    _surfer.MoveData.Velocity = vi * (vi.magnitude / v.magnitude);
                    _surfer.MoveData.MomentumModifier = Vector3.zero;
                }

                _surfer.MoveData.Velocity += (1.0f + (_deltaTime * 0.5f)) * _surfer.MoveData.BaseVelocity;
                _surfer.MoveData.BaseVelocity = Vector3.zero;
            }
            else
            {
                CheckSlope();
            }
            _surfer.MoveData.Momentum = false;
        }

        private void ApplyGravity()
        {
            if (_surfer.GroundObject == null || _surfer.MoveData.GravityFactor < 0)
            {
                _surfer.MoveData.Velocity.y -= _surfer.MoveData.GravityFactor * _config.Gravity * _deltaTime;
                _surfer.MoveData.Velocity.y += _surfer.MoveData.BaseVelocity.y * _deltaTime;
                _surfer.MoveData.BaseVelocity.y = 0;
            }
        }

        private void CalculateWalkVelocity(float modifier = 1.0f)
        {
            if (_surfer.GroundObject == null)
            {
                _surfer.MoveData.Velocity += AirInputMovement() * modifier;
                if (_surfer.MoveData.Buttons.HasFlag(InputActions.Brake))
                {
                    var vely = _surfer.MoveData.Velocity.y;
                    SurfPhysics.Friction(ref _surfer.MoveData.Velocity, 0f, _config.BrakeSpeed, _deltaTime);
                    _surfer.MoveData.Velocity.y = vely;
                }
            }
            else
            {
                _surfer.MoveData.Velocity += GroundInputMovement() * modifier;

                var friction = _surfer.MoveData.SurfaceFriction * _config.Friction;
                var stopSpeed = _config.StopSpeed;
                SurfPhysics.Friction(ref _surfer.MoveData.Velocity, stopSpeed, friction, _deltaTime);
            }
        }

        private void CalculateNoclipVelocity()
        {
            AngleVectors(_surfer.MoveData.ViewAngles, out Vector3 forward, out Vector3 right, out Vector3 up);

            var wishVel = Vector3.zero;
            for (int i = 0; i < 3; i++)
                wishVel[i] = forward[i] * _surfer.MoveData.ForwardMove + right[i] * _surfer.MoveData.SideMove;
            var wishDir = wishVel.normalized;

            _surfer.MoveData.Velocity += wishDir * _config.NoclipSpeed;
            SurfPhysics.Friction(ref _surfer.MoveData.Velocity, _config.StopSpeed, _config.NoclipFriction, _deltaTime);
        }

        private Vector3 GroundInputMovement()
        {
            GetWishValues(_surfer, _config, out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed);
            wishSpeed *= _surfer.MoveData.WalkFactor;

            return SurfPhysics.Accelerate(_surfer.MoveData.Velocity, wishDir,
                wishSpeed, _config.Accelerate, _deltaTime, _surfer.MoveData.SurfaceFriction);
        }

        private Vector3 AirInputMovement()
        {
            GetWishValues(_surfer, _config, out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed);
            var aircap = _surfer.MoveData.Surfing
                ? _config.AirCap * _config.AirCapSurfModifier
                : _config.AirCap;

            var result = SurfPhysics.AirAccelerate(_surfer.MoveData.Velocity, wishDir,
                wishSpeed, _config.AirAccel, aircap, _deltaTime, out _surfer.MoveData.GainCoefficient);

            return result;
        }

        public static void GetWishValues(ISurfControllable surfer, MovementConfig config, out Vector3 wishVel, out Vector3 wishDir, out float wishSpeed)
        {
            var forward = surfer.Forward;
            var right = surfer.Right;

            forward.y = 0;
            right.y = 0;

            wishVel = forward * surfer.MoveData.ForwardMove + right * surfer.MoveData.SideMove;
            wishVel.y = 0;

            wishSpeed = wishVel.magnitude;
            wishDir = wishVel.normalized;

            if (wishSpeed > config.MaxSpeed)
            {
                wishVel *= config.MaxSpeed / wishSpeed;
                wishSpeed = config.MaxSpeed;
            }
        }

        public static void AngleVectors(Vector3 angles, out Vector3 forward, out Vector3 right, out Vector3 up)
        {
            var quat = Quaternion.Euler(angles);
            forward = quat * Vector3.forward;
            right = quat * Vector3.right;
            up = quat * Vector3.up;
        }

        private bool MovingUpRapidly(float value = 0.0f)
        {
            if (value == 0.0f) value = _surfer.MoveData.Velocity.y;
            return value > _config.JumpPower * _config.MovingUpRapidlyFactor;
        }

        private bool CheckGrounded()
        {
            var wasSurfing = _surfer.MoveData.Surfing;

            _surfer.MoveData.SurfaceFriction = 1f;
            _surfer.MoveData.Surfing = false;
            _surfer.MoveData.Sliding = false;

            if (_surfer.MoveData.Buttons.HasFlag(InputActions.Slide))
            {
                _surfer.MoveData.Sliding = true;
            }

            var trace = BoxCastToFloor(.1f, .99f);
            var movingUp = _surfer.MoveData.Velocity.y > 0;
            var goingAgainstSlope = false;
            var quickJump = false;

            if (trace.HitCollider != null)
            {
                var slopeDir = Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, trace.PlaneNormal));
                var dot = Vector3.Dot(_surfer.MoveData.Velocity.normalized, slopeDir);
                goingAgainstSlope = dot > 0;

                if (trace.PlaneNormal.y <= SurfPhysics.SurfSlope)
                {
                    _surfer.MoveData.Surfing = true;
                    _surfer.MoveData.SurfNormal = trace.PlaneNormal;
                }
                else if (goingAgainstSlope
                    && dot >= _config.SlideDot)
                {
                    var tempVel = _surfer.MoveData.Velocity;
                    SurfPhysics.ClipVelocity(tempVel, trace.PlaneNormal, ref tempVel, 1.0f);
                    if (tempVel.y > _config.JumpPower * _config.SlideFactor)
                    {
                        _surfer.MoveData.Sliding = true;
                    }
                }
                quickJump = _surfer.GroundObject == null && !goingAgainstSlope && movingUp && trace.Distance < _surfer.MoveData.Velocity.y;
            }

            if (MovingUpRapidly()
                || trace.HitCollider == null
                || _surfer.MoveData.Surfing
                || _surfer.MoveData.Sliding
                || _surfer.MoveData.GravityFactor < 0
                || quickJump
                || (_surfer.MoveType == MoveType.Ladder && movingUp))
            {
                if (_surfer.MoveData.JustJumped && !trace.IsShit)
                {
                    _surfer.MoveData.Origin.y = trace.HitPoint.y + HammerScale;
                }
                _surfer.MoveData.GroundTest = 0;
                SetGround(null);
                return false;
            }
            else
            {
                if (wasSurfing && _surfer.MoveData.GroundTest == 0)
                {
                    _surfer.MoveData.GroundTest++;
                    SetGround(null);
                    return false;
                }
                _surfer.MoveData.GroundTest = 0;
                SetGround(trace.HitCollider.gameObject, trace.PlaneNormal);
                _surfer.MoveData.Origin.y = trace.HitPoint.y + HammerScale;

                // slant boost, but only if velocity is away from slope 
                if (_surfer.MoveData.JustGrounded)
                {
                    if (!goingAgainstSlope)
                    {
                        //SurfPhysics.ClipVelocity(_surfer.MoveData.Velocity, trace.PlaneNormal, ref _surfer.MoveData.Velocity, 1.0f);
                        SurfPhysics.Reflect(_surfer, _deltaTime, trace.PlaneNormal);
                    }
                    _surfer.MoveData.Velocity.y = 0;
                }

                return true;
            }
        }

        private void SetGround(GameObject obj, Vector3 normal = default)
        {
            _surfer.MoveData.GroundNormal = normal;

            if (obj != null)
            {
                if (_surfer.GroundObject == null)
                {
                    _surfer.MoveData.JustGrounded = true;
                    _surfer.MoveData.PreGroundedVelocity = _surfer.MoveData.Velocity;
                }

                _surfer.GroundObject = obj;
            }
            else
            {
                _surfer.GroundObject = null;
            }
        }

        private void CheckJump()
        {
            if (!_surfer.MoveData.Buttons.HasFlag(InputActions.Jump)
                || _surfer.GroundObject == null)
            {
                return;
            }

            if (!_config.AutoBhop && _surfer.MoveData.OldButtons.HasFlag(InputActions.Jump))
            {
                return;
            }

            _surfer.MoveData.Velocity.y += _config.JumpPower;
            _surfer.MoveData.JustJumped = true;
            SetGround(null);
        }

        private void CheckDuck()
        {
            if (!_surfer.MoveData.Buttons.HasFlag(InputActions.Duck))
            {
                if (_surfer.MoveData.Ducked)
                {
                    UnDuck();
                }
                return;
            }

            if (!_surfer.MoveData.Ducked)
            {
                Duck();
            }
        }

        private void CheckSteps()
        {
            if (_surfer.GroundObject != null)
            {
                // @Note: multiplying extents by 1.01 helps with walking up stairs to prevent being stopped
                // but it sometimes fails us when walking down stairs.  Could fix this by doing another BoxCast
                // just for going down, with extents multiplied by 1 or 0.99, but it's probably not worth it.
                // I think that's what's happening anyways
                var extents = _surfer.Collider.bounds.extents;
                extents.y = 0.15f;
                var nextPos = _surfer.MoveData.Origin + _surfer.MoveData.AbsVelocity * _deltaTime;
                var center = nextPos + new Vector3(0, _surfer.Collider.bounds.size.y - extents.y, 0);
                //var distance = _surfer.Collider.bounds.size.y - extents.y / 2f;
                var distance = 10f;
                if (Physics.BoxCast(center: center,
                    halfExtents: extents,
                    direction: Vector3.down,
                    orientation: _surfer.Orientation,
                    maxDistance: distance,
                    layerMask: SurfPhysics.GroundLayerMask,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore,
                    hitInfo: out RaycastHit hit))
                {
                    if (!hit.collider.enabled
                        || hit.point == Vector3.zero
                        || hit.normal.y <= SurfPhysics.SurfSlope)
                    {
                        return;
                    }

                    var stepHeight = Mathf.Abs(hit.point.y - _surfer.MoveData.Origin.y);
                    if (_surfer.MoveData.Origin.y > hit.point.y)
                    {
                        stepHeight -= HammerScale * 2f;
                    }
                    if (stepHeight <= _config.StepSize)
                    {
                        _surfer.MoveData.Origin.y = hit.point.y + HammerScale;
                    }
                }
            }
        }

        private void CheckSlope()
        {
            if (_surfer.MoveData.Surfing)
            {
                return;
            }

            var origin = _surfer.MoveData.Origin + new Vector3(0, _surfer.Collider.bounds.extents.y + 0.1f, 0);
            var direction = Vector3.down;
            var distance = 0.2f;

            var hitCount = Physics.BoxCastNonAlloc(results: _hitCache,
                center: origin,
                direction: direction,
                orientation: Quaternion.identity,
                maxDistance: distance,
                halfExtents: _surfer.Collider.bounds.extents,
                layerMask: SurfPhysics.GroundLayerMask,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hitCount; i++)
            {
                if (!_hitCache[i].collider.enabled
                    || _hitCache[i].normal.y <= SurfPhysics.SurfSlope
                    || _hitCache[i].normal.y >= 1)
                {
                    continue;
                }

                var slopeDir = Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, _hitCache[i].normal));
                var dot = Vector3.Dot(_surfer.MoveData.AbsVelocity.normalized, slopeDir);
                var goingAgainstSlope = dot > 0;

                if (!goingAgainstSlope)
                {
                    continue;
                }

                Debug.DrawLine(origin, _hitCache[i].point, Color.magenta, 5f);

                _surfer.MoveData.MomentumModifier = _hitCache[i].normal;
                SurfPhysics.ClipVelocity(_surfer.MoveData.Velocity, _hitCache[i].normal, ref _surfer.MoveData.Velocity, 1.0f);
                SurfPhysics.ClipVelocity(_surfer.MoveData.BaseVelocity, _hitCache[i].normal, ref _surfer.MoveData.BaseVelocity, 1.0f);

                var end = origin + _surfer.MoveData.AbsVelocity.normalized * 2f;

                Debug.DrawLine(origin, end, Color.red, 5f);
            }

            //// todo: remove reflect function, seems useless when all it does is clip velocity?
            //if (_surfer.GroundObject != null)
            ////if (!_surfer.MoveData.JustGrounded && !_surfer.MoveData.Surfing && _surfer.GroundObject != null)
            //{
            //    //SurfPhysics.Reflect(_surfer, _deltaTime);
            //}
        }

        private Collider[] _waterTestCache = new Collider[32];

        private void CheckWater()
        {
            var extents = _surfer.Collider.bounds.extents;
            extents.x *= 0.9f;
            extents.z *= 0.9f;
            var center = _surfer.MoveData.Origin;
            center.y += extents.y;

            var hitCount = Physics.OverlapBoxNonAlloc(center, extents, _waterTestCache, _surfer.Orientation, 1 << LayerMask.NameToLayer("Water")); // todo:maybe cache layermask
            _surfer.MoveData.WaterDepth = hitCount > 0 ? 0.1f : 0f;

            for (int i = 0; i < hitCount; i++)
            {
                if (!_waterTestCache[i].enabled)
                {
                    continue;
                }

                var water = _waterTestCache[i];
                var headPoint = _surfer.MoveData.Origin + new Vector3(0, _surfer.Collider.bounds.size.y, 0);

                if (water.bounds.Contains(headPoint))
                {
                    _surfer.MoveData.WaterDepth = 1f;
                }
                else
                {
                    var closetsPoint = water.ClosestPoint(headPoint);
                    var dist = Vector3.Distance(closetsPoint, headPoint);
                    _surfer.MoveData.WaterDepth = Mathf.Max(0.1f, (_surfer.Collider.bounds.size.y - dist) / _surfer.Collider.bounds.size.y);
                }
            }

            if (_surfer.MoveData.WaterDepth >= _config.WaterDepthToSwim)
            {
                _surfer.MoveType = MoveType.Swim;
            }
            else if (_surfer.MoveType == MoveType.Swim)
            {
                _surfer.MoveType = MoveType.Walk;
            }
        }

        private void Duck()
        {
            var duckDistance = _surfer.StandingExtents.y * 2 * _config.DuckDistance;
            var duckHalf = duckDistance * 0.5f;

            _surfer.Collider.size = _surfer.Collider.size - new Vector3(0, duckDistance, 0);
            _surfer.Collider.center = _surfer.Collider.center - new Vector3(0, duckHalf, 0);

            if (_surfer.GroundObject == null)
            {
                IncrementOrigin(new Vector3(0, duckHalf, 0));
            }

            _surfer.MoveData.Ducked = true;
            _surfer.MoveData.WalkFactor -= _config.DuckWalkModifier;
        }

        private void UnDuck()
        {
            if (_surfer.MoveData.Surfing)
            {
                return;
            }

            var duckDistance = _surfer.StandingExtents.y * 2 * _config.DuckDistance;
            var duckHalf = duckDistance * 0.5f;

            if (_surfer.GroundObject == null)
            {
                var toGround = BoxCastToFloor(duckHalf + HammerScale, 1f);
                if (toGround.HitCollider != null)
                {
                    // don't unduck if on slope
                    if (toGround.PlaneNormal.y <= SurfPhysics.SurfSlope)
                    {
                        return;
                    }
                    _surfer.MoveData.Origin.y = toGround.HitPoint.y + HammerScale;
                }
                else
                {
                    _surfer.MoveData.Origin -= new Vector3(0, duckHalf, 0);
                }
                CheckGrounded();
            }

            _surfer.Collider.size = _surfer.Collider.size + new Vector3(0, duckDistance, 0);
            _surfer.Collider.center = _surfer.Collider.center + new Vector3(0, duckHalf, 0);

            _surfer.MoveData.Ducked = false;
            _surfer.MoveData.WalkFactor += _config.DuckWalkModifier;
        }

        private void IncrementOrigin(Vector3 amount)
        {
            _surfer.MoveData.PreviousOrigin = _surfer.MoveData.Origin;

            if (_surfer.MoveType == MoveType.Noclip && !_config.NoclipCollide)
            {
                _surfer.MoveData.Origin += amount;
                return;
            }

            var maxMove = .15f;
            var steps = Mathf.CeilToInt(amount.magnitude / maxMove);
            if (steps <= 0) steps = 1;
            steps = Mathf.Max(steps, 1);

            var increment = amount / steps;
            for (int i = 0; i < steps; i++)
            {
                _surfer.MoveData.Origin += increment;
                SurfPhysics.ResolveCollisions(_surfer);
            }

            //var prevOrigin = _surfer.MoveData.PreviousOrigin;
            //var newOrigin = _surfer.MoveData.Origin;
            //var movementThisStep = newOrigin - prevOrigin;
            //var newMovement = movementThisStep;
            //if (movementThisStep.magnitude >= _surfer.Collider.bounds.extents.x)
            //{
            //    var center = prevOrigin;
            //    center.y += _surfer.Collider.bounds.extents.y;

            //    var hitCount = Physics.BoxCastNonAlloc(center: center,
            //        halfExtents: _surfer.Collider.bounds.extents * 0.5f,
            //        direction: movementThisStep.normalized,
            //        orientation: Quaternion.identity,
            //        results: _hitCache,
            //        maxDistance: movementThisStep.magnitude,
            //        layerMask: SurfPhysics.GroundLayerMask,
            //        queryTriggerInteraction: QueryTriggerInteraction.Ignore);

            //    if (hitCount > 0)
            //    {
            //        for (int i = 0; i < hitCount; i++)
            //        {
            //            if (!_hitCache[i].collider.enabled)
            //            {
            //                continue;
            //            }
            //            newMovement += _hitCache[i].normal * (movementThisStep.magnitude - _hitCache[i].distance);
            //            SurfPhysics.ClipVelocity(_surfer.MoveData.Velocity, _hitCache[i].normal, ref _surfer.MoveData.Velocity, 1.01f);
            //        }
            //        _surfer.MoveData.Origin = prevOrigin + newMovement;
            //    }
            //}
        }

        private Trace BoxCastToFloor(float distance = 0.05f, float extentModifier = 1.0f)
        {
            //var extents = _surfer.Collider.bounds.extents * extentModifier;
            //var center = _surfer.MoveData.Origin;
            //center.y += extents.y + 0.02f;
            //distance += 0.02f;

            //if (Physics.BoxCast(center, extents, Vector3.down, out RaycastHit hit, Quaternion.identity, distance, SurfPhysics.GroundLayerMask, QueryTriggerInteraction.Ignore))
            //{
            //    return hit;
            //}
            //return default;

            var extents = _surfer.Collider.bounds.extents * extentModifier;
            var center = _surfer.MoveData.Origin;
            center.y += extents.y + 0.02f;
            distance += 0.02f;

            if (_surfer.MoveData.Velocity.y < 0)
            {
                var dv = _surfer.MoveData.Velocity.y * -1.01f * _deltaTime;
                distance = Mathf.Max(distance, dv);
            }

            var count = Physics.BoxCastNonAlloc(center,
                extents,
                Vector3.down,
                _hitCache,
                _surfer.Orientation,
                distance,
                SurfPhysics.GroundLayerMask,
                QueryTriggerInteraction.Ignore);

            var greatY = float.MinValue;
            RaycastHit bestHit = default;

            for (int i = 0; i < count; i++)
            {
                // trash physx interpenetration bullshit.....
                if (Tracer.HitIsShit(_hitCache[i]))
                {
                    continue;
                }

                if (bestHit.collider == null)
                {
                    bestHit = _hitCache[i];
                    greatY = _hitCache[i].point.y;
                }

                if (_hitCache[i].normal.y <= SurfPhysics.SurfSlope && _hitCache[i].normal.y > 0)
                {
                    return _hitCache[i];
                }

                if (_hitCache[i].point.y > greatY)
                {
                    bestHit = _hitCache[i];
                    greatY = _hitCache[i].point.y;
                }
            }

            return bestHit;
        }

        private MoveStyle GetMoveStyle(Vector3 angles, Vector3 velocity)
        {
            float tempAngle = angles[1];
            if (tempAngle < 0)
                tempAngle += 360;

            VectorAngles(velocity, ref angles);

            float tempAngle2 = tempAngle - angles[1];

            if (tempAngle2 < 0)
                tempAngle2 = -tempAngle2;

            if (tempAngle2 < 22.5 || tempAngle2 > 337.5)
                return MoveStyle.FW;
            if (tempAngle2 > 22.5 && tempAngle2 < 67.5 || tempAngle2 > 292.5 && tempAngle2 < 337.5)
                return MoveStyle.HSW;
            if (tempAngle2 > 67.5 && tempAngle2 < 112.5 || tempAngle2 > 247.5 && tempAngle2 < 292.5)
                return MoveStyle.SW;
            if (tempAngle2 > 112.5 && tempAngle2 < 157.5 || tempAngle2 > 202.5 && tempAngle2 < 247.5)
                return MoveStyle.BWHSW;
            if (tempAngle2 > 157.5 && tempAngle2 < 202.5)
                return MoveStyle.BW;

            return MoveStyle.FW; // Unknown
        }

        private static void VectorAngles(Vector3 velocity, ref Vector3 angles)
        {
            float tmp, yaw, pitch;
            if (Mathf.Approximately(velocity[2], 0) && Mathf.Approximately(velocity[0], 0))
            {
                yaw = 0f;
                if (velocity[1] > 0)
                    pitch = 270f;
                else
                    pitch = 90f;
            }
            else
            {
                yaw = (Mathf.Atan2(velocity[0], velocity[2]) * (180f / Mathf.PI));
                if (yaw < 0)
                    yaw += 360;
                tmp = Mathf.Sqrt(velocity[0] * velocity[0] + velocity[2] * velocity[2]);
                pitch = (Mathf.Atan2(tmp, -velocity[1]) * (180f / Mathf.PI));
                if (pitch < 0)
                    pitch += 360;
            }

            angles[0] = pitch;
            angles[1] = yaw;
            angles[2] = 0f;
        }

    }
}
