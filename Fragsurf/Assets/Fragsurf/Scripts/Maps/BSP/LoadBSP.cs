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
        }

        private void OnDestroy()
        {
            BspToUnity?.Dispose();
        }

    }
}

