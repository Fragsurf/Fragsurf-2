using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_ReplayListEntryData
    {
        public ReplayHuman Replay;
        public Action OnKick;
        public Action OnSpectate;
    }

    public class Modal_ReplayListEntry : EntryElement<Modal_ReplayListEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private Button _kickButton;
        [SerializeField]
        private Button _specButton;

        public override void LoadData(Modal_ReplayListEntryData data)
        {
            _name.text = data.Replay.DisplayName;
            _kickButton.onClick.AddListener(() =>
            {
                data.OnKick?.Invoke();
                if (data.Replay != null && data.Replay.IsValid())
                {
                    data.Replay.Delete();
                }
            });
            _specButton.onClick.AddListener(() =>
            {
                data.OnSpectate?.Invoke();
                if(data.Replay != null && data.Replay.IsValid())
                {
                    FSGameLoop.GetGameInstance(false).Get<SpectateController>().TargetHuman = data.Replay;
                }
            });

        }

    }
}