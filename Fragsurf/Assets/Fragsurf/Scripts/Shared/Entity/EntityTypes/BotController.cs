using Fragsurf.Movement;
using Fragsurf.Shared.Packets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class BotController 
    {

        public Human Human { get; private set; }
        public UserCmd Command { get; } = new UserCmd();

        private bool _flipped;
        private static bool _flip;

        public BotController(Human hu)
        {
            Human = hu;
            _flipped = _flip;
            _flip = !_flip;
        }

        public void Tick()
        {
            Command.Reset();

            var sin = Mathf.Sin(Time.time);

            if (_flipped)
            {
                sin *= -1;
            }

            if(sin <= 0)
            {
                Command.Buttons |= InputActions.MoveLeft;
            }
            else
            {
                Command.Buttons |= InputActions.MoveRight;
            }

            if(Random.value >= .98f)
            {
                Command.Buttons |= InputActions.Jump;
                Command.Buttons |= InputActions.HandAction;
            }

            //Command.Angles = Human.Angles + new Vector3(0, 1, 0);

            Human.RunCommand(Command, false);
        }

        protected virtual void _Tick() { }

    }
}

