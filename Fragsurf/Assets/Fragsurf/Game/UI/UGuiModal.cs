using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class UGuiModal : MonoBehaviour, IUIModal
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

        public string ModalName => _modalName;
        public CursorType CursorType => _cursorType;
        public bool IsOpen => _modalContainer.activeSelf;
        public bool IsHovered => false;
        bool IUIModal.HasFocusedInputField => HasFocusedInputField();
        bool IUIModal.ClosesOnEscape => !_ignoreEscape;

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
            UIManager.Instance.Add(this);
        }

        protected virtual void OnDestroy()
        {
            UIManager.Instance.Remove(this);
        }

        public bool HasFocusedInputField()
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
            OnOpen();
        }

        public void Close()
        {
            _modalContainer.SetActive(false);
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

