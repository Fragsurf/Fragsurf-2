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
                Command.Buttons |= InputActions.HandAction;

                if(Random.value <= .5f)
                {
                    Command.Buttons |= InputActions.Jump;
                }
            }

            //Command.Angles = Human.Angles + new Vector3(0, 1, 0);

            if (Human.Game.IsHost)
            {
                var player = Human.Game.PlayerManager.FindPlayer(Human.OwnerId);
                if(player == null)
                {
                    Human.RunCommand(Command, false);
                }
                else
                {
                    Human.Game.Get<UserCmdHandler>().HandleUserCommand(player, Command);
                }
            }
        }

        protected virtual void _Tick() { }

    }
}

