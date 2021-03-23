using Steamworks;
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

        private static ClanChatEntry _lastEntry;
        private static SteamId _lastMessageId;

        public new void Clear()
        {
            _lastEntry = null;
            _lastMessageId = 0;
            base.Clear();
        }

        public override void LoadData(ClanChatEntryData data)
        {
            if (_lastMessageId == data.Friend.Id)
            {
                _lastEntry._message.text += "\n" + data.Message;
                _parent.Remove(this);
                gameObject.SetActive(false);
                return;
            }

            _name.text = !string.IsNullOrEmpty(data.Friend.Name) ? data.Friend.Name : "Unknown";
            _clan.text = !string.IsNullOrEmpty(data.Friend.Name) ? data.Clan.Name : "Unknown";
            _message.text = data.Message;
            _message.richText = false;
            _lastMessageId = data.Friend.Id;
            _lastEntry = this;

            var avatar = GetComponentInChildren<SteamAvatar>();
            if (avatar)
            {
                avatar.SteamId = data.Friend.Id;
                avatar.Fetch();
            }
        }

    }
}

