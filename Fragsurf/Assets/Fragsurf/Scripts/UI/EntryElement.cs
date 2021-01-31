using Fragsurf.Utility;
using System.Collections;
using UnityEngine;

namespace Fragsurf.UI
{
    public abstract class EntryElement<T> : MonoBehaviour
    {

        public abstract void Initialize(T data);

        private WaitForEndOfFrame _eof = new WaitForEndOfFrame();

        public EntryElement<T> Create(T data)
        {
            var clone = GameObject.Instantiate(gameObject).GetComponent<EntryElement<T>>();

            clone.Initialize(data);

            return clone;
        }

        public void Append(T data)
        {
            var clone = GameObject.Instantiate(gameObject).GetComponent<EntryElement<T>>();
            var rt = clone.GetComponent<RectTransform>();
            var parentRt = transform.parent.GetComponent<RectTransform>();
            rt.SetParent(parentRt);
            rt.SetSiblingIndex(parentRt.childCount);
            rt.localPosition = Vector3.zero;
            rt.localRotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            gameObject.SetActive(true);
            clone.Initialize(data);
            StartCoroutine(RebuildLayout(parentRt.gameObject));
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
