using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fragsurf.Utility
{
    public static class UnityExtensions
    {

        private static Dictionary<Color, string> _hashRGBACache = new Dictionary<Color, string>();
        public static string HashRGBA(this Color c)
        {
            if (!_hashRGBACache.ContainsKey(c))
            {
                _hashRGBACache[c] = "#" + ColorUtility.ToHtmlStringRGBA(c);
            }
            return _hashRGBACache[c];
        }

        // TRANSFORM/GAMEOBJECT

        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            var result = gameObject.GetComponent<T>();
            if (result == null)
            {
                result = gameObject.AddComponent<T>();
            }
            return result;
        }

        public static T GetOrAddComponent<T>(this Transform tr)
        where T : Component
        {
            return GetOrAddComponent<T>(tr.gameObject);
        }

        public static void DestroyComponentsInChildren<T>(this GameObject gameObject)
            where T : Component
        {
            foreach (var c in gameObject.GetComponentsInChildren<T>())
            {
                GameObject.Destroy(c);
            }
        }

        public static void SetChildrenCollidersToTrigger(this GameObject gameObject, bool isTrigger = true)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = isTrigger;
            }
        }

        public static void SetCollidersEnabled(this GameObject gameObject, bool enabled = true)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = enabled;
            }
        }

        public static void SetChildrenCollidersToConvexTrigger(this GameObject gameObject, bool isTrigger = true)
        {
            foreach (var collider in gameObject.GetComponentsInChildren<MeshCollider>())
            {
                collider.convex = true;
            }
            gameObject.SetChildrenCollidersToTrigger(isTrigger);
        }

        public static void SetLayerRecursively(this Transform transform, int layer, int leaveAlone = -1)
        {
            if (transform.gameObject.layer != leaveAlone)
            {
                transform.gameObject.layer = layer;
            }

            foreach (Transform t in transform)
            {
                if (t.gameObject.layer != leaveAlone)
                {
                    t.gameObject.layer = layer;
                }
                if (t.childCount > 0)
                {
                    SetLayerRecursively(t, layer, leaveAlone);
                }
            }
        }
        public static void RebuildLayout(this GameObject root)
        {
            var rt = root.GetComponent<RectTransform>();
            if (rt)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
                //LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }
        }

        public static Vector3 ClampToParent(this RectTransform rt, RectTransform parent)
        {
            var op = rt.anchoredPosition;
            var pos = op;

            Vector3 minPosition = parent.rect.min - rt.rect.min;
            Vector3 maxPosition = parent.rect.max - rt.rect.max;

            pos.x = Mathf.Clamp(rt.anchoredPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(rt.anchoredPosition.y, minPosition.y, maxPosition.y);

            rt.anchoredPosition = pos;

            return pos - op;
        }

        public static Rect GetScreenSpaceRect(this RectTransform transform)
        {
            Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
            Rect rect = new Rect(transform.position.x, Screen.height - transform.position.y, size.x, size.y);
            rect.x -= (transform.pivot.x * size.x);
            rect.y -= ((1.0f - transform.pivot.y) * size.y);
            return rect;
        }

        public static bool MouseIsHovering(this RectTransform tr)
        {
            Vector2 localMousePosition = tr.InverseTransformPoint(Input.mousePosition);
            if (tr.rect.Contains(localMousePosition))
            {
                return true;
            }
            return false;
        }



        // VECTOR/QUATERNION

        public static Vector3 VectorMa(Vector3 start, float scale, Vector3 direction)
        {
            return start + direction * scale;
        }
        public static bool IsNaN(this Vector3 vec)
        {
            return float.IsNaN(vec.x) || float.IsNaN(vec.y) || float.IsNaN(vec.z);
        }
        public static bool IsNaN(this Quaternion q)
        {
            return float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w);
        }
        public static Vector3 StringToVector3(string sVector)
        {
            // Remove the parentheses
            if (sVector.StartsWith("(") && sVector.EndsWith(")"))
            {
                sVector = sVector.Substring(1, sVector.Length - 2);
            }

            // split the items
            string[] sArray = sVector.Split(',');

            Vector3 result = new Vector3(
                float.Parse(sArray[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(sArray[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture),
                float.Parse(sArray[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture));

            return result;
        }
    }
}

