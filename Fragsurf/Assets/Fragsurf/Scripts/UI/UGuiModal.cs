using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public class UGuiModal : MonoBehaviour
    {

        public UnityEvent OnOpened = new UnityEvent();
        public UnityEvent OnClosed = new UnityEvent();

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
            UGuiManager.Instance.Add(this);
            DevConsole.RegisterObject(this);

            _inputFields = GetComponentsInChildren<InputField>();
            _tmpInputFields = GetComponentsInChildren<TMP_InputField>();

            if (!_modalContainer)
            {
                _modalContainer = gameObject;
            }

            StartCoroutine(CloseOnStart());
        }

        private IEnumerator CloseOnStart()
        {
            yield return new WaitForEndOfFrame();
            if (_closeOnStart)
            {
                Close();
            }
            else
            {
                Open();
            }
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
            OnOpened?.Invoke();
        }

        public void Close()
        {
            _modalContainer.SetActive(false);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.RemoveFromEscapeStack(this);
            }
            OnClose();
            OnClosed?.Invoke();
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

