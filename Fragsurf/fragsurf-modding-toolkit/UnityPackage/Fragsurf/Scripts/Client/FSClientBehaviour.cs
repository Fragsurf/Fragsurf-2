using UnityEngine;

namespace Fragsurf.Client
{
    public class FSClientBehaviour : FSClientScript
    {

        private void Awake()
        {
            GameClient.Instance.AddFSComponent(this);
        }

    }
}

