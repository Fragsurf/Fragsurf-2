using UnityEngine;
using UnityEngine.UIElements;

namespace Fragsurf.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class GameDocument : MonoBehaviour
    {

        public string DocumentName;
        public CursorType CursorType;
        public bool SingleInstance;
        public bool OpenOnStart;

        public UIDocument UiDocument { get; private set; }

        public Focusable FocusedElement => UiDocument.rootVisualElement.focusController.focusedElement;
        public bool Focused => (IsOpen && ShowCursor) || FocusedElement != null;
        public bool ShowCursor { get; private set; }
        public bool IsOpen
        {
            get => UiDocument.enabled && UiDocument.rootVisualElement.visible;
            set => SetIsOpen(value);
        }

        private void SetIsOpen(bool value)
        {
            UiDocument.rootVisualElement.visible = value;
            if (CursorType == CursorType.Always)
            {
                ShowCursor = value;
            }

            if (value)
            {
                _OnOpen();
            }
            else 
            {
                _OnClose();
            }
        }

        private void Start()
        {
            if (!OpenOnStart)
            {
                IsOpen = false;
            }
        }

        private void OnEnable()
        {
            UiDocument = GetComponent<UIDocument>();
            UiDocument.rootVisualElement.Query<GameVisualElement>().ForEach(e => e.Document = UiDocument);

            if (string.IsNullOrEmpty(DocumentName)
                && UiDocument.rootVisualElement != null
                && UiDocument.rootVisualElement.childCount > 0)
            {
                DocumentName = UiDocument.rootVisualElement[0].name;
            }

            if (SingleInstance && GameDocumentManager.Instance.FindDocument(DocumentName))
            {
                GameObject.Destroy(gameObject);
                return;
            }

            GameDocumentManager.Instance.AddDocument(this);

            _OnEnable();

            SetIsOpen(true);
        }

        private void OnDisable()
        {
            if (GameDocumentManager.Instance)
            {
                GameDocumentManager.Instance.RemoveDocument(this);
            }

            _OnDisable();

            ShowCursor = false;
        }

        private void Update()
        {
            if (CursorType == CursorType.Click
                && Input.GetKeyDown(KeyCode.Mouse0)
                && !ShowCursor
                && IsOpen)
            {
                ShowCursor = true;
            }

            UiDocument.rootVisualElement.Query<GameVisualElement>().ForEach(e => e.Update());

            _Update();
        }

        protected virtual void _Update() { }
        protected virtual void _OnEnable() { }
        protected virtual void _OnDisable() { }
        protected virtual void _OnOpen() { }
        protected virtual void _OnClose() { }

    }
}


