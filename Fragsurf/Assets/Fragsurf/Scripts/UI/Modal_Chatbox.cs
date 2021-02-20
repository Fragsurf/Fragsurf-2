using Fragsurf.Shared;
using Fragsurf.Shared.Player;
using Fragsurf.Shared.Packets;
using TMPro;
using UnityEngine;
using System.Collections;
using Fragsurf.Utility;
using UnityEngine.UI;
using Steamworks;

namespace Fragsurf.UI
{
    public class Modal_Chatbox : UGuiModal
    {

        [Header("Chatbox Fields")]

        [SerializeField]
        private TMP_Text _targetChannel;
        [SerializeField]
        private TMP_InputField _input;
        [SerializeField]
        private GameObject[] _hideOnClose;
        [SerializeField]
        private ScrollRect _scrollRect;
        [SerializeField]
        private Toggle _enableClanChatToggle;
        [SerializeField]
        private GameObject _clanChatHint;

        private TextChat _textChat;
        private Modal_ChatboxChatEntry _chatTemplate;
        private bool _sendToClan;

        private void Start()
        {
            _chatTemplate = gameObject.GetComponentInChildren<Modal_ChatboxChatEntry>();
            _chatTemplate.gameObject.SetActive(false);
            _input.onSubmit.AddListener(OnSubmit);
            _enableClanChatToggle.onValueChanged.AddListener(ToggleClanChat);
            ToggleClanChat(false);
            SetSendToClan(false);
            HookTextChat();

            SteamFriends.OnClanChatMessage += SteamFriends_OnClanChatMessage;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SteamFriends.OnClanChatMessage -= SteamFriends_OnClanChatMessage;
        }

        private void SteamFriends_OnClanChatMessage(SteamId clanChatId, Friend friend, int msgId, string msgType, string msg)
        {
            if (!UGuiManager.Instance || !_enableClanChatToggle.isOn || msg == "__history__")
            {
                return;
            }
            var clans = UGuiManager.Instance.Find<Modal_Clans>();
            if (!clans || clanChatId != clans.ActiveClanChatId)
            {
                return;
            }
            ReceiveMessage(friend.Name, msg, "Clan");
        }

        protected override void OnClose()
        {
            _input.DeactivateInputField();
            _input.interactable = false;
            _scrollRect.enabled = false;
            foreach (var obj in _hideOnClose)
            {
                obj.SetActive(false);
            }
        }

        protected override void OnOpen()
        {
            foreach(var obj in _hideOnClose)
            {
                obj.SetActive(true);
            }
            _scrollRect.enabled = true;
            StartCoroutine(AfterOpen());
        }

        private IEnumerator AfterOpen()
        {
            _input.interactable = true;
            _input.text = string.Empty;
            yield return 0;
            _input.ActivateInputField();
        }

        private void Update()
        {
            // theoretically it will be null whenever the game client is destroyed and a new one is created
            // so we can hook back up to the new one
            if (!_textChat)
            {
                HookTextChat();
            }

            if(IsOpen && Input.GetKeyDown(KeyCode.Tab) && _enableClanChatToggle.isOn)
            {
                SetSendToClan(!_sendToClan);
            }
        }

        private void ToggleClanChat(bool value)
        {
            if (!value)
            {
                SetSendToClan(false);
            }
            _clanChatHint.SetActive(value);
            StartCoroutine(AfterOpen());
        }

        private void SetSendToClan(bool sendToClan)
        {
            _sendToClan = sendToClan;
            _targetChannel.text = sendToClan ? "[Clan]" : "[Game]";
            _targetChannel.transform.parent.gameObject.RebuildLayout();
        }

        private void HookTextChat()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (!cl || !cl.TextChat)
            {
                return;
            }
            _textChat = cl.TextChat;
            _textChat.OnMessageReceived += TextChat_OnMessageReceived;
        }

        private void TextChat_OnMessageReceived(ChatMessage chatPacket)
        {
            var pl = FSGameLoop.GetGameInstance(false).PlayerManager.FindPlayer(chatPacket.ClientIndex);
            if(pl == null)
            {
                return;
            }
            ReceiveMessage(pl.DisplayName, chatPacket.Message);
        }

        private void OnSubmit(string value)
        {
            _input.text = string.Empty;
            _input.DeactivateInputField();
            Close();
            if (string.IsNullOrWhiteSpace(value) || !_textChat)
            {
                return;
            }
            if (_sendToClan)
            {
                if (!SteamClient.IsValid)
                {
                    return;
                }
                var clans = UGuiManager.Instance.Find<Modal_Clans>();
                SteamFriends.SendClanChatRoomMessage(clans.ActiveClan.Id, value);
            }
            else
            {
                _textChat.MessageAll(value);
            }
        }

        private void ReceiveMessage(string playerName, string message, string clanTag = null)
        {
            _chatTemplate.Append(new Modal_ChatboxChatEntryData()
            {
                PlayerName = playerName,
                Message = message,
                ClanTag = clanTag
            });

            StartCoroutine(AfterMessage());
        }

        private IEnumerator AfterMessage()
        {
            yield return 0;
            _chatTemplate.transform.parent.gameObject.RebuildLayout();
        }

    }
}

