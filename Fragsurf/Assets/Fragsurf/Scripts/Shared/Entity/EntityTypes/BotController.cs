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

        public BotController(Human hu)
        {
            Human = hu;
        }

        public void Tick()
        {
            Human.RunCommand(Command, false);
        }

        protected virtual void _Tick() { }

    }
}

