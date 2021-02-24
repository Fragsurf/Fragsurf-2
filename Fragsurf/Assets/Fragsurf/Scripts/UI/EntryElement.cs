using Fragsurf.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class EntryElement<T> : MonoBehaviour
    {

        public int EntryLimit = 100;
        public TMP_InputField SearchField;
        public abstract void LoadData(T data);
        protected virtual bool ContainsSearch(string input) { return true; }

        private List<GameObject> _children = new List<GameObject>();
        private bool _searchIsHooked;
        protected EntryElement<T> _parent;

        public List<GameObject> Children => _children;

        private void DoSearch(string input)
        {
            foreach(var child in _children)
            {
                if(!child.TryGetComponent(out EntryElement<T> ee))
                {
                    continue;
                }
                child.SetActive(string.IsNullOrWhiteSpace(input) || ee.ContainsSearch(input));
            }
        }

        public void Clear()
        {
            if(_children == null)
            {
                return;
            }

            if (SearchField)
            {
                SearchField.text = string.Empty;
            }

            foreach(var obj in _children)
            {
                GameObject.Destroy(obj);
            }
            _children.Clear();

            transform.parent.gameObject.RebuildLayout();
        }

        private EntryElement<T> SpawnEntry(T data)
        {
            var clone = GameObject.Instantiate(gameObject, transform.parent).GetComponent<EntryElement<T>>();
            clone._parent = this;
            clone.SearchField = null;
            _children.Add(clone.gameObject);
            clone.gameObject.SetActive(true);
            clone.LoadData(data);
            transform.parent.gameObject.RebuildLayout();

            if(_children.Count > EntryLimit)
            {
                GameObject.Destroy(_children[0]);
                _children.RemoveAt(0);
            }

            if (SearchField && !_searchIsHooked)
            {
                SearchField.onValueChanged.AddListener(DoSearch);
                _searchIsHooked = true;
            }

            return clone;
        }

        public EntryElement<T> Prepend(T data)
        {
            var entry = SpawnEntry(data);
            entry.transform.SetAsFirstSibling();
            return entry;
        }

        public EntryElement<T> Append(T data)
        {
            var entry = SpawnEntry(data);
            entry.transform.SetAsLastSibling();
            return entry;
        }

        public void Remove(EntryElement<T> child)
        {
            if (child && child.gameObject)
            {
                _children.Remove(child.gameObject);
                GameObject.Destroy(child.gameObject);
            }
        }

    }
}
