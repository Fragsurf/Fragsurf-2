using UnityEngine;

namespace Fragsurf.Shared.Entity
{
    public abstract class CameraController
    {

        public CameraController(NetEntity viewer)
        {
            Viewer = viewer;
        }

        protected abstract bool HideViewer { get; }

        public NetEntity Viewer { get; private set; }
        public Camera Camera { get; private set; }

        public abstract void Update();

        public void Activate(Camera camera)
        {
            Camera = camera;
            Camera.cullingMask = LayerMask.GetMask("Default", "Ragdoll", "Water") | (1 << Viewer.Game.ScopeLayer);
            Camera.enabled = true;

            if (HideViewer && Viewer.EntityGameObject)
            {
                Viewer.EntityGameObject.SetVisible(false);
            }
        }

        public void Deactivate()
        {
            if (HideViewer && Viewer.EntityGameObject)
            {
                Viewer.EntityGameObject.SetVisible(true);
            }
        }

    }
}
