using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fragsurf.Utility;

namespace Fragsurf.UI
{
    public class UIManager : SingletonComponent<UIManager>
    {

        private List<IUIModal> _modals = new List<IUIModal>();

        public bool HasFocusedInputField()
        {
            foreach(var modal in _modals)
            {
                if (modal.HasFocusedInputField)
                {
                    return true;
                }
            }
            return false;
        }

        public void CloseModal(string modalName)
        {
            var modal = Find(modalName);
            if(modal != null && modal.IsOpen)
            {
                modal.Close();
                if (modal.ClosesOnEscape)
                {
                    RemoveFromEscapeStack(modal.ModalName);
                }
            }
        }

        public void OpenModal(string modalName)
        {
            var modal = Find(modalName);
            if (modal != null && !modal.IsOpen)
            {
                modal.Open();
                if (modal.ClosesOnEscape)
                {
                    AddToEscapeStack(modal.ModalName);
                }
            }
        }

        public void ToggleModal(string modalName)
        {
            var modal = Find(modalName);
            if(modal != null)
            {
                if (modal.IsOpen)
                {
                    CloseModal(modalName);
                }
                else
                {
                    OpenModal(modalName);
                }
            }
        }

        public IUIModal Find(string name)
        {
            return _modals.FirstOrDefault(x => string.Equals(x.ModalName, name, System.StringComparison.OrdinalIgnoreCase));
        }

        public void Add(IUIModal modal)
        {
            _modals.Add(modal);
        }

        public void Remove(IUIModal modal)
        {
            _modals.Remove(modal);
        }

        public bool HasCursor()
        {
            foreach(var modal in _modals)
            {
                if ((modal.IsOpen 
                    && modal.CursorType != CursorType.None)
                    || modal.HasFocusedInputField)
                {
                    return true;
                }
            }
            return false;
        }

        public void Alert(string message)
        {
            Debug.Log("ALERT: " + message);
        }

        private void AddToEscapeStack(string name)
        {
            throw new System.NotImplementedException();
        }

        private void RemoveFromEscapeStack(string name)
        {
            throw new System.NotImplementedException();
        }

    }
}

