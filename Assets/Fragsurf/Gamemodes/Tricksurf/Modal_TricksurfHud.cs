using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using Fragsurf.Utility;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.Tricksurf
{
    public class Modal_TricksurfHud : UGuiModal
    {

        [SerializeField]
        private TMP_Text _speedText;
        [SerializeField]
        private TMP_Text _boardExitStartText;
        [SerializeField]
        private TMP_Text _touchListText;

        private int _touchInfoCount;

        private SH_Tricksurf _tricksurf => FSGameLoop.GetGameInstance(false).Get<SH_Tricksurf>();

        private int _boardSpeed;
        private int _exitSpeed;
        private bool _wasSurfing;

        private void Start()
        {
            _tricksurf.OnTriggerEntered += OnTriggerEntered;
            _touchListText.text = string.Empty;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_tricksurf != null)
            {
                _tricksurf.OnTriggerEntered -= OnTriggerEntered;
            }
        }

        private void Update()
        {
            if (SpectateController.SpecTarget == null)
            {
                _boardSpeed = 0;
                _exitSpeed = 0;
                _wasSurfing = false;
                return;
            }
            var mc = SpectateController.SpecTarget.MovementController as CSMovementController;
            var surfing = mc.MoveData.Surfing;
            if (surfing && !_wasSurfing)
            {
                _boardSpeed = SpectateController.SpecTarget.HammerVelocity();
            }
            else if (!surfing && _wasSurfing)
            {
                _exitSpeed = SpectateController.SpecTarget.HammerVelocity();
            }
            _wasSurfing = surfing;

            _speedText.text = $"{GetTargetVelocity()}";
            _boardExitStartText.text = $"{_boardSpeed} board\n{_exitSpeed} exit\n{GetTargetStartSpeed()} start";
        }

        private void OnTriggerEntered(BasePlayer player, TouchInfo touchInfo)
        {
            if (player.Entity != SpectateController.SpecTarget)
            {
                return;
            }
            var newTxt = _touchListText.text;
            _touchInfoCount++;
            if (_touchInfoCount > 6)
            {
                var linesToRemove = _touchInfoCount - 6;
                newTxt = newTxt.RemoveFirstLines(linesToRemove);
                _touchInfoCount = 6;
            }
            _touchListText.text = $"{newTxt}\n<size=18>{touchInfo.Speed} u/s</size> {touchInfo.Trigger.ActorName}";
        }

        private int GetTargetVelocity()
        {
            if (SpectateController.SpecTarget == null)
            {
                return 0;
            }
            return SpectateController.SpecTarget.HammerVelocity(!DevConsole.GetVariable<bool>("game.verticalvelocity"));
        }

        private int GetTargetStartSpeed()
        {
            var pl = FSGameLoop.GetGameInstance(false).PlayerManager.FindPlayer(SpectateController.SpecTarget);
            if (SpectateController.SpecTarget == null || pl == null)
            {
                return 0;
            }
            return _tricksurf.GetStartSpeed(pl);
        }

        private string GetTargetName()
        {
            var pl = FSGameLoop.GetGameInstance(false).PlayerManager.FindPlayer(SpectateController.SpecTarget);
            if (SpectateController.SpecTarget == null 
                || pl == null 
                || pl.ClientIndex == FSGameLoop.GetGameInstance(false).ClientIndex)
            {
                return string.Empty;
            }

            return pl.DisplayName;
        }

    }
}

