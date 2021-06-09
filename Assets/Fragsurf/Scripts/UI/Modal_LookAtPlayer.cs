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
        private GameObject _specContainer;
        [SerializeField]
        private TMP_Text _specName;
        [SerializeField]
        private TMP_Text _specList;
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

        private int[] _specCache = new int[64];
        private void Update()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if(cl == null)
            {
                return;
            }
            var spec = cl.Get<SpectateController>();

            if (SpectateController.SpecTarget == null
                || Human.Local == SpectateController.SpecTarget)
            {
                _specContainer.SetActive(false);
            }
            else if(SpectateController.SpecTarget != Human.Local)
            {
                var pl = cl.PlayerManager.FindPlayer(SpectateController.SpecTarget);
                if(pl != null)
                {
                    _specContainer.SetActive(true);
                    var specStr = string.Empty;
                    _specName.text = $"Watching: {pl.DisplayName}";
                    var specCount = spec.GetPlayersSpectating(SpectateController.SpecTarget.EntityId, _specCache);
                    for(int i = 0; i < specCount; i++)
                    {
                        var pl2 = cl.PlayerManager.FindPlayer(_specCache[i]);
                        if(pl2 == null)
                        {
                            continue;
                        }
                        specStr += pl2.DisplayName + "\n";
                    }
                    _specList.text = specStr;
                }
                else
                {
                    _specContainer.SetActive(false);
                    _specList.text = string.Empty;
                    _specName.text = string.Empty;
                }
            }

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

