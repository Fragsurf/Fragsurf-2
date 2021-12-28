using Fragsurf.Actors;
using Fragsurf.Movement;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class CSMovementController : MovementController, ISurfControllable
    {

        //private MovementConfig _moveConfig = new MovementConfig();
        private SurfController _surfController = new SurfController();
        private List<FSMTrigger> _touchingTriggers = new List<FSMTrigger>();
        private List<FSMTrigger> _lastTickTriggers = new List<FSMTrigger>();

        public MoveType MoveType { get; set; } = MoveType.Walk;
        public MoveData MoveData { get; } = new MoveData();
        public BoxCollider Collider => Human.HumanGameObject.BoundsCollider;
        public Quaternion Orientation => Quaternion.identity;
        public Vector3 Forward { get; set; }
        public Vector3 Right { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 StandingExtents => new Vector3(.858f, 1.83f, .858f) * .5f;
        public override GameObject GroundObject { get; set; }
        public override bool JustGrounded => MoveData.JustGrounded;
        public override bool JustJumped => MoveData.JustJumped;
        public override bool MouseControlsRotation => true;

        public CSMovementController(Human human) 
            : base(human)
        {
        }

        public override void ExecuteMovement(UserCmd.CmdFields cmd)
        {
            //MoveData.LimitedExecution = !Human.Game.IsServer && Human != Human.Local;
            MoveData.OldButtons = MoveData.Buttons;

            Right = Quaternion.Euler(cmd.Angles) * Vector3.right;
            Forward = Quaternion.Euler(new Vector3(0, cmd.Angles.y, 0)) * Vector3.forward;
            Up = Quaternion.Euler(cmd.Angles) * Vector3.up;

            MoveData.Buttons = cmd.Buttons;
            MoveData.ViewAngles = cmd.Angles;
            MoveData.Origin = Human.Origin;
            MoveData.Velocity = Human.Velocity;
            MoveData.BaseVelocity = Human.BaseVelocity;
            MoveData.ForwardMove = 0;
            MoveData.SideMove = 0;
            MoveData.UpMove = 0;

            if (cmd.Buttons.HasFlag(InputActions.MoveBack))
            {
                MoveData.ForwardMove = -Human.Game.GameMovement.Config.ForwardSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveForward))
            {
                MoveData.ForwardMove = Human.Game.GameMovement.Config.ForwardSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveRight))
            {
                MoveData.SideMove = Human.Game.GameMovement.Config.SideSpeed;
            }

            if (cmd.Buttons.HasFlag(InputActions.MoveLeft))
            {
                MoveData.SideMove = -Human.Game.GameMovement.Config.SideSpeed;
            }

            _surfController.CalculateMovement(this, Human.Game.GameMovement.Config, Time.fixedDeltaTime);

            if (Human.Game.IsHost || Human.Local == Human)
            {
                var prevVelocity = Human.Velocity;

                Human.Velocity = MoveData.Velocity;
                Human.BaseVelocity = MoveData.BaseVelocity;
                Human.Origin = MoveData.Origin;
                Human.Angles = MoveData.ViewAngles;
                Human.Ducked = MoveData.Ducked;

                var prevOrigin = Human.Origin;
                DetectTouchingTriggers();
                TouchTriggers();

                if(Human.Origin != prevOrigin)
                {
                    Human.Velocity = prevVelocity;
                }
            }

            TickFootstep();
        }

        private static RaycastHit[] _touchBuffer = new RaycastHit[32];
        private static Collider[] _touchBuffer2 = new Collider[32];
        private void DetectTouchingTriggers()
        {
            var prevOrigin = MoveData.PreviousOrigin;
            var newOrigin = MoveData.Origin;
            var dist = Vector3.Distance(prevOrigin, newOrigin);
            var extents = Collider.bounds.extents;
            extents.x *= .99f;
            extents.z *= .99f;

            if (dist <= .1f)
            {
                var center = newOrigin;
                center.y += Collider.bounds.extents.y;
                var hitCount = Physics.OverlapBoxNonAlloc(center: center,
                   halfExtents: extents,
                   orientation: Quaternion.identity,
                   mask: 1 << 0,
                   results: _touchBuffer2,
                   queryTriggerInteraction: QueryTriggerInteraction.Collide);

                for (int i = 0; i < hitCount; i++)
                {
                    if (!_touchBuffer2[i].isTrigger || !_touchBuffer2[i].enabled)
                    {
                        continue;
                    }
                    var itrig = _touchBuffer2[i].GetComponentInParent<FSMTrigger>();
                    if (itrig != null)
                    {
                        _touchingTriggers.Add(itrig);
                    }
                }
            }
            else
            {
                var dir = (newOrigin - prevOrigin).normalized;
                var center = prevOrigin;
                center.y += Collider.bounds.extents.y;

                var hitCount = Physics.BoxCastNonAlloc(center: center,
                    halfExtents: extents,
                    direction: dir,
                    orientation: Quaternion.identity,
                    maxDistance: dist,
                    layerMask: 1 << 0,
                    results: _touchBuffer,
                    queryTriggerInteraction: QueryTriggerInteraction.Collide);

                for (int i = 0; i < hitCount; i++)
                {
                    if (!_touchBuffer[i].collider.enabled
                        || !_touchBuffer[i].collider.isTrigger)
                    {
                        continue;
                    }
                    var itrig = _touchBuffer[i].collider.GetComponentInParent<FSMTrigger>();
                    if (itrig != null)
                    {
                        _touchingTriggers.Add(itrig);
                    }
                }
            }
        }

        private void TouchTriggers()
        {
            foreach (var trigger in _touchingTriggers)
            {
                if (!_lastTickTriggers.Contains(trigger))
                {
                    trigger.OnStartTouch(Human.EntityId, Human.Game.IsHost);
                }
                else
                {
                    trigger.OnTouch(Human.EntityId, Human.Game.IsHost);
                }
            }

            foreach (var trigger in _lastTickTriggers)
            {
                if (!_touchingTriggers.Contains(trigger))
                {
                    trigger.OnEndTouch(Human.EntityId, Human.Game.IsHost);
                }
            }

            _lastTickTriggers.Clear();
            foreach (var t in _touchingTriggers)
            {
                _lastTickTriggers.Add(t);
            }
            _touchingTriggers.Clear();
        }

        private const float _stepDistance = 3.0f;
        private float _noSpam;
        private float _traveled;
        private float _stepRand;
        protected void TickFootstep()
        {
            if(_noSpam > 0)
            {
                _noSpam -= Time.fixedDeltaTime;
            }
            _swimSoundTimer -= Time.fixedDeltaTime;
            _traveled += (MoveData.Origin - MoveData.PreviousOrigin).magnitude;
            if (_traveled >= _stepDistance + _stepRand)
            {
                var spd = Human.HammerVelocity();
                var maxSpd = Human.Game.GameMovement.MaxSpeed;
                var vol = Mathf.Lerp(0f, 1f, (float)spd / maxSpd);
                PlayFootstepSound(vol);
                _stepRand = Random.Range(0.0f, 0.1f);
                _traveled = 0.0f;
            }
            else if ((MoveData.JustGrounded || MoveData.JustJumped) && _noSpam <= 0)
            {
                PlayFootstepSound(Random.Range(.7f, 1f));
                if(Time.realtimeSinceStartup - _lastFootstepTime <= .1f)
                {
                    _noSpam = .45f;
                }
            }
        }

        private static Collider[] _footstepTest = new Collider[32];
        private float _swimSoundTimer;
        private float _lastFootstepTime;
        private void PlayFootstepSound(float vol)
        {
            if (!Human.HumanGameObject || !Human.HumanGameObject.FeetAudioSource)
            {
                return;
            }

            var feetSrc = Human.HumanGameObject.FeetAudioSource;

            if (!GroundObject)
            {
                if (MoveData.InWater 
                    && MoveData.WaterDepth <= 0.83f
                    && MoveData.WaterDepth >= 0.5f
                    && GameData.Instance.SwimSound 
                    && _swimSoundTimer <= 0)
                {
                    _swimSoundTimer = 2f;
                    feetSrc.PlayClip(GameData.Instance.SwimSound, vol);
                }
                return;
            }

            //var hits = Physics.OverlapBoxNonAlloc(MoveData.Origin + new Vector3(0, .25f, 0), new Vector3(Collider.size.x / 2.1f, 0.28f, Collider.size.z / 2.1f), _footstepTest, Quaternion.identity, 1 << Layers.Fidelity, QueryTriggerInteraction.Ignore);
            //SurfaceTypeIdentifier bestHit = null;

            //if (hits != 0)
            //{
            //    // prioritize water because we don't want to play concrete sound when walking in thin pool
            //    // todo: maybe blend footstep sounds i.e. 50% water 50% concrete
            //    for (int i = 0; i < hits; i++)
            //    {
            //        if (!_footstepTest[i].enabled
            //            || !_footstepTest[i].TryGetComponent(out SurfaceTypeIdentifier surfId))
            //        {
            //            continue;
            //        }
            //        if (bestHit == null || surfId.SurfaceType == SurfaceType.Water)
            //        {
            //            bestHit = surfId;
            //            if (surfId.SurfaceType == SurfaceType.Water)
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}

            //var surfaceType = bestHit ? bestHit.SurfaceType : SurfaceType.Concrete;

            //var cfg = GameData.Instance.Surfaces != null
            //    ? GameData.Instance.Surfaces.GetSurfaceTypeConfig(surfaceType)
            //    : null;

            //if (cfg == null)
            //{
            //    return;
            //}

            //var audioClip = cfg.GetFootstepSound();
            //if (audioClip == null)
            //{
            //    return;
            //}

            var footstepSound = GameData.Instance.FootstepSound;
            if (footstepSound == null) return;

            vol = Mathf.Clamp(vol, 0f, 1f);

            feetSrc.PlayClip(footstepSound, vol, true);

            _lastFootstepTime = Time.realtimeSinceStartup;
        }

    }
}

