using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_EquippablesSlotEntry : EntryElement<Modal_EquippablesSlotEntry.Data>
    {
        public class Data
        {
            public string Bind;
            public ItemSlot Slot;
        }

        [SerializeField]
        private TMP_Text _slotName;

        private ItemSlot _slot;
        private Modal_EquippablesItemEntry _itemTemplate;

        public override void LoadData(Data data)
        {
            _slot = data.Slot;
            _itemTemplate = gameObject.GetComponentInChildren<Modal_EquippablesItemEntry>();
            _itemTemplate.gameObject.SetActive(false);
            _slotName.text = $"({data.Bind}) {data.Slot}";
        }

        public void BuildItems()
        {
            _itemTemplate.Clear();
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
            foreach(var item in spec.TargetHuman.Equippables.Items)
            {
                if(item.EquippableGameObject.Data.Slot == _slot)
                {
                    _itemTemplate.Append(new Modal_EquippablesItemEntry.Data()
                    {
                        Item = item
                    });
                }
            }
            StartCoroutine(RebuildAfterFrame());
        }

        private IEnumerator RebuildAfterFrame()
        {
            yield return 0;
            transform.parent.gameObject.RebuildLayout();
        }

    }
}

