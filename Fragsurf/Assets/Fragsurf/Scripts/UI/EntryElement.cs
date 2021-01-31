using Fragsurf.Utility;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class EntryElement<T> : MonoBehaviour
    {

        public int EntryLimit = 100;
        public abstract void LoadData(T data);

        private List<GameObject> _children;

        public List<GameObject> Children => _children;

        public void Clear()
        {
            if(_children == null)
            {
                return;
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
            if (_children == null)
            {
                _children = new List<GameObject>();
            }

            var clone = GameObject.Instantiate(gameObject).GetComponent<EntryElement<T>>();
            clone.gameObject.SetActive(true);
            _children.Add(clone.gameObject);
            var rt = clone.GetComponent<RectTransform>();
            var parentRt = transform.parent.GetComponent<RectTransform>();
            rt.SetParent(parentRt);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            clone.LoadData(data);
            transform.parent.gameObject.RebuildLayout();

            if(_children.Count > EntryLimit)
            {
                GameObject.Destroy(_children[0]);
                _children.RemoveAt(0);
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

    }
}
