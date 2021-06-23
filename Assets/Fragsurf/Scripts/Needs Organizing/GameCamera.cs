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

        private int _fieldOfView = 75;
        private int _weaponFieldOfView = 45;

        private static GameObject _depthCamera;
        private static Camera _camera;
        public static Camera Camera => GetCamera();
        public static AudioListener AudioListener => Camera.GetComponentInChildren<AudioListener>(true);

        [ConVar("cam.weaponfov", "", ConVarFlags.UserSetting)]
        public int WeaponFieldOfView
        {
            get => _weaponFieldOfView;
            set => _weaponFieldOfView = Mathf.Clamp(value, 30, 60);
        }

        [ConVar("cam.fov", "", ConVarFlags.UserSetting)]
        public int FieldOfView
        {
            get => _fieldOfView;
            set 
            {
                _fieldOfView = Mathf.Clamp(value, 60, 110);
                if (_camera)
                {
                    _camera.fieldOfView = _fieldOfView;
                }
            }
        }

        [ConVar("cam.bob", "", ConVarFlags.UserSetting)]
        public bool Bob { get; set; } = true;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.fieldOfView = FieldOfView;

            DevConsole.RegisterObject(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            DevConsole.RemoveAll(this);
        }

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

