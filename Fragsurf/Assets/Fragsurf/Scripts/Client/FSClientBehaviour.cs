using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Client
{
    public class FSClientBehaviour : FSClientScript
    {

        private void Awake()
        {
            var cl = FSGameLoop.GetGameInstance(false);
            if (cl)
            {
                cl.AddFSComponent(this);
            }
        }

    }
}

