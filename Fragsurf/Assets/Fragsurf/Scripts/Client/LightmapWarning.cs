using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class LightmapWarning : FSSharedScript
    {

        protected override void _Initialize()
        {
            enabled = false;
        }

        protected override void OnGameLoaded()
        {
            enabled = LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(17, 17, 900, 32), "<color=black><size=16>LIGHTMAP NOT BUILT, NOT REPRESENTATIVE OF FINAL QUALITY!</size></color>");
            GUI.Label(new Rect(16, 16, 900, 32), "<color=red><size=16>LIGHTMAP NOT BUILT, NOT REPRESENTATIVE OF FINAL QUALITY!</size></color>");
        }

    }
}
