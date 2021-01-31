using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public class Modal_Console : UGuiModal
    {

        [SerializeField]
        private TMP_InputField _inputField;

        private Modal_ConsoleEntry _template;

        private void Start()
        {
            _template = GetComponentInChildren<Modal_ConsoleEntry>(true);
            if (!_template)
            {
                throw new System.Exception("Console is missing entry template or container!");
            }
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
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
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
            }
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

    }
}

