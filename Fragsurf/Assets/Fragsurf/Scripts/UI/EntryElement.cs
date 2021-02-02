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

            var clone = GameObject.Instantiate(gameObject, transform.parent).GetComponent<EntryElement<T>>();
            _children.Add(clone.gameObject);
            clone.gameObject.SetActive(true);
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
