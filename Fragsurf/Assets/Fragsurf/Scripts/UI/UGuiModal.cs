using TMPro;
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
        private TMP_InputField[] _tmpInputFields;

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
            if (_closeOnStart)
            {
                Close();
            }
            else
            {
                Open();
            }
            UGuiManager.Instance.Add(this);
            DevConsole.RegisterObject(this);
        }

        protected virtual void OnDestroy()
        {
            if (UGuiManager.Instance)
            {
                UGuiManager.Instance.Remove(this);
            }
            DevConsole.RemoveAll(this);
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
            _modalContainer.SetActive(true);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.AddToEscapeStack(this);
            }
            OnOpen();
        }

        public void Close()
        {
            _modalContainer.SetActive(false);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.RemoveFromEscapeStack(this);
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

