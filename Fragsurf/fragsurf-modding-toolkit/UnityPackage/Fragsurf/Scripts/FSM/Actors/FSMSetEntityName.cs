using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.FSM.Actors
{
    public class FSMSetEntityName : FSMTrigger
    {

        [Header("Set Entity Name Options")]
        [Tooltip("Sets the Entity's Name")]
        public string EntityName;

        protected override void _TriggerEnter(NetEntity entity)
        {
            entity.EntityName = EntityName;   
        }

    }
}

