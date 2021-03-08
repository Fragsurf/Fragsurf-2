using UnityEngine;

namespace LightingTools.LightProbesVolumes
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(BoxCollider))]
    public class LightProbesVolumeSettings : MonoBehaviour
    {
        public float horizontalSpacing = 2.0f;
        public float verticalSpacing = 2.0f;
        public float offsetFromFloor = 0.5f;
        public int numberOfLayers = 2;
        public bool fillVolume = false;
        public bool followFloor = true;
        public bool discardInsideGeometry;
        public bool drawDebug = false;

        private void OnEnable()
        {
            var boxCollider = GetComponent<BoxCollider>();
            if (boxCollider)
            {
                boxCollider.enabled = false;
            }
        }

    }
}