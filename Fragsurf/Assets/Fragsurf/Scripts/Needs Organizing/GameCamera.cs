using Fragsurf.Client;
using Fragsurf.Maps;
using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Fragsurf
{
    public class GameCamera : SingletonComponent<GameCamera>
    {

        private static GameObject _depthCamera;
        private static Camera _camera;
        public static Camera Camera => GetCamera();

        public void EnableDepthCamera(bool enabled)
        {
            var depthCam = Camera.GetComponentInChildren<PreserveCamera>();
            if (!depthCam)
            {
                return;
            }
            depthCam.gameObject.SetActive(enabled);
        }

        private void Update()
        {
            var dcEnabled = Map.Current is BSPMap;
            if(_depthCamera && _depthCamera.activeSelf != dcEnabled)
            {
                _depthCamera.SetActive(dcEnabled);
            }
        }

        public void Stack(Camera cam)
        {
            var gameCameraURP = Camera.GetComponent<UniversalAdditionalCameraData>();
            if (!gameCameraURP || gameCameraURP.cameraStack.Contains(cam))
            {
                return;
            }
            var camURP = cam.GetComponent<UniversalAdditionalCameraData>();
            if (!camURP)
            {
                return;
            }
            camURP.renderType = CameraRenderType.Overlay;
            gameCameraURP.cameraStack.Add(cam);
        }

        public void Unstack(Camera cam)
        {
            var gameCameraURP = Camera.GetComponent<UniversalAdditionalCameraData>();
            if (!gameCameraURP)
            {
                return;
            }
            gameCameraURP.cameraStack.Remove(cam);
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
            var dc = _camera.GetComponentInChildren<PreserveCamera>(true);
            if (dc)
            {
                _depthCamera = dc.gameObject;
            }
            return _camera;
        }

    }
}

