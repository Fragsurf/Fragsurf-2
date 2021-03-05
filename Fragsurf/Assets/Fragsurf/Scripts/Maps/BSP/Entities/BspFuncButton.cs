using Fragsurf.BSP;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("func_button")]
    public class BspFuncButton : GenericBspEntityMonoBehaviour<FuncButton>, IInteractable
    {

        private float _resetTimer;
        private Vector3 _downPosition;
        private Vector3 _originalPosition;

        protected override void OnStart()
        {
            gameObject.SetChildrenCollidersToConvexTrigger();
            _originalPosition = transform.position;
            _downPosition = _originalPosition + Entity.MoveDir.TOUDirection() * .15f;
        }

        protected override void OnUpdate()
        {
            if(_resetTimer > 0)
            {
                _resetTimer -= Time.deltaTime;
            }

            var targetPos = _resetTimer > 0 ? _downPosition : _originalPosition;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Entity.Speed * BspToUnity.Options.WorldScale * Time.deltaTime);
        }

        public void OnInteract(NetEntity interactee)
        {
            Press();
        }

        public void Press()
        {
            if (_resetTimer > 0 || _resetTimer == -1)
            {
                return;
            }

            _resetTimer = Entity.DelayBeforeReset;

            // OnPressed:prisondoor,Open,,0,-1
            Fire("OnPressed");
        }

        protected override void _Input(BspEntityOutput output)
        {
            switch (output.TargetInput.ToLower())
            {
                case "press":
                    Press();
                    break;
            }
        }

        public void MouseEnter(int clientIndex)
        {
        }

        public void MouseExit(int clientIndex)
        {
        }
    }
}

