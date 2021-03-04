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
        public bool IsOpen => _modalContainer.activeInHierarchy;

        protected virtual void Awake()
        {
            UGuiManager.Instance.Add(this);
            DevConsole.RegisterObject(this);

            GetInputFields();

            if (!_modalContainer)
            {
                _modalContainer = gameObject;
            }

            StartCoroutine(CloseOnStart());
        }

        protected void GetInputFields()
        {
            _inputFields = GetComponentsInChildren<InputField>(true);
            _tmpInputFields = GetComponentsInChildren<TMP_InputField>(true);
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
            if (transform.parent)
            {
                var parentModal = transform.parent.GetComponentInParent<UGuiModal>();
                if (parentModal)
                {
                    parentModal.Open();
                }
            }
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.AddToEscapeStack(this);
            }
            OnOpened?.Invoke();
            OnOpen();
        }

        public void Close()
        {
            _modalContainer.SetActive(false);
            if (!_ignoreEscape)
            {
                UGuiManager.Instance.RemoveFromEscapeStack(this);
            }
            OnClosed?.Invoke();
            OnClose();
        }

        public void Toggle()
        {
            if (IsOpen)
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

