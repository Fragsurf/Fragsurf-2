using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Fragsurf
{
    public class GameCamera : SingletonComponent<GameCamera>
    {

        private static Camera _camera;
        public static Camera Camera => GetCamera();

        public void Stack(Camera cam)
        {
            var urpData = Camera.GetComponent<UniversalAdditionalCameraData>();
            if (!urpData || urpData.cameraStack.Contains(cam))
            {
                return;
            }
            urpData.cameraStack.Add(cam);
        }

        public void Unstack(Camera cam)
        {
            var urpData = Camera.GetComponent<UniversalAdditionalCameraData>();
            if (!urpData)
            {
                return;
            }
            urpData.cameraStack.Remove(cam);
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private static Camera GetCamera()
        {
            if (!_camera)
            {
                GameObject.Instantiate(Resources.Load<GameObject>("GameCamera"));
            }
            return _camera;
        }

    }
}

