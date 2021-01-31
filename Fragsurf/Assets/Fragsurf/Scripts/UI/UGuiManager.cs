using UnityEngine;
using System.Collections.Generic;
using Fragsurf.Utility;
using System;

namespace Fragsurf.UI
{
    public class UGuiManager : SingletonComponent<UGuiManager>
    {

        private Dictionary<string, UGuiModal> _modals = new Dictionary<string, UGuiModal>(StringComparer.OrdinalIgnoreCase);
        private List<UGuiModal> _escapeStack = new List<UGuiModal>();

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
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if(_escapeStack.Count == 0)
                {
                    OpenModal("EscapeMenu");
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

    }
}

