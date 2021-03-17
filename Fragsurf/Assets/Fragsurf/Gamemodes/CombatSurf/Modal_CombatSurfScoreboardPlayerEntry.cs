using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using Fragsurf.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.Gamemodes.CombatSurf
{
    public class Modal_CombatSurfScoreboardPlayerEntry : EntryElement<Modal_CombatSurfScoreboardPlayerEntry.Data>
    {

        public class Data 
        {
            public IPlayer Player;
        }

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _score;
        [SerializeField]
        private SteamAvatar _steamAvatar;
        [SerializeField]
        private GameObject _isPlayerObject;

        public override void LoadData(Data data)
        {
            var localClient = -1;
            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                localClient = cl.ClientIndex;
            }

            if (_isPlayerObject)
            {
                _isPlayerObject.gameObject.SetActive(data.Player.ClientIndex == localClient);
            }

            _name.text = data.Player.DisplayName;
            if (_steamAvatar)
            {
                _steamAvatar.SteamId = data.Player.SteamId;
                _steamAvatar.Fetch();
            }

            if (!cl)
            {
                _score.text = string.Empty;
                return;
            }

            var stats = cl.Get<CombatSurfStatTracker>();
            var kills = stats.GetKills(data.Player.ClientIndex);
            var deaths = stats.GetDeaths(data.Player.ClientIndex);
            var damage = stats.GetDamage(data.Player.ClientIndex);
            var latency = data.Player.LatencyMs;
            _score.text = $"{latency}ms | <color=yellow>{damage}</color> dmg | <color=green>{kills}</color> kills | <color=red>{deaths}</color> deaths";

        }

    }
}

