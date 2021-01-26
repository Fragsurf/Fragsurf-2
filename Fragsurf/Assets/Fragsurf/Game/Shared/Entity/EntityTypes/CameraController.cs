using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public abstract class CameraController
    {

        public CameraController(NetEntity viewer, Camera camera)
        {
            Viewer = viewer;
            Camera = camera;
        }

        private NetEntity _viewer;
        private Camera _camera;
        protected abstract bool HideViewer { get; }

        public NetEntity Viewer
        {
            get => _viewer;
            set => SetViewer(value);
        }

        public Camera Camera
        {
            get => _camera;
            set => SetCamera(value);
        }

        public abstract void Update();

        private void SetViewer(NetEntity newViewer)
        {
            if (HideViewer)
            {
                if (_viewer != null && _viewer.EntityGameObject)
                {
                    _viewer.EntityGameObject.SetVisible(true);
                }
                if (newViewer != null && newViewer.EntityGameObject)
                {
                    newViewer.EntityGameObject.SetVisible(false);
                }
            }
            else
            {
                newViewer.EntityGameObject.SetVisible(true);
            }

            _viewer = newViewer;

            if(Camera != null && _viewer != null)
            {
                Camera.cullingMask |= 1 << Viewer.Game.ScopeLayer;
            }
        }

        private void SetCamera(Camera cam)
        {
            if (!cam)
            {
                _camera = null;
                return;
            }

            _camera = cam;
            _camera.cullingMask = LayerMask.GetMask("Default", "Ragdoll");
            _camera.enabled = true;

            if(Viewer != null)
            {
                _camera.cullingMask |= 1 << Viewer.Game.ScopeLayer;
            }

            foreach(var camera in Camera.allCameras)
            {
                if(camera != cam)
                {
                    camera.enabled = false;
                }
            }
        }

    }
}
