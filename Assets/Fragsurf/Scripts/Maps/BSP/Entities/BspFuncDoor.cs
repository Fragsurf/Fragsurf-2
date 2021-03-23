using Fragsurf.BSP;
using Fragsurf.Shared.Entity;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("func_door", "func_movelinear")]
    public class BspFuncDoor : GenericBspEntityMonoBehaviour<FuncDoor>
    {

        private Vector3 _openPosition;
        private Vector3 _closedPosition;
        [NetProperty]
        private bool _open { get; set; }
        private float _resetTimer;

        protected override void OnStart()
        {
            _open = !Entity.DefaultIsClosed;

            var moveDist = 5f;
            if(Entity is FuncMoveLinear ml)
            {
                moveDist = ml.MoveDistance * BspToUnity.Options.WorldScale;
            }

            if (_open)
            {
                _openPosition = transform.position;
                _closedPosition = transform.position + Entity.MoveDir.TOUDirection() * moveDist;
            }
            else
            {
                _openPosition = transform.position + Entity.MoveDir.TOUDirection() * moveDist;
                _closedPosition = transform.position;
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
            var targetPos = _open ? _openPosition : _closedPosition;
            var moveSpeed = Entity.Speed * BspToUnity.Options.WorldScale * Time.deltaTime;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPos, moveSpeed);
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

        protected override void _Input(BspEntityOutput output)
        {
            switch (output.TargetInput.ToLower()) 
            {
                case "toggle":
                    Toggle();
                    break;
                case "open":
                    Open();
                    break;
                case "close":
                    Close();
                    break;
            }
        }

    }
}

