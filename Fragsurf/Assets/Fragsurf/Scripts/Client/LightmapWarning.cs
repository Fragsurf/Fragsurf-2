using Fragsurf.Shared;
using UnityEngine;

namespace Fragsurf.Client
{
    [Inject(InjectRealm.Client)]
    public class LightmapWarning : FSSharedScript
    {

        private GUIStyle _style;

        protected override void _Initialize()
        {
            enabled = false;
            _style = new GUIStyle()
            {
                alignment = TextAnchor.UpperCenter,
                richText = true,
                fontSize = 15
            };
        }

        protected override void OnGameLoaded()
        {
            enabled = LightmapSettings.lightmaps == null || LightmapSettings.lightmaps.Length == 0;
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(0, 16, Screen.width, 32), "<color=black><size=16>LIGHTMAP NOT BUILT, NOT REPRESENTATIVE OF FINAL QUALITY!</size></color>", _style);
            GUI.Label(new Rect(0, 15, Screen.width, 32), "<color=red><size=16>LIGHTMAP NOT BUILT, NOT REPRESENTATIVE OF FINAL QUALITY!</size></color>", _style);
        }

    }
}
