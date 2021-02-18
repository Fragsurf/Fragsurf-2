using Steamworks;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class ClanSettingsToggleEntryData 
    {
        public Clan Clan;
    }

    public class ClanSettingsToggleEntry : EntryElement<ClanSettingsToggleEntryData>, IPointerUpHandler, IPointerDownHandler
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        private Clan _clan;
        private Vector2 _downPos;

        public override void LoadData(ClanSettingsToggleEntryData data)
        {
            var clans = UGuiManager.Instance.Find<Modal_Clans>();
            _clan = data.Clan;
            _button.interactable = !clans.IsClanAdded(data.Clan);
            _name.text = data.Clan.Name;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _downPos = Input.mousePosition;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var dist = Vector2.Distance(_downPos, Input.mousePosition);
            if(dist > 10)
            {
                return;
            }

            var clans = UGuiManager.Instance.Find<Modal_Clans>();
            if (clans.IsClanAdded(_clan))
            {
                _button.interactable = true;
                clans.RemoveClan(_clan);
            }
            else
            {
                _button.interactable = false;
                clans.AddClan(_clan);
            }
        }

    }
}

