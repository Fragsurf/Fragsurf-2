using Fragsurf.Shared.Entity;
using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public class FirstPersonCameraController : CameraController
    {

        public FirstPersonCameraController(NetEntity viewer, Camera camera)
            : base(viewer, camera)
        {

        }

        protected override bool HideViewer => true;

        public override void Update()
        {
            if(Viewer == null || Camera == null)
            {
                Debug.Log(Viewer + ":" + Camera);
                return;
            }

            var targetOrigin = Viewer.EntityGameObject 
                ? Viewer.EntityGameObject.transform.position
                : Viewer.Origin;
            var targetAngles = Viewer.EntityGameObject
                ? Viewer.EntityGameObject.transform.eulerAngles
                : Viewer.Angles;

            if(Viewer is Human player)
            {
                targetOrigin += player.HumanGameObject.EyeOffset;
                if(Viewer == Human.Local)
                {
                    targetAngles = Viewer.Angles;
                }
                //targetAngles += human.Viewpunch;
            }

            Camera.transform.position = targetOrigin;
            Camera.transform.eulerAngles = targetAngles;
        }
    }
}

