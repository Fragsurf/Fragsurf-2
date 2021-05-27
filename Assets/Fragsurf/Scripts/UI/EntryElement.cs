using Fragsurf.Utility;
using Mosframe;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.UI
{
    public abstract class EntryElement<T> : MonoBehaviour, IDynamicScrollViewItem
    {

        public int EntryLimit = 100;
        public TMP_InputField SearchField;
        public DynamicScrollView DynamicScrollView;
        public abstract void LoadData(T data);
        protected virtual bool ContainsSearch(string input) { return true; }
        protected virtual bool DataContainsSearch(T data, string input) { return true; }

        public static List<T> DynamicScrollData => _filtered ? _filteredDynamicScrollData : _dynamicScrollData;

        // todo: this can't be static, there can only be 1 dynamic scroll view..
        private static bool _filtered;
        private static List<T> _dynamicScrollData = new List<T>();
        private static List<T> _filteredDynamicScrollData = new List<T>();

        // todo: probably don't need 2 children lists..
        private List<GameObject> _children = new List<GameObject>();
        private List<EntryElement<T>> _childrenElements = new List<EntryElement<T>>();
        private bool _searchIsHooked;
        protected EntryElement<T> _parent;

        public List<GameObject> Children => _children;
        public List<EntryElement<T>> ChildrenElements => _childrenElements;

        protected virtual bool AutoRebuildLayout => true;

        private void DoSearch(string input)
        {
            if(DynamicScrollView)
            {
                _filtered = true;
                _filteredDynamicScrollData.Clear();
                foreach(var d in _dynamicScrollData)
                {
                    if(!DataContainsSearch(d, input))
                    {
                        continue;
                    }
                    _filteredDynamicScrollData.Add(d);
                }
                DynamicScrollView.totalItemCount = _filteredDynamicScrollData.Count;
                return;
            }

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
            _childrenElements.Clear();

            if (AutoRebuildLayout)
            {
                transform.parent.gameObject.RebuildLayout();
            }

            if(DynamicScrollView)
            {
                DynamicScrollView.totalItemCount = 0;
            }
        }

        private EntryElement<T> SpawnEntry(T data)
        {
            var clone = GameObject.Instantiate(gameObject, transform.parent).GetComponent<EntryElement<T>>();
            clone._parent = this;
            clone.SearchField = null;
            _children.Add(clone.gameObject);
            _childrenElements.Add(clone);
            clone.gameObject.SetActive(true);
            clone.LoadData(data);

            return clone;
        }

        public EntryElement<T> Prepend(T data)
        {
            if (SearchField && !_searchIsHooked)
            {
                SearchField.onValueChanged.AddListener(DoSearch);
                _searchIsHooked = true;
            }

            if (DynamicScrollView)
            {
                _dynamicScrollData.Insert(0, data);
                DynamicScrollView.totalItemCount++;
                return null;
            }

            if (_children.Count > EntryLimit)
            {
                GameObject.Destroy(_children[_children.Count - 1]);
                _children.RemoveAt(_children.Count - 1);
            }
            var entry = SpawnEntry(data);
            entry.transform.SetAsFirstSibling();
            if (AutoRebuildLayout)
            {
                transform.parent.gameObject.RebuildLayout();
            }
            return entry;
        }

        public EntryElement<T> Append(T data)
        {
            if (SearchField && !_searchIsHooked)
            {
                SearchField.onValueChanged.AddListener(DoSearch);
                _searchIsHooked = true;
            }

            if (DynamicScrollView)
            {
                _dynamicScrollData.Insert(DynamicScrollData.Count, data);
                DynamicScrollView.totalItemCount++;
                return null;
            }

            if (_children.Count > EntryLimit)
            {
                GameObject.Destroy(_children[0]);
                _children.RemoveAt(0);
            }

            var entry = SpawnEntry(data);
            entry.transform.SetAsLastSibling();
            if (AutoRebuildLayout)
            {
                transform.parent.gameObject.RebuildLayout();
            }
            return entry;
        }

        public void Remove(EntryElement<T> child)
        {
            _childrenElements.Remove(child);
            if (child && child.gameObject)
            {
                _children.Remove(child.gameObject);
                GameObject.Destroy(child.gameObject);
            }
        }

        public void onUpdateItem(int index)
        {
            if(index >= 0 
                && DynamicScrollData.Count > 0 
                && index < DynamicScrollData.Count)
            {
                LoadData(DynamicScrollData[index]);
            }
        }

    }
}
