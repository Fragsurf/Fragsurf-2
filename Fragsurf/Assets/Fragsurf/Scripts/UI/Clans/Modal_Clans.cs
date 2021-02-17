using Steamworks;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
            SteamFriends.OnClanChatLeave += SteamFriends_OnClanChatLeave;

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
            SteamFriends.OnClanChatJoin -= SteamFriends_OnClanChatJoin;
            SteamFriends.OnClanChatLeave -= SteamFriends_OnClanChatLeave;

            if (SteamClient.IsValid && _activeClan.Id != 0)
            {
                SteamFriends.LeaveClanChatRoom(_activeClan.Id);
            }
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
        }

        private void RefreshClanList()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }
            _clanEntryTemplate.Clear();
            foreach (var clan in SteamFriends.GetClans())
            {
                var data = new ClanEntryData()
                {
                    Clan = clan,
                    OnClick = () => SwitchToClan(clan)
                };
                _clanEntryTemplate.Append(data);
            }
        }

        private async void RefreshMemberList()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }
            _clanMemberEntry.Clear();
            await Task.Delay(500);
            _clanMemberEntry.Clear();
            foreach (var member in SteamFriends.GetClanChatMembers(_activeClan.Id))
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
            if (!SteamClient.IsValid)
            {
                return;
            }

            _clanChatId = default;

            if(_clanChatId != 0 && _activeClan.Id != 0)
            {
                SteamFriends.LeaveClanChatRoom(_activeClan.Id);
            }
            
            await Task.Delay(100);
            var joinResult = await SteamFriends.JoinClanChatRoom(clan.Id);
            if (!joinResult.Success)
            {
                Debug.Log("Failed to join clan chat: " + clan.Name);
                return;
            }

            _clanChatId = joinResult.Id;
            _activeClan = clan;

            RefreshMemberList();

            SteamFriends.SendClanChatRoomMessage(_activeClan.Id, "__history__");
        }

        private void SteamFriends_OnClanChatMessage(SteamId clanChatId, Friend friend, int messageId, string msgType, string message)
        {
            if (clanChatId != _clanChatId)
            {
                return;
            }

            if (message == "__history__")
            {
                if(friend.Id != SteamClient.SteamId)
                {
                    return;
                }
                _clanChatEntryTemplate.Clear();
                var startIdx = Mathf.Max(messageId - 100, 0);
                var endIdx = messageId - 1;
                for(int i = startIdx; i <= endIdx; i++)
                {
                    if(!SteamFriends.GetClanChatMessage(clanChatId, i, out Friend hChatter, out string hMsgType, out string hMessage)
                        || hMessage == "__history__")
                    {
                        continue;
                    }
                    _clanChatEntryTemplate.Append(new ClanChatEntryData()
                    {
                        Clan = _activeClan,
                        Friend = hChatter,
                        MessageType = hMsgType,
                        Message = hMessage
                    });
                }
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

