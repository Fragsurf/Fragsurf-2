using Fragsurf.Shared;
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

        private GameAudioSource _swapSound;
        private Equippable _item;
        private bool _wasEquipped;

        public override void LoadData(Data data)
        {
            if(gameObject.TryGetComponent(out _swapSound))
            {
                _swapSound.Category = SoundCategory.UI;
            }
            _itemName.text = data.Item.ItemName;
            _item = data.Item;
        }

        private void Update()
        {
            if (_item == null 
                || !_item.IsValid())
            {
                _wasEquipped = false;
                _item = null;
                _parent.Remove(this);
                return;
            }

            if(!_wasEquipped && _item.Equipped)
            {
                if (_swapSound)
                {
                    _swapSound.Play();
                }
                _itemName.color = Color.cyan;
            }
            else if(_wasEquipped && !_item.Equipped)
            {
                _itemName.color = Color.white;
            }

            _wasEquipped = _item.Equipped;
        }

    }
}

