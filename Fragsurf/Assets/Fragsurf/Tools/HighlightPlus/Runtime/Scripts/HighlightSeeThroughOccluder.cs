using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HighlightPlus {

    public struct MeshData {
        public Renderer renderer;
        public int subMeshCount;
    }

    public enum DetectionMethod {
        Stencil = 0,
        RayCast = 1
    }
	[ExecuteInEditMode]
	public class HighlightSeeThroughOccluder : MonoBehaviour {

        public DetectionMethod detectionMethod = DetectionMethod.Stencil;

        [NonSerialized]
        public MeshData[] meshData;

        List<Renderer> rr;

        void OnEnable () {
            if (gameObject.activeInHierarchy) {
                Init();
            }
        }

        void Init() {

            if (detectionMethod == DetectionMethod.RayCast) {
                HighlightEffect.RegisterOccluder(this);
                return;
            }

            if (rr == null) {
                rr = new List<Renderer>();
            } else {
                rr.Clear();
            }
            GetComponentsInChildren<Renderer>(rr);
            int rrCount = rr.Count;
            meshData = new MeshData[rrCount];
            for (int k = 0; k < rrCount; k++) {
                meshData[k].renderer = rr[k];
                meshData[k].subMeshCount = 1;
                if (rr[k] is MeshRenderer) {
                    MeshFilter mf = rr[k].GetComponent<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null) {
                        meshData[k].subMeshCount = mf.sharedMesh.subMeshCount;
                    }
                } else if (rr[k] is SkinnedMeshRenderer) {
                    SkinnedMeshRenderer smr = (SkinnedMeshRenderer)rr[k];
                    meshData[k].subMeshCount = smr.sharedMesh.subMeshCount;
                }
            }
            if (rrCount > 0) {
                HighlightEffect.RegisterOccluder(this);
            }
        }

        void OnDisable () {
            HighlightEffect.UnregisterOccluder(this);
        }
    }

}