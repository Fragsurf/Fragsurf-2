using Fragsurf.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class EntryElement<T> : MonoBehaviour
    {

        public abstract void LoadData(T data);

        private WaitForEndOfFrame _eof = new WaitForEndOfFrame();
        private List<GameObject> _children;

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

            RebuildLayout();
        }

        public void Append(T data)
        {
            if(_children == null)
            {
                _children = new List<GameObject>();
            }

            var clone = GameObject.Instantiate(gameObject).GetComponent<EntryElement<T>>();
            clone.gameObject.SetActive(true);
            _children.Add(clone.gameObject);
            var rt = clone.GetComponent<RectTransform>();
            var parentRt = transform.parent.GetComponent<RectTransform>();
            rt.SetParent(parentRt);
            rt.SetSiblingIndex(parentRt.childCount);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            clone.LoadData(data);

            RebuildLayout();
        }

        private void RebuildLayout()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(RebuildLayout(transform.parent.gameObject));
            }
        }

        private IEnumerator RebuildLayout(GameObject gameObj)
        {
            yield return _eof;
            yield return _eof;

            if (gameObj)
            {
                gameObj.RebuildLayout();
            }
        }

    }
}
