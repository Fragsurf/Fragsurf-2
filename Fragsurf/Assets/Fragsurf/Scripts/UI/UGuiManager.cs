using UnityEngine;
using System.Collections.Generic;
using Fragsurf.Utility;
using System;
using UnityEngine.EventSystems;

namespace Fragsurf.UI
{
    public class UGuiManager : SingletonComponent<UGuiManager>
    {

        [SerializeField]
        private EventSystem _eventSystem;
        [SerializeField]
        private Canvas _mainCanvas;

        private Dictionary<string, UGuiModal> _modals = new Dictionary<string, UGuiModal>(StringComparer.OrdinalIgnoreCase);
        private List<UGuiModal> _escapeStack = new List<UGuiModal>();

        public bool EscapeEnabled = true;
        public Canvas Canvas => _mainCanvas;

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);

            DevConsole.RegisterObject(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DevConsole.RemoveAll(this);
        }

        private void Update()
        {
            if (EscapeEnabled && Input.GetKeyDown(KeyCode.Escape))
            {
                for (int i = _escapeStack.Count - 1; i >= 0; i--)
                {
                    if (!_escapeStack[i].IsOpen)
                    {
                        _escapeStack.RemoveAt(i);
                    }
                }
                if (_escapeStack.Count == 0)
                {
                    OpenModal<Modal_MainMenu>();
                }
                else
                {
                    _escapeStack[0].Close();
                }
            }
        }

        public void AddToEscapeStack(UGuiModal modal)
        {
            if (_escapeStack.Contains(modal))
            {
                _escapeStack.Remove(modal);
            }
            _escapeStack.Insert(0, modal);
        }

        public void RemoveFromEscapeStack(UGuiModal modal)
        {
            _escapeStack.Remove(modal);
        }

        public bool HasFocusedInput()
        {
            foreach(var kvp in _modals)
            {
                if (kvp.Value.HasFocusedInput())
                {
                    return true;
                }
            }
            return false;
        }

        public void OpenModal<T>()
            where T : UGuiModal
        {
            var m = Find<T>();
            if (m)
            {
                m.Open();
            }
        }

        public void CloseModal<T>()
            where T : UGuiModal
        {
            var m = Find<T>();
            if (m)
            {
                m.Close();
            }
        }

        public void ToggleModal<T>()
            where T : UGuiModal
        {
            var m = Find<T>();
            if (m)
            {
                m.Toggle();
            }
        }

        [ConCommand("modal.close", "Closes a modal", ConVarFlags.Silent)]
        public void CloseModal(string modalName)
        {
            var modal = Find(modalName);
            if (modal)
            {
                modal.Close();
            }
        }

        [ConCommand("modal.open", "Opens a modal", ConVarFlags.Silent)]
        public void OpenModal(string modalName)
        {
            var modal = Find(modalName);
            if(modal)
            {
                var mp = modal.GetComponentInParent<UGuiModal>();
                if (mp)
                {
                    mp.Open();
                }
                modal.Open();
            }
        }

        [ConCommand("modal.toggle", "Toggles a modal open or closed", ConVarFlags.Silent)]
        public void ToggleModal(string modalName)
        {
            var modal = Find(modalName);
            if (modal)
            {
                modal.Toggle();
            }
        }

        public T Find<T>()
            where T : UGuiModal
        {
            foreach (var m in _modals)
            {
                if (m.Value is T)
                {
                    return m.Value as T;
                }
            }
            return null;
        }

        public UGuiModal Find(string name)
        {
            if(_modals.TryGetValue(name, out UGuiModal m))
            {
                return m;
            }
            return null;
        }

        public void Add(UGuiModal modal)
        {
            if (_modals.ContainsKey(modal.Name))
            {
                throw new Exception("Modal already exists: " + modal.Name);
            }
            _modals.Add(modal.Name, modal);
        }

        public void Remove(UGuiModal modal)
        {
            _modals.Remove(modal.Name);
        }

        public bool HasCursor()
        {
            foreach(var kvp in _modals)
            {
                if (kvp.Value.IsOpen && kvp.Value.CursorType != CursorType.None)
                {
                    return true;
                }
            }
            return false;
        }

        public void Popup(string message)
        {
            Debug.LogError("Popup not implemented:" + message);
        }

        public static void EnableEventSystem()
        {
            if(Instance && Instance._eventSystem)
            {
                Instance._eventSystem.enabled = true;
            }
        }

        public static void DisableEventSystem()
        {
            if (Instance && Instance._eventSystem)
            {
                Instance._eventSystem.enabled = false;
            }
        }

    }
}

