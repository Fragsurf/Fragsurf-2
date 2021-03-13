using Fragsurf.Shared.Entity;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_EquippablesItemEntry : EntryElement<Modal_EquippablesItemEntry.Data>
    {

        public class Data
        {
            public Equippable Item;
        }

        [SerializeField]
        private TMP_Text _itemName;

        private Equippable _item;

        public override void LoadData(Data data)
        {
            _itemName.text = data.Item.ItemName;
            _item = data.Item;
        }

        private void Update()
        {
            if (_item == null 
                || !_item.IsValid())
            {
                _item = null;
                _parent.Remove(this);
                return;
            }
            _itemName.color = _item.Equipped ? Color.green : Color.white;
        }

    }
}

