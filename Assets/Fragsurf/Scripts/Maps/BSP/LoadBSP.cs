using System.Collections;
using System.IO;
using UnityEngine;

namespace Fragsurf.BSP
{
    public class LoadBSP : MonoBehaviour
    {
        [Header("Helpers")]
        public string MapDirectory;
        public bool LoadOnStart;

        public ReflectionProbe ReflectionProbe;
        public BspToUnityOptions Options;

        public BspToUnity BspToUnity { get; private set; }

        private void Start()
        {
            if (LoadOnStart)
            {
                Load();
            }
        }

        public void Load()
        {
            if (!string.IsNullOrEmpty(MapDirectory))
            {
                Options.FilePath = Path.Combine(MapDirectory, Options.FilePath);
            }

            BspToUnity = new BspToUnity(Options);
            var bspRoot = BspToUnity.Generate();
            bspRoot.transform.SetParent(transform, true);

            if (ReflectionProbe)
            {
                ReflectionProbe.RenderProbe();
            }

            StartCoroutine(MakeCollidersConvex());
        }

        // Setting a ton of MeshColliders to convex in one frame takes forever
        // In some cases 15+ seconds.  So here just do it in a coroutine so load time isn't stupid slow
        private IEnumerator MakeCollidersConvex()
        {
            var ct = 0;
            foreach(var mc in BspToUnity.CollidersToConvex)
            {
                mc.convex = true;
                ct++;
                if(ct >= 10)
                {
                    ct = 0;
                    yield return 0;
                }
            }
        }

        private void OnDestroy()
        {
            BspToUnity?.Dispose();
        }

    }
}

