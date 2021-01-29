using UnityEngine;
using System.Collections.Generic;
using Fragsurf.Utility;

namespace Fragsurf.UI
{
    public class UGuiManager : SingletonComponent<UGuiManager>
    {

        private List<UGuiModal> _modals = new List<UGuiModal>();

        private void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
        }

        public void AddToEscapeStack(string name)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveFromEscapeStack(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool HasInputFocus()
        {
            foreach(var modal in _modals)
            {
                if (modal.HasInputFocus())
                {
                    return true;
                }
            }
            return false;
        }

        public void CloseModal(string modalName)
        {
            var modal = Find(modalName);
            if (modal)
            {
                modal.Close();
            }
        }

        public void OpenModal(string modalName)
        {
            var modal = Find(modalName);
            if(modal)
            {
                modal.Open();
            }
        }

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
            foreach(var modal in _modals)
            {
                if(string.Equals(modal.Name, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    return modal;
                }
            }
            return null;
        }

        public void Add(UGuiModal modal)
        {
            _modals.Add(modal);
        }

        public void Remove(UGuiModal modal)
        {
            _modals.Remove(modal);
        }

        public bool HasCursor()
        {
            foreach(var modal in _modals)
            {
                if (modal.IsOpen && modal.CursorType != CursorType.None)
                {
                    return true;
                }
            }
            return false;
        }

        public void Popup(string message)
        {
            Debug.LogError("Popup not implemented!" + message);
        }

    }
}

