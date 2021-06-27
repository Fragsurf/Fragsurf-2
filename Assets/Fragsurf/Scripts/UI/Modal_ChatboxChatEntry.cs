using Fragsurf.Shared.Player;
using Fragsurf.Utility;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_ChatboxChatEntryData
    {
        public string PlayerName;
        public string Message;
        public string ClanTag;
        public int Team = -1;
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
        private StringBuilder _sb = new StringBuilder();
        private const string _sbFormat = "{0} <color={1}>{2} |</color> {3}";

        protected override bool AutoRebuildLayout => false;

        public override void LoadData(Modal_ChatboxChatEntryData data)
        {
            var clanTag = !string.IsNullOrEmpty(data.ClanTag) ? data.ClanTag : string.Empty;
            Color teamColor = new Color32(245, 157, 15, 255);
            if(data.Team != -1)
            {
                teamColor = PlayerManager.GetTeamColor(data.Team);
            }
            _sb.AppendFormat(_sbFormat, clanTag, teamColor.HashRGBA(), data.PlayerName, data.Message);
            _originalFadeDuration = _fadeDuration;
            _message.text = _sb.ToString();
            _chatbox.OnOpened.AddListener(OnChatboxOpened);
            _chatbox.OnClosed.AddListener(OnChatboxClosed);

            _sb.Clear();

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

