using System.Collections;
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
        private TMP_Text _message;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private float _fadeDelay = 8f;
        [SerializeField]
        private float _fadeDuration = .1f;

        private float _originalFadeDuration;

        protected override bool AutoRebuildLayout => false;

        public override void LoadData(Modal_ChatboxChatEntryData data)
        {
            var name = $"<color=orange>{data.PlayerName} |</color>";
            var msg = data.Message;

            if (!string.IsNullOrEmpty(data.ClanTag))
            {
                name = $"<color=yellow>[{data.ClanTag}]</color> {name}";
                msg = $"<i>{msg}</i>";
            }

            _originalFadeDuration = _fadeDuration;
            _message.text = $"{name} {msg}";
            _chatbox.OnOpened.AddListener(OnChatboxOpened);
            _chatbox.OnClosed.AddListener(OnChatboxClosed);

            StartCoroutine(SetSize());
        }

        private IEnumerator SetSize()
        {
            yield return 0; 
            GetComponent<RectTransform>().sizeDelta = _message.textBounds.size;
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
                    gameObject.SetActive(false);
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
            gameObject.SetActive(true);
        }

        private void OnChatboxClosed()
        {
            _canvasGroup.alpha = _fadeDuration > 0 ? _fadeDuration / _originalFadeDuration : 0f;
            gameObject.SetActive(_fadeDuration > 0);
        }

    }
}

