using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Fragsurf.Shared
{
    public static class Layers
    {
        public static int Default;
        public static int TransparentFX;
        public static int IgnoreRaycast;
        public static int PostProcessing;
        public static int Water;
        public static int UI;
        public static int Host;
        public static int Client;
        public static int Viewmodel;
        public static int Ragdoll;
        public static int Invisible;
        public static int Fidelity;

        static Layers()
        {
            for(int i = 0; i < 32; i++)
            {
                var layerName = string.Concat(LayerMask.LayerToName(i).Where(i => !new[] { '.', ' ', '-' }.Contains(i)));
                if (!string.IsNullOrEmpty(layerName))
                {
                    var typeInfo = typeof(Layers).GetField(layerName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Static);
                    if(typeInfo == null)
                    {
                        Debug.LogError("Script is missing layer: " + layerName);
                        continue;
                    }
                    typeInfo.SetValue(null, i);
                }
            }
        }

    }
}
