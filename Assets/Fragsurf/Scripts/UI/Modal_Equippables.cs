using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fragsurf.Shared.Entity;
using System;
using Fragsurf.Utility;
using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.Movement;

namespace Fragsurf.UI
{
    public class Modal_Equippables : UGuiModal
    {

        private Modal_EquippablesSlotEntry _slotTemplate;
        private float _hideTimer;
        private int _prevItemCount;
        private int _lastHuId;
        private const float _hideDelay = 3f;

        private void Start()
        {
            _slotTemplate = gameObject.GetComponentInChildren<Modal_EquippablesSlotEntry>(true);
            _slotTemplate.gameObject.SetActive(false);

            BuildSlots();
        }

        private void Update()
        {
            if(_hideTimer > 0)
            {
                _hideTimer -= Time.deltaTime;
                if(_hideTimer <= 0)
                {
                    HideSlots(true);
                }
            }

            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl)
            {
                return;
            }

            var spec = cl.Get<SpectateController>();
            if (!spec || spec.TargetHuman == null)
            {
                return;
            }

            var id = spec.TargetHuman.EntityId;
            var count = spec.TargetHuman.Equippables.Items.Count;
            var btns = spec.TargetHuman.CurrentCmd.Buttons;
            if(btns.HasFlag(InputActions.Slot1)
                || btns.HasFlag(InputActions.Slot2)
                || btns.HasFlag(InputActions.Slot3)
                || btns.HasFlag(InputActions.Slot4)
                || btns.HasFlag(InputActions.Slot5))
            {
                HideSlots(false);
                if(_hideTimer <= 0)
                {
                    BuildSlotItems();
                    _prevItemCount = count;
                    _lastHuId = id;
                }
                _hideTimer = _hideDelay;
            }

            // try to update whenever there's a change
            if (_hideTimer > 0 
                && (btns.HasFlag(InputActions.Drop) || _prevItemCount != count || _lastHuId != id))
            {
                BuildSlotItems();
                _prevItemCount = count;
                _lastHuId = id;
            }
        }

        private void BuildSlots()
        {
            _slotTemplate.Clear();
            foreach(ItemSlot slot in Enum.GetValues(typeof(ItemSlot)))
            {
                if(slot == ItemSlot.None)
                {
                    continue;
                }
                _slotTemplate.Append(new Modal_EquippablesSlotEntry.Data()
                {
                    Slot = slot,
                    Bind = GetBindKey(SlotToAction(slot))
                });
            }
            _slotTemplate.transform.parent.gameObject.RebuildLayout();

            HideSlots(true);
        }

        private InputActions SlotToAction(ItemSlot slot)
        {
            switch (slot)
            {
                case ItemSlot.Heavy:
                    return InputActions.Slot1;
                case ItemSlot.Light:
                    return InputActions.Slot2;
                case ItemSlot.Melee:
                    return InputActions.Slot3;
                case ItemSlot.Utility:
                    return InputActions.Slot4;
            }
            return InputActions.None;
        }

        private string GetBindKey(InputActions action)
        {
            var binds = UserSettings.Binds.FindBindDatas($"+input {action}");
            if (binds != null && binds.Count > 0)
            {
                return binds[0].KeyName.ToString().ToLower().Replace("alpha", "");
            }
            return "-";
        }

        private void HideSlots(bool hidden)
        {
            foreach (var slot in _slotTemplate.Children)
            {
                slot.gameObject.SetActive(!hidden);
            }
        }

        private void BuildSlotItems()
        {
            foreach (var slot in _slotTemplate.Children)
            {
                slot.GetComponent<Modal_EquippablesSlotEntry>().BuildItems();
            }
        }

    }
}

