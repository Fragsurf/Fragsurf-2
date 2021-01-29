using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class UGuiModal : MonoBehaviour
    {

        [SerializeField]
        private GameObject _modalContainer;
        [SerializeField]
        private string _modalName;
        [SerializeField]
        private bool _closeOnStart;
        [SerializeField]
        private bool _ignoreEscape;
        [SerializeField]
        private CursorType _cursorType;

        private InputField[] _inputFields;

        public string Name => _modalName;
        public CursorType CursorType => _cursorType;
        public bool IsOpen => _modalContainer.activeSelf;

        protected virtual void Awake()
        {
            _inputFields = GetComponentsInChildren<InputField>();

            if (!_modalContainer)
            {
                _modalContainer = gameObject;
            }
            if (_closeOnStart)
            {
                Close();
            }
            else
            {
                Open();
            }
            UGuiManager.Instance.Add(this);
        }

        protected virtual void OnDestroy()
        {
            if (UGuiManager.Instance)
            {
                UGuiManager.Instance.Remove(this);
            }
        }

        public bool HasInputFocus()
        {
            if(_inputFields == null || _inputFields.Length == 0)
            {
                return false;
            }
            foreach(var input in _inputFields)
            {
                if (input.isFocused)
                {
                    return true;
                }
            }
            return false;
        }

        public void Open()
        {
            _modalContainer.SetActive(true);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.AddToEscapeStack(Name);
            }
            OnOpen();
        }

        public void Close()
        {
            _modalContainer.SetActive(false);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.RemoveFromEscapeStack(Name);
            }
            OnClose();
        }

        public void Toggle()
        {
            if (_modalContainer.activeSelf)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }

    }
}

