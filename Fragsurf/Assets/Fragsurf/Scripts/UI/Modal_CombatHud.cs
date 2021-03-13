using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_CombatHud : UGuiModal
    {

        [SerializeField]
        private TMP_Text _health;
        [SerializeField]
        private TMP_Text _ammo;
        [SerializeField]
        private TMP_Text _itemName;
        [SerializeField]
        private TMP_Text _itemLabel;
        [SerializeField]
        private GameObject _healthContainer;
        [SerializeField]
        private GameObject _itemContainer;

        private int _prevHealth;
        private Color _originalHealthColor;

        private void Start()
        {
            _originalHealthColor = _health.color;
        }

        private void Update()
        {
            var game = FSGameLoop.GetGameInstance(false);
            if (!game 
                || !game.TryGet(out SpectateController spec)
                || spec.TargetHuman == null)
            {
                _healthContainer.SetActive(false);
                _itemContainer.SetActive(false);
                return;
            }
            if (!_healthContainer.activeSelf)
            {
                _healthContainer.SetActive(true);
                _itemContainer.SetActive(true);
            }
            _health.text = spec.TargetHuman.Health.ToString();
            

            if(spec.TargetHuman.Health < _prevHealth)
            {
                _health.color = Color.red;
            }
            else if(spec.TargetHuman.Health > _prevHealth)
            {
                _health.color = Color.green;
            }

            _prevHealth = spec.TargetHuman.Health;
            _health.color = Color.Lerp(_health.color, _originalHealthColor, 4f * Time.deltaTime);

            var item = spec.TargetHuman.Equippables.Equipped;
            if(item == null 
                || item.EquippableGameObject == null
                || item.EquippableGameObject.Data == null)
            {
                _itemContainer.gameObject.SetActive(false);
                return;
            }

            _itemContainer.gameObject.SetActive(true);
            _itemName.text = item.EquippableGameObject.Data.Name;

            if(item.EquippableGameObject is MeleeEquippable melee)
            {
                _ammo.gameObject.SetActive(false);
                _itemLabel.gameObject.SetActive(false);
            }
            else if(item.EquippableGameObject is GunEquippable gun)
            {
                _itemLabel.gameObject.SetActive(true);
                _ammo.gameObject.SetActive(true);
                _ammo.text = $"{gun.RoundsInClip}/{gun.ExtraRounds}";
            }
        }

    }
}

