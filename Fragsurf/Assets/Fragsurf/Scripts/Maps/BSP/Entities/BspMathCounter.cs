using Fragsurf.BSP;
using SourceUtils.ValveBsp.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("math_counter")]
    public class BspMathCounter : GenericBspEntityMonoBehaviour<MathCounter>
    {

        private int _counter;

        protected override void OnStart()
        {
            _counter = Entity.Start;
        }

        public void Add(int amount)
        {
            var canHitMax = _counter < Entity.Max;
            _counter += amount;
            if(_counter >= Entity.Max && canHitMax)
            {
                Fire("OnHitMax");
            }
        }

        public void Subtract(int amount)
        {
            var canHitMin = _counter > Entity.Min;
            _counter -= amount;
            if (_counter <= Entity.Min && canHitMin)
            {
                Fire("OnHitMin");
            }
        }

        protected override void _Input(BspEntityOutput output)
        {
            switch (output.TargetInput.ToLower())
            {
                case "add":
                    if(int.TryParse(output.Parameter, out int amountToAdd))
                    {
                        Add(amountToAdd);
                    }
                    break;
                case "subtract":
                    if (int.TryParse(output.Parameter, out int amountToSubtract))
                    {
                        Subtract(amountToSubtract);
                    }
                    break;
            }
        }

    }
}

