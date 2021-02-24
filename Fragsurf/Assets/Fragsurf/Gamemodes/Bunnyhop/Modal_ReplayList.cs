using Fragsurf.Client;
using Fragsurf.Shared;
using Fragsurf.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Gamemodes.Bunnyhop
{
    public class Modal_ReplayList : UGuiModal
    {

        private Modal_ReplayListEntry _replayEntryTemplate;

        private void Start()
        {
            _replayEntryTemplate = GetComponentInChildren<Modal_ReplayListEntry>();
            _replayEntryTemplate.gameObject.SetActive(false);
        }

        private void RefreshReplayList()
        {
            _replayEntryTemplate.Clear();
            foreach (var ent in FSGameLoop.GetGameInstance(false).EntityManager.Entities)
            {
                if (!(ent is ReplayHuman rep))
                {
                    continue;
                }
                _replayEntryTemplate.Append(new Modal_ReplayListEntryData()
                {
                    Replay = rep,
                    OnKick = () => RefreshReplayList(),
                    OnSpectate = () => { }
                });
            }
        }

        private void Update()
        {
            var count = 0;
            foreach (var ent in FSGameLoop.GetGameInstance(false).EntityManager.Entities)
            {
                if (!(ent is ReplayHuman rep))
                {
                    continue;
                }
                count++;
            }
            if(count != _replayEntryTemplate.Children.Count)
            {
                RefreshReplayList();
            }
        }

        protected override void OnOpen() => RefreshReplayList();

    }

}

