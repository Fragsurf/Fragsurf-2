using System;
using Fragsurf.BSP;
using UnityEngine;
using SourceUtils.ValveBsp.Entities;
using Fragsurf.Shared.Entity;

namespace Fragsurf.BSP
{
    [EntityComponent("func_door_rotating")]
    public class BspFuncDoorRotating : GenericBspEntityMonoBehaviour<FuncDoorRotating>
    {

        private Quaternion _openRotation;
        private Quaternion _closedRotation;
        private Vector3 _rotateAxis = Vector3.up;
        [NetProperty]
        private bool _open;
        private float _resetTimer;

        protected override void OnStart()
        {
            _open = !Entity.DefaultIsClosed;
            _openRotation = Quaternion.AngleAxis(Entity.Distance, _rotateAxis);
            _closedRotation = transform.rotation;
        }

        public void Open()
        {
            _open = true;
            _resetTimer = Entity.DefaultIsClosed ? Entity.DelayBeforeReset : 0;
        }

        public void Close()
        {
            _open = false;
            _resetTimer = Entity.DefaultIsClosed ? 0 : Entity.DelayBeforeReset;
        }

        public void Toggle()
        {
            if (_open)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        protected override void OnUpdate()
        {
            if(_resetTimer > 0)
            {
                _resetTimer -= Time.deltaTime;
                if(_resetTimer <= 0)
                {
                    _open = !Entity.DefaultIsClosed;
                }
            }

            var speed = Entity.Speed * Time.deltaTime;
            var targetRotation = _open ? _openRotation : _closedRotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed);
        }

        protected override void _Input(BspEntityOutput output)
        {
            if(string.Equals(output.TargetInput, "open", StringComparison.OrdinalIgnoreCase))
            {
                Open();
            }
            else if(string.Equals(output.TargetInput, "close", StringComparison.OrdinalIgnoreCase))
            {
                Close();
            }
        }

    }
}

