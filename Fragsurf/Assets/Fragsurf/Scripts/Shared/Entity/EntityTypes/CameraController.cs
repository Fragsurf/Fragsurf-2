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

        public float SensitivityModifier { get; protected set; } = 1f;
        public NetEntity Viewer { get; private set; }
        public Camera Camera { get; private set; }

        public abstract void Update();
        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }

        public void Activate(Camera camera)
        {
            Camera = camera;
            Camera.cullingMask = LayerMask.GetMask("Default", "Ragdoll", "Water") | (1 << Viewer.Game.ScopeLayer);
            Camera.enabled = true;

            if (HideViewer && Viewer.EntityGameObject)
            {
                Viewer.EntityGameObject.SetVisible(false);
            }

            OnActivate();
        }

        public void Deactivate()
        {
            if (HideViewer && Viewer.EntityGameObject)
            {
                Viewer.EntityGameObject.SetVisible(true);
            }

            OnDeactivate();
        }

    }
}
