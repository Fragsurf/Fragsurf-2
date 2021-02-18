using System;
using System.Collections;
using UnityEngine;

namespace HighlightPlus {

    public enum TriggerMode {
        ColliderEventsOnlyOnThisObject = 0,
        RaycastOnThisObjectAndChildren = 1,
        Volume = 2
    }

    public enum RayCastSource {
        MousePosition = 0,
        CameraDirection = 1
    }


    [RequireComponent(typeof(HighlightEffect))]
    [ExecuteInEditMode]
    public class HighlightTrigger : MonoBehaviour {

        [Tooltip("Used to trigger automatic highlighting including children objects.")]
        public TriggerMode triggerMode = TriggerMode.ColliderEventsOnlyOnThisObject;
        public Camera raycastCamera;
        public RayCastSource raycastSource = RayCastSource.MousePosition;
        public float maxDistance;
        public LayerMask volumeLayerMask;

        const int MAX_RAYCAST_HITS = 100;

        [NonSerialized]
        public Collider[] colliders;
        Collider currentCollider;
        static RaycastHit[] hits;

        void OnEnable() {
            Init();
        }


        void Start() {
            if (triggerMode == TriggerMode.RaycastOnThisObjectAndChildren) {
                if (raycastCamera == null) {
                    raycastCamera = HighlightManager.GetCamera();
                    if (raycastCamera == null) {
                        Debug.LogError("Highlight Trigger on " + gameObject.name + ": no camera found!");
                    }
                }
                if (colliders != null && colliders.Length > 0) {
                    hits = new RaycastHit[MAX_RAYCAST_HITS];
                    StartCoroutine(DoRayCast());
                }
            } else {
                Collider collider = GetComponent<Collider>();
                if (collider == null) {
                    if (GetComponent<MeshFilter>() != null) {
                        gameObject.AddComponent<MeshCollider>();
                    }
                }
            }
        }


        IEnumerator DoRayCast() {
            while (triggerMode == TriggerMode.RaycastOnThisObjectAndChildren) {
                if (raycastCamera != null) {
                    Ray ray;
                    if (raycastSource == RayCastSource.MousePosition) {
                        ray = raycastCamera.ScreenPointToRay(Input.mousePosition);
                    } else {
                        ray = new Ray(raycastCamera.transform.position, raycastCamera.transform.forward);
                    }
                    int hitCount;
                    if (maxDistance > 0) {
                        hitCount = Physics.RaycastNonAlloc(ray, hits, maxDistance);
                    } else {
                        hitCount = Physics.RaycastNonAlloc(ray, hits);
                    }
                    bool hit = false;
                    for (int k = 0; k < hitCount; k++) {
                        Collider theCollider = hits[k].collider;
                        for (int c = 0; c < colliders.Length; c++) {
                            if (colliders[c] == theCollider) {
                                hit = true;
                                if (theCollider != currentCollider) {
                                    SwitchCollider(theCollider);
                                    k = hitCount;
                                    break;
                                }
                            }
                        }
                    }
                    if (!hit && currentCollider != null) {
                        SwitchCollider(null);
                    }
                }
                yield return null;
            }
        }


        void SwitchCollider(Collider newCollider) {
            currentCollider = newCollider;
            if (currentCollider != null) {
                Highlight(true);
            } else {
                Highlight(false);
            }
        }


        void OnMouseDown() {
            if (isActiveAndEnabled && triggerMode == TriggerMode.ColliderEventsOnlyOnThisObject) {
                Highlight(true);
            }
        }

        void OnMouseEnter() {
            if (isActiveAndEnabled && triggerMode == TriggerMode.ColliderEventsOnlyOnThisObject) {
                Highlight(true);
            }
        }

        void OnMouseExit() {
            if (isActiveAndEnabled && triggerMode == TriggerMode.ColliderEventsOnlyOnThisObject) {
                Highlight(false);
            }
        }

        void Highlight(bool state) {
            HighlightEffect hb = transform.GetComponent<HighlightEffect>();
            if (hb == null && state) {
                hb = gameObject.AddComponent<HighlightEffect>();
            }
            if (hb != null) {
                hb.SetHighlighted(state);
            }
        }

        public void Init() {
            if (raycastCamera == null) {
                raycastCamera = HighlightManager.GetCamera();
            }
            if (triggerMode == TriggerMode.RaycastOnThisObjectAndChildren) {
                colliders = GetComponentsInChildren<Collider>();
            }
        }

        public void OnTriggerEnter(Collider other) {
            if (triggerMode == TriggerMode.Volume) {
                if ((volumeLayerMask & (1 << other.gameObject.layer)) != 0) {
                    Highlight(true);
                }
            }
        }

        public void OnTriggerExit(Collider other) {
            if (triggerMode == TriggerMode.Volume) {
                if ((volumeLayerMask & (1 << other.gameObject.layer)) != 0) {
                    Highlight(false);
                }
            }
        }
    }

}