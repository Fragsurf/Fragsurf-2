using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class Modal_ColorPicker : UGuiModal
    {

        public const string Identifier = "ColorPicker";

        [SerializeField]
        private Button _cancelButton;
        [SerializeField]
        private Button _saveButton;
        [SerializeField]
        private ColorPicker _colorPicker;

        private Color _returnColor;
        private Action<Color> _onSave;
        private Action _onCancel;

        private void Start()
        {
            _colorPicker.onColorChanged += OnColorChanged;
            _cancelButton.onClick.AddListener(OnCancel);
            _saveButton.onClick.AddListener(OnSave);
        }

        protected override void OnClose()
        {
            base.OnClose();

            _onCancel?.Invoke();
            _onSave = null;
            _onCancel = null;
        }

        private void OnColorChanged(Color color)
        {
            _returnColor = color;
        }

        public void OpenWithCallback(Color startColor, Action<Color> onSave, Action onCancel)
        {
            Open();
            _colorPicker.color = startColor;
            _onSave = onSave;
            _onCancel = onCancel;
        }

        private void OnCancel()
        {
            _onCancel?.Invoke();
            _onCancel = null;
            _onSave = null;
            Close();
        }

        private void OnSave()
        {
            _onSave?.Invoke(_returnColor);
            _onCancel = null;
            _onSave = null;
            Close();
        }

    }
}

