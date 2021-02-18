using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Clans : UGuiModal
    {

        [ConVar("clans.channels", "", ConVarFlags.UserSetting | ConVarFlags.UserSettingHidden)]
        public string Channels { get; set; } = string.Empty;
        [ConVar("clans.default", "", ConVarFlags.UserSetting | ConVarFlags.UserSettingHidden)]
        public ulong DefaultChannel { get; set; }

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

            StopLoadingHistory();
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

        public void RefreshClanList()
        {
            if (!SteamClient.IsValid)
            {
                return;
            }

            _clanEntryTemplate.Clear();

            var clans = SteamFriends.GetClans().Where(x => IsClanAdded(x)).ToList();
            if(clans.Count() == 0)
            {
                // poo
                return;
            }

            var selectedId = clans.FindIndex(x => x.Id == DefaultChannel) != -1 
                ? DefaultChannel 
                : (ulong)clans[0].Id;

            foreach (var clan in clans)
            {
                var selected = clan.Id == selectedId;
                if (selected)
                {
                    SwitchToClan(clan);
                }
                _clanEntryTemplate.Append(new ClanEntryData()
                {
                    Clan = clan,
                    OnClick = () => SwitchToClan(clan),
                    Selected = selected
                });
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

            DefaultChannel = clan.Id;
            _pendingChats.Clear();
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

            _messageInput.text = string.Empty;
            _messageInput.ActivateInputField();
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
                StopLoadingHistory();
                _chatHistoryCoroutine = StartCoroutine(LoadChatHistory(messageId, clanChatId));
                return;
            }

            var data = new ClanChatEntryData()
            {
                Clan = _activeClan,
                Friend = friend,
                Message = message,
                MessageType = msgType
            };

            if (_loadingHistory)
            {
                _pendingChats.Add(data);
            }
            else
            {
                _clanChatEntryTemplate.Append(data);
            }
        }

        private List<ClanChatEntryData> _pendingChats = new List<ClanChatEntryData>();
        private Coroutine _chatHistoryCoroutine;
        private bool _loadingHistory;
        private IEnumerator LoadChatHistory(int startId, SteamId clanChatId)
        {
            _loadingHistory = true;

            const int chunksize = 5;
            const int historyNeeded = 100;

            var ids = new List<ClanChatEntryData>();
            var idx = 0;

            while(true)
            {
                idx++;
                if(idx > startId || ids.Count >= historyNeeded)
                {
                    break;
                }
                var msgId = startId - idx;
                if (!SteamFriends.GetClanChatMessage(clanChatId, msgId, out Friend friend, out string msgType, out string msg)
                    || msg == "__history__")
                {
                    continue;
                }
                ids.Add(new ClanChatEntryData()
                {
                    Clan = _activeClan,
                    Friend = friend,
                    Message = msg,
                    MessageType = msgType
                });
            }

            ids.Reverse();

            foreach(var chunk in SplitList(ids, chunksize))
            {
                foreach(var chatData in chunk)
                {
                    _clanChatEntryTemplate.Append(chatData);
                }
                yield return 0;
            }

            _loadingHistory = false;

            foreach(var chat in _pendingChats)
            {
                _clanChatEntryTemplate.Append(chat);
            }
            _pendingChats.Clear();
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Mathf.Min(nSize, locations.Count - i));
            }
        }

        private void StopLoadingHistory()
        {
            if(_chatHistoryCoroutine != null)
            {
                StopCoroutine(_chatHistoryCoroutine);
                _chatHistoryCoroutine = null;
            }
            _loadingHistory = false;
        }

        public bool IsClanAdded(Clan clan)
        {
            if (string.IsNullOrWhiteSpace(Channels))
            {
                return false;
            }
            return Channels.Contains(clan.Id.ToString());
        }

        public void AddClan(Clan clan)
        {
            if (IsClanAdded(clan))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(Channels))
            {
                Channels = string.Empty;
            }
            Channels += clan.Id + ",";
        }

        public void RemoveClan(Clan clan)
        {
            if (!IsClanAdded(clan))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(Channels))
            {
                Channels = string.Empty;
                return;
            }
            Channels = Channels.Replace(clan.Id.ToString() + ",", string.Empty);
        }

    }
}

