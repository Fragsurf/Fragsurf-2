using SourceUtils.ValveBsp.Entities;
using UnityEngine;

namespace Fragsurf.BSP
{
    [EntityComponent("light_environment")]
    public class BspLightEnvironment : GenericBspEntityMonoBehaviour<LightEnvironment>
    {
        private void Start()
        {
            if (BspToUnity.Options.Sun == null)
            {
                return;
            }
            BspToUnity.Options.Sun.transform.localPosition = Vector3.zero;
            BspToUnity.Options.Sun.transform.rotation = Quaternion.Euler(new Vector3(-Entity.Pitch, -Entity.Angles.Y + 90, -Entity.Angles.X));
            BspToUnity.Options.Sun.color = new Color32(Entity.Light.R, Entity.Light.G, Entity.Light.B, Entity.Light.A);
            //BspToUnity.Options.Sun.lightmapBakeType = LightmapBakeType.Mixed;
            //BspToUnity.Options.Sun.mix = LightmapBakeType.Mixed;

            //var prop = BspToUnity.Options.Sun.GetType().GetField("isBaked", System.Reflection.BindingFlags.NonPublic
            //    | System.Reflection.BindingFlags.Instance);
            //prop.SetValue(BspToUnity.Options.Sun, true);
        }
    }
}