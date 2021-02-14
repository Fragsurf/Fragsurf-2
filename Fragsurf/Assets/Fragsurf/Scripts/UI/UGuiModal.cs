using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class UGuiModal : MonoBehaviour
    {

        [SerializeField]
        private bool _standalone;
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
        private TMP_InputField[] _tmpInputFields;

        public bool IsStandalone => _standalone;

        public string Name
        {
            get => _modalName;
            set => _modalName = value;
        }
        public CursorType CursorType => _cursorType;
        public bool IsOpen => _modalContainer.activeSelf;

        protected virtual void Awake()
        {
            _inputFields = GetComponentsInChildren<InputField>();
            _tmpInputFields = GetComponentsInChildren<TMP_InputField>();

            if (!_modalContainer)
            {
                _modalContainer = gameObject;
            }

            if (!_standalone)
            {
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

            DevConsole.RegisterObject(this);
        }

        protected virtual void OnDestroy()
        {
            if (UGuiManager.Instance
                && !_standalone)
            {
                UGuiManager.Instance.Remove(this);
            }
            DevConsole.RemoveAll(this);
        }

        protected virtual void OnEnable()
        {
            if (_standalone)
            {
                Open();
            }
        }

        protected virtual void OnDisable()
        {
            if (_standalone)
            {
                Close();
            }
        }

        public bool HasFocusedInput()
        {
            foreach(var input in _inputFields)
            {
                if (input.isFocused)
                {
                    return true;
                }
            }
            foreach(var tmpInput in _tmpInputFields)
            {
                if (tmpInput.isFocused)
                {
                    return true;
                }
            }
            return false;
        }

        public void Open()
        {
            if (_standalone)
            {
                return;
            }
            _modalContainer.SetActive(true);
            if (!_ignoreEscape && !_standalone)
            {
                UGuiManager.Instance.AddToEscapeStack(this);
            }
            OnOpen();
        }

        public void Close()
        {
            if (_standalone)
            {
                return;
            }
            _modalContainer.SetActive(false);
            if (!_ignoreEscape && !_standalone)
            {
                UGuiManager.Instance.RemoveFromEscapeStack(this);
            }
            OnClose();
        }

        public void Toggle()
        {
            if (_standalone)
            {
                return;
            }
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

