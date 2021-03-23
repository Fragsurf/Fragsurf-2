using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Shared.Player;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_LookAtPlayer : UGuiModal
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private CanvasGroup _canvasGroup;

        private float _targetAlpha;
        private float _hoverTimer;
        private const float _timeToDisplay = .1f;

        private void Start()
        {
            _name.text = string.Empty;
            _targetAlpha = 0;
        }

        private void Update()
        {
            if (_canvasGroup)
            {
                _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, _targetAlpha, 16 * Time.deltaTime);
            }

            if (!GameCamera.Camera)
            {
                _hoverTimer = 0;
                _targetAlpha = 0;
                _name.text = string.Empty;
                return;
            }

            var isHovering = false;
            var ray = GameCamera.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 20, 1 << Layers.Client, QueryTriggerInteraction.Collide)
                && hit.collider.TryGetComponent(out EntityGameObject entObj)
                && entObj.Entity is Human hu)
            {
                _hoverTimer += Time.deltaTime;
                var cl = FSGameLoop.GetGameInstance(false);
                var player = cl.PlayerManager.FindPlayer(hu);
                if (player != null)
                {
                    _name.text = player.DisplayName;
                    _name.color = PlayerManager.GetTeamColor(player.Team);
                }
                else
                {
                    _name.text = entObj.Entity.GetType().Name;
                    _name.color = Color.white;
                }
                isHovering = true;
            }
            else
            {
                _hoverTimer = 0f;
            }

            _targetAlpha = isHovering && _hoverTimer >= _timeToDisplay ? 1 : 0;
        }

    }
}

