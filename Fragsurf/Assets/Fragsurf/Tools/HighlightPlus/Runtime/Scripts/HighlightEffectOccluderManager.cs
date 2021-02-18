using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightPlus {


    public partial class HighlightEffect : MonoBehaviour {

        static List<HighlightSeeThroughOccluder> occluders = new List<HighlightSeeThroughOccluder>();
        static Dictionary<Camera, int> occludersFrameCount = new Dictionary<Camera, int>();
        static CommandBuffer cbOccluder;
        static Material fxMatOccluder;
        static RaycastHit[] hits;

        bool cancelSeeThroughThisFrame;
        public static void RegisterOccluder(HighlightSeeThroughOccluder occluder) {
            if (!occluders.Contains(occluder)) {
                occluders.Add(occluder);
            }
        }

        public static void UnregisterOccluder(HighlightSeeThroughOccluder occluder) {
            if (occluders.Contains(occluder)) {
                occluders.Remove(occluder);
            }
        }

        public void RenderOccluders(CommandBuffer cb, Camera cam) {

            int occludersCount = occluders.Count;
            if (occludersCount == 0 || rmsCount == 0) return;

            Vector3 camPos = cam.transform.position;

            bool useRayCastCheck = false;
            // Check if raycast method is needed
            for (int k = 0; k < occludersCount; k++) {
                HighlightSeeThroughOccluder occluder = occluders[k];
                if (occluder == null || !occluder.isActiveAndEnabled) continue;
                if (occluder.detectionMethod == DetectionMethod.RayCast) {
                    useRayCastCheck = true;
                    break;
                }
            }
            if (useRayCastCheck) {
                // Compute bounds
                Bounds bounds = new Bounds();
                for (int r = 0; r < rms.Length; r++) {
                    if (rms[r].renderer != null) {
                        if (bounds.size.x == 0) {
                            bounds = rms[r].renderer.bounds;
                        } else {
                            bounds.Encapsulate(rms[r].renderer.bounds);
                        }
                    }
                }
                Vector3 pos = bounds.center;
                Vector3 offset = pos - camPos;
                float maxDistance = Vector3.Distance(pos, camPos);
                if (hits == null || hits.Length == 0) {
                    hits = new RaycastHit[64];
                }
                int hitCount = Physics.BoxCastNonAlloc(pos - offset, bounds.extents * 0.9f, offset.normalized, hits, Quaternion.identity, maxDistance);
                for (int k = 0; k < hitCount; k++) {
                    for (int j = 0; j < occludersCount; j++) {
                        if (hits[k].collider.transform == occluders[j].transform) {
                            cancelSeeThroughThisFrame = true;
                            return;
                        }
                    }
                }
            }

            int lastFrameCount;
            occludersFrameCount.TryGetValue(cam, out lastFrameCount);
            int currentFrameCount = Time.frameCount;
            if (currentFrameCount == lastFrameCount) return;
            occludersFrameCount[cam] = currentFrameCount;

            if (cbOccluder == null) {
                cbOccluder = new CommandBuffer();
                cbOccluder.name = "Occluder";
            }

            if (fxMatOccluder == null) {
                InitMaterial(ref fxMatOccluder, "HighlightPlus/Geometry/SeeThroughOccluder");
                if (fxMatOccluder == null) return;
            }


            for (int k = 0; k < occludersCount; k++) {
                HighlightSeeThroughOccluder occluder = occluders[k];
                if (occluder == null || !occluder.isActiveAndEnabled) continue;
                if (occluder.detectionMethod == DetectionMethod.Stencil) {
                    if (occluder.meshData == null || occluder.meshData.Length == 0) continue;
                    // Per renderer
                    for (int m = 0; m < occluder.meshData.Length; m++) {
                        // Per submesh
                        Renderer renderer = occluder.meshData[m].renderer;
                        if (renderer.isVisible) {
                            for (int s = 0; s < occluder.meshData[m].subMeshCount; s++) {
                                cb.DrawRenderer(renderer, fxMatOccluder, s);
                            }
                        }
                    }
                }
            }
        }

    }
}
