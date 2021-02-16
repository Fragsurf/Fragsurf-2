using Steamworks;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Clans : UGuiModal
    {

        [SerializeField]
        private TMP_InputField _messageInput;

        private ClanChatEntry _clanChatEntryTemplate;
        private ClanEntry _clanEntryTemplate;
        private ClanMemberEntry _clanMemberEntry;
        private SteamId _clanChatId;
        private Clan _activeClan;

        private void Start()
        {
            _clanChatEntryTemplate = gameObject.GetComponentInChildren<ClanChatEntry>();
            _clanChatEntryTemplate.gameObject.SetActive(false);
            _clanEntryTemplate = gameObject.GetComponentInChildren<ClanEntry>();
            _clanEntryTemplate.gameObject.SetActive(false);
            _clanMemberEntry = gameObject.GetComponentInChildren<ClanMemberEntry>();
            _clanMemberEntry.gameObject.SetActive(false);

            RefreshClanList();

            SteamFriends.OnClanChatMessage += SteamFriends_OnClanChatMessage;
            SteamFriends.OnClanChatJoin += SteamFriends_OnClanChatJoin;
            SteamFriends.OnClanChatLeave += SteamFriends_OnClanChatLeave; ;

            _messageInput.onSubmit.AddListener(OnMessageSubmit);
        }

        private void SteamFriends_OnClanChatLeave(SteamId arg1, Friend arg2, bool arg3, bool arg4)
        {
            Debug.Log("Leave event");
        }

        private void SteamFriends_OnClanChatJoin(SteamId arg1, Friend arg2)
        {
            Debug.Log("Join event");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SteamFriends.OnClanChatMessage -= SteamFriends_OnClanChatMessage;
        }

        private void OnMessageSubmit(string message)
        {
            _messageInput.text = string.Empty;
            _messageInput.ActivateInputField();

            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            SteamFriends.SendClanChatRoomMessage(_clanChatId, message);

            Debug.Log("Send to: " + _clanChatId);
        }

        private void RefreshClanList()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }
            foreach(var clan in SteamFriends.GetClans())
            {
                var data = new ClanEntryData()
                {
                    Clan = clan,
                    OnClick = () => SwitchToClan(clan)
                };
                _clanEntryTemplate.Append(data);
            }
        }

        private void RefreshMemberList()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }
            _clanMemberEntry.Clear();
            foreach (var member in _activeClan.GetChatMembers())
            {
                _clanMemberEntry.Append(new ClanMemberEntryData()
                {
                    Friend = member,
                    OnClick = () => { Debug.Log("Do something"); }
                });
            }
        }

        private async void SwitchToClan(Clan clan)
        {
            SteamFriends.LeaveClanChatRoom(_clanChatId);

            _clanChatId = default;

            if (!SteamClient.IsValid)
            {
                return;
            }

            var joinResult = await SteamFriends.JoinClanChatRoom(clan.Id);
            if (!joinResult.Success)
            {
                Debug.Log("Failed to join clan chat: " + clan.Name);
                return;
            }

            _clanChatId = joinResult.Id;
            _activeClan = clan;

            RefreshMemberList();

            SteamFriends.SendClanChatRoomMessage(_clanChatId, $"<color=yellow>I have joined {clan.Name}</color>");
        }

        private void SteamFriends_OnClanChatMessage(SteamId clanChatId, Friend friend, string msgType, string message)
        {
            if (clanChatId != _clanChatId)
            {
                return;
            }

            _clanChatEntryTemplate.Append(new ClanChatEntryData()
            {
                Clan = _activeClan,
                Friend = friend,
                Message = message,
                MessageType = msgType
            });
        }

    }
}

