using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class SurfaceFidelity : FSSharedScript
    {

        protected override void OnGameLoaded()
        {
            GenerateSurfaceColliders();
        }

        private void GenerateSurfaceColliders()
        {
            //foreach(var surfId in FindObjectsOfType<SurfaceTypeIdentifier>())
            //{
            //    var mf = surfId.GetComponent<MeshFilter>();
            //    if (!mf)
            //    {
            //        continue;
            //    }
            //    var obj = new GameObject("[Surface Fidelity]");
            //    obj.transform.SetParent(surfId.transform);
            //    obj.transform.localPosition = Vector3.zero;
            //    obj.transform.localRotation = Quaternion.identity;
            //    obj.transform.localScale = Vector3.one;
            //    obj.AddComponent<SurfaceTypeIdentifier>().SurfaceType = surfId.SurfaceType;
            //    obj.layer = Layers.Fidelity;
            //    var mc = obj.AddComponent<MeshCollider>();
            //    mc.sharedMesh = mf.sharedMesh;
            //    mc.convex = false;
            //}
        }

    }
}

