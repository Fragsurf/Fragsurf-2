using Fragsurf.Utility;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Console : UGuiModal
    {

        public const string Identifier = "Console";

        [SerializeField]
        private TMP_InputField _inputField;
        private Modal_ConsoleEntry _template;

        public bool CloseParentModalOnClose;
        public CircularBuffer<string> InputHistory = new CircularBuffer<string>(20);
        
        private void Start()
        {
            _template = GetComponentInChildren<Modal_ConsoleEntry>(true);
            _template.gameObject.SetActive(false);
            _inputField.onSubmit.AddListener(OnInputSubmit);
            DevConsole.OnMessageLogged += OnMessageLogged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DevConsole.OnMessageLogged -= OnMessageLogged;
        }

        protected override void OnOpen()
        {
            CloseParentModalOnClose = true;
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        protected override void OnClose()
        {
            if (CloseParentModalOnClose && transform.parent)
            {
                var p = transform.parent.GetComponentInParent<UGuiModal>();
                if (p)
                {
                    p.Close();
                }
            }
        }

        private void OnMessageLogged(string message)
        {
            var consoleData = new Modal_ConsoleEntryData()
            {
                Message = message,
                ElapsedTime = Time.time
            };
            _template.Append(consoleData);
        }

        private void OnInputSubmit(string contents)
        {
            if (!string.IsNullOrWhiteSpace(contents))
            {
                DevConsole.ExecuteLine(contents);
                InputHistory.PushFront(contents);
            }
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

    }
}

