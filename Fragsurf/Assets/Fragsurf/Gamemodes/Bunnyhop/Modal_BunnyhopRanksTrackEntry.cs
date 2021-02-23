using Fragsurf.Actors;
using Fragsurf.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_BunnyhopRanksTrackEntryData
    {
        public FSMTrack Track;
        public Action OnClick;
        public bool Selected;
    }
    public class Modal_BunnyhopRanksTrackEntry : EntryElement<Modal_BunnyhopRanksTrackEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _button;

        private static Modal_BunnyhopRanksTrackEntry _activeTab;

        public override void LoadData(Modal_BunnyhopRanksTrackEntryData data)
        {
            var name = data.Track.IsMainTrack ? $"[Main] {data.Track.TrackName}" : data.Track.TrackName;
            _name.text = $"{name} - {data.Track.TrackType}";
            _button.onClick.AddListener(() =>
            {
                if (_activeTab)
                {
                    _activeTab._button.interactable = true;
                }
                _activeTab = this;
                _button.interactable = false;
                data.OnClick?.Invoke();
            });
            if (data.Selected)
            {
                _button.interactable = false;
            }
        }
    }
}

