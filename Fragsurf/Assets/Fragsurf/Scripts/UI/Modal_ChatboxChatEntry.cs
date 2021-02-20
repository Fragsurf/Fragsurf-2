using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_ChatboxChatEntryData
    {
        public string PlayerName;
        public string Message;
        public string ClanTag;
    }
    public class Modal_ChatboxChatEntry : EntryElement<Modal_ChatboxChatEntryData>
    {

        [SerializeField]
        private Modal_Chatbox _chatbox;
        [SerializeField]
        private TMP_Text _name;
        [SerializeField]
        private TMP_Text _message;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private float _fadeDelay = 8f;
        [SerializeField]
        private float _fadeDuration = .1f;

        private float _originalFadeDuration;

        public override void LoadData(Modal_ChatboxChatEntryData data)
        {
            var name = $"{data.PlayerName} |";

            if (!string.IsNullOrEmpty(data.ClanTag))
            {
                name = $"<color=yellow>[{data.ClanTag}]</color> {name}";
            }

            _originalFadeDuration = _fadeDuration;
            _name.text = name;
            _message.text = data.Message;
            _chatbox.OnOpened.AddListener(OnChatboxOpened);
            _chatbox.OnClosed.AddListener(OnChatboxClosed);
        }

        private void Update()
        {
            if (_chatbox.IsOpen)
            {
                return;
            }

            if (_fadeDelay <= 0)
            {
                if(_fadeDuration <= 0)
                {
                    _canvasGroup.alpha = 0;
                    return;
                }
                _fadeDuration -= Time.deltaTime;
                _canvasGroup.alpha = _fadeDuration / _originalFadeDuration;
                return;
            }

            _fadeDelay -= Time.deltaTime;
        }

        private void OnDestroy()
        {
            if (_chatbox)
            {
                _chatbox.OnOpened.RemoveListener(OnChatboxOpened);
                _chatbox.OnClosed.RemoveListener(OnChatboxClosed);
            }
        }

        private void OnChatboxOpened()
        {
            _canvasGroup.alpha = 1;
        }

        private void OnChatboxClosed()
        {
            _canvasGroup.alpha = _fadeDuration > 0 ? _fadeDuration / _originalFadeDuration : 0f;
        }

    }
}

