using UnityEngine;
using Fragsurf.Movement;
using System.Collections.Generic;
using Fragsurf.Shared.Entity;

namespace Fragsurf.FSM.Actors
{
    public class FSMGravity : FSMTrigger
    {
        private class GravityTimer
        {
            public ISurfControllable Surfer;
            public float Timer;
            public int Tick;
        }

        [Header("Gravity Options")]

        [Tooltip("Multiplier for the player's gravity")]
        public float Gravity = 1f;
        [Tooltip("Time, in seconds, before setting the player's gravity back to 1")]
        public float Duration = 0.5f;

        private List<GravityTimer> _gravityTimers = new List<GravityTimer>();

        protected override void _TriggerEnter(NetEntity entity)
        {
            if(!(entity is Human hu)
                || !(hu.MovementController is ISurfControllable character)
                || character.MoveType == MoveType.Noclip)
            {
                return;
            }

            character.MoveData.GravityFactor = Gravity;
            GetOrCreateTimer(character).Timer = Duration;

            GravityTimer GetOrCreateTimer(ISurfControllable surfer)
            {
                foreach (var timer in _gravityTimers)
                {
                    if (timer.Surfer == surfer)
                    {
                        return timer;
                    }
                }
                var result = new GravityTimer() { Surfer = surfer };
                _gravityTimers.Add(result);
                return result;
            }
        }

        public override void Tick()
        {
            for(int i = _gravityTimers.Count - 1; i >= 0; i--)
            {
                _gravityTimers[i].Timer -= Time.fixedDeltaTime;
                if(_gravityTimers[i].Timer <= 0)
                {
                    _gravityTimers[i].Surfer.MoveData.GravityFactor = 1f;
                    _gravityTimers.RemoveAt(i);
                }
                else
                {
                    _gravityTimers[i].Tick++;
                }
            }
        }

        protected override void _OnDrawGizmos()
        {
            base._OnDrawGizmos();

            var collider = GetComponent<Collider>();
            if(collider != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(collider.bounds.center, 0.5f);
                Gizmos.DrawLine(collider.bounds.center, collider.bounds.center + Vector3.up * 10);
            }
        }

    }
}
