using Fragsurf.Movement;
using Fragsurf.Shared.Packets;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public class EquippableManager 
    {

        private List<Equippable> _equippables = new List<Equippable>();
        private Equippable _lastEquipped;

        private Dictionary<InputActions, ItemSlot> _buttonToSlot = new Dictionary<InputActions, ItemSlot>()
        {
            { InputActions.Slot1, ItemSlot.Melee },
            { InputActions.Slot2, ItemSlot.Light },
            { InputActions.Slot3, ItemSlot.Heavy },
            { InputActions.Slot4, ItemSlot.Utility }
        };

        public int Count => _equippables.Count;
        public Equippable Equipped { get; private set; }
        public List<Equippable> Items => _equippables;

        public void RunCommand(UserCmd.CmdFields userCmd)
        {
            if (userCmd.Buttons.HasFlag(InputActions.Drop)
                && Equipped != null)
            {
                Equipped.Drop();
                return;
            }

            if (userCmd.Buttons.HasFlag(InputActions.NextItem))
            {
                EquipItem(GetNextItem());
                return;
            }

            if (userCmd.Buttons.HasFlag(InputActions.PrevItem))
            {
                EquipItem(_lastEquipped ?? GetNextItem());
                return;
            }

            foreach (var kvp in _buttonToSlot)
            {
                if (userCmd.Buttons.HasFlag(kvp.Key))
                {
                    EquipItem(GetNextItem(kvp.Value));
                    return;
                }
            }

            if (Equipped != null)
            {
                Equipped.RunCommand(userCmd);
            }
        }

        public void SetEquippedItem(Equippable item)
        {
            if(item == null || Equipped == item)
            {
                return;
            }
            Equipped = item;
        }

        private void EquipItem(Equippable item)
        {
            if(item == null || Equipped == item)
            {
                return;
            }
            if(Equipped != null)
            {
                _lastEquipped = Equipped;
                Equipped.Equipped = false;
            }
            item.Equipped = true;
        }

        public void Add(Equippable item)
        {
            if (_equippables.Contains(item))
            {
                Debug.LogError("Adding Equippable twice..");
                return;
            }

            _equippables.Add(item);

            if (!item.EquippableGameObject)
            {
                return;
            }

            for(int i = Items.Count - 1; i >= 0; i--)
            {
                var itm = Items[i];
                if(itm == item 
                    || !itm.EquippableGameObject
                    || itm.EquippableGameObject.Data.Slot != item.EquippableGameObject.Data.Slot)
                {
                    continue;
                }
                itm.Drop();
            }
        }

        public void DropAllItems()
        {
            for(int i = _equippables.Count - 1; i >= 0; i--)
            {
                _equippables[i].Drop();
                //_equippables[i].HumanId = -1;
            }
        }

        public void Remove(Equippable item)
        {
            if(_lastEquipped == item)
            {
                _lastEquipped = null;
            }
            if(item == Equipped)
            {
                Equipped = null;
            }
            _equippables.Remove(item);
        }

        public bool HasItem(string itemName)
        {
            foreach(var item in _equippables)
            {
                if(string.Equals(item.ItemName, itemName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasItemInSlot(ItemSlot slot)
        {
            foreach(var item in _equippables)
            {
                if(item.EquippableGameObject.Data.Slot == slot)
                {
                    return true;
                }
            }
            return false;
        }

        public Equippable GetNextItem(ItemSlot slot = ItemSlot.None)
        {
            if(_equippables.Count == 1)
            {
                return _equippables[0];
            }

            var choices = slot == ItemSlot.None
                ? _equippables
                : _equippables.Where(x => x.EquippableGameObject && x.EquippableGameObject.Data.Slot == slot).ToList();

            if(choices.Count <= 1)
            {
                return choices.Count == 0 ? null : choices[0];
            }

            var nextIndex = choices.IndexOf(Equipped) + 1;
            return choices[nextIndex == choices.Count ? 0 : nextIndex];
        }

    }
}

