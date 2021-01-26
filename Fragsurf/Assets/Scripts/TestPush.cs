using Fragsurf.FSM.Actors;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using UnityEngine;

public class TestPush : FSMTrigger
{

    [Header("Test Push Options")]

    public Vector3 SetVelocity;

    protected override void _TriggerEnter(NetEntity entity)
    {
        Debug.Log(entity.Game.IsHost);
        if (entity is Human hu
            && hu.MovementController is DefaultMovementController mc)
        {
            hu.Velocity = SetVelocity;
        }
    }

}
