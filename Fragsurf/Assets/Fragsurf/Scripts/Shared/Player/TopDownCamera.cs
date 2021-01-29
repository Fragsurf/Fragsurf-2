using UnityEngine;
using Fragsurf.Shared.Entity;

namespace Fragsurf.Shared.Player
{
    public class TopDownCamera : CameraController
    {

        public TopDownCamera(NetEntity viewer, Camera camera)
            : base(viewer, camera)
        {

        }

        protected override bool HideViewer => false;

        public override void Update()
        {
            if (Viewer == null || Camera == null)
            {
                Debug.Log(Viewer + ":" + Camera);
                return;
            }

            var targetOrigin = Viewer.EntityGameObject
                ? Viewer.EntityGameObject.transform.position
                : Viewer.Origin;

            Camera.transform.position = targetOrigin + new Vector3(0, 7, -3.25f);
            Camera.transform.eulerAngles = new Vector3(65, 0, 0);
            //Camera.transform.RotateAround(targetOrigin, Vector3.up, targetAngles.y);
            //Camera.transform.RotateAround(targetOrigin, Vector3.forward, targetAngles.z);
        }
    }
}

