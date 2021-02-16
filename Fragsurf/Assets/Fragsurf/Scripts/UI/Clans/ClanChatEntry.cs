using Steamworks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class ClanChatEntryData
    {
        public Clan Clan;
        public Friend Friend;
        public string MessageType;
        public string Message;
    }
    public class ClanChatEntry : EntryElement<ClanChatEntryData>
    {

        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _clan;
        [SerializeField]
        private TMP_Text _message;

        public override void LoadData(ClanChatEntryData data)
        {
            _name.text = data.Friend.Name;
            _clan.text = data.Clan.Name;
            _message.text = data.Message;
        }

    }
}

