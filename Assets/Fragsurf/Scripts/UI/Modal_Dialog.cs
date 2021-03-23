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

        private Action _onConfirm;
        private Action _onCancel;

        private void Start()
        {
            _okButton.onClick.AddListener(() =>
            {
                Close();
                _onConfirm?.Invoke();
            });
            _cancelButton.onClick.AddListener(() =>
            {
                Close();
            });
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
            _onCancel = null;
            Open();
        }

        public void Confirmation(string title, string message, Action onConfirm, Action onCancel = null, string confirmButtonText = "Ok", string cancelButtonText = "Cancel")
        {
            _onCancel = onCancel;
            _cancelButton.GetComponentInChildren<TMP_Text>().text = cancelButtonText;
            Popup(title, message, onConfirm, confirmButtonText);
        }

    }
}

