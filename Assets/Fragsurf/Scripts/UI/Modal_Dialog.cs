using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_Dialog : UGuiModal
    {

        public const string Identifier = "Dialog";

        [SerializeField]
        private TMP_Text _title;
        [SerializeField]
        private TMP_Text _message;
        [SerializeField]
        private Button _okButton;
        [SerializeField]
        private Button _cancelButton;
        [SerializeField]
        private TMP_InputField _input;

        private Action _onConfirm;
        private Action<string> _onConfirmInput;
        private Action _onCancel;

        private void Start()
        {
            _okButton.onClick.AddListener(() =>
            {
                Close();
                _onConfirm?.Invoke();
                _onConfirmInput?.Invoke(_input.text);
            });
            _cancelButton.onClick.AddListener(() =>
            {
                Close();
            });
            _input.text = string.Empty;
        }

        protected override void OnClose()
        {
            _onCancel?.Invoke();
        }

        public void Popup(string title, string message, Action onConfirm = null, string confirmButtonText = "Ok")
        {
            _title.text = title;
            _message.text = message;
            _okButton.GetComponentInChildren<TMP_Text>().text = confirmButtonText;
            _onConfirm = onConfirm;
            _onConfirmInput = null;
            _onCancel = null;
            _input.gameObject.SetActive(false);
            Open();
        }

        public void Confirmation(string title, string message, Action onConfirm, Action onCancel = null, string confirmButtonText = "Ok", string cancelButtonText = "Cancel")
        {
            _onCancel = onCancel;
            _cancelButton.GetComponentInChildren<TMP_Text>().text = cancelButtonText;
            _input.gameObject.SetActive(false);
            _onConfirmInput = null;
            Popup(title, message, onConfirm, confirmButtonText);
        }

        public void TakeInput(string title, string message, Action<string> onConfirm, Action onCancel = null, string confirmButtonText = "Ok", string cancelButtonText = "Cancel")
        {
            _onCancel = onCancel;
            _cancelButton.GetComponentInChildren<TMP_Text>().text = cancelButtonText;
            _input.gameObject.SetActive(true);
            _input.text = string.Empty;
            _title.text = title;
            _message.text = message;
            _onConfirmInput = null;
            _okButton.GetComponentInChildren<TMP_Text>().text = confirmButtonText;
            _onConfirm = null;
            _onConfirmInput = onConfirm;
            _onCancel = onCancel;
            Open();
            _input.ActivateInputField();
        }

    }
}

