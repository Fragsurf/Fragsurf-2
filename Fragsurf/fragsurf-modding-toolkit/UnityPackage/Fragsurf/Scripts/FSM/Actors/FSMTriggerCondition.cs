using UnityEngine;

namespace Fragsurf.FSM.Actors
{
    public enum FSMTriggerCondition
    {
        None = 0,
        Grounded = 1,
        Surfing = 2,
        Sliding = 3,
        InAir = 4,
        GroundedTwice = 5
    }
}
