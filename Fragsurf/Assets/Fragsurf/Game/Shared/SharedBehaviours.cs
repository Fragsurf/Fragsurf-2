using UnityEngine;
using Fragsurf.Shared.Maps;

namespace Fragsurf.Shared
{
    public static class SharedBehaviours
    {
        public static string GamemodeOverride;
        public static string MapOverride;
        private static bool _initialized;

        public static GameObject ComponentObj { get; private set; }

        public static void Begin()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
        }

        public static GameObject CreateContainer(string name)
        {
            return new GameObject($"CONTAINER[{name}]");
        }

        public static T GetOrCreateComponent<T>()
            where T : MonoBehaviour
        {
            var existingComponent = GameObject.FindObjectOfType<T>();

            if (existingComponent != null)
                return existingComponent;

            if(ComponentObj == null)
            {
                ComponentObj = new GameObject { name = "[Component Object]" };
                GameObject.DontDestroyOnLoad(ComponentObj);
            }

            return ComponentObj.AddComponent<T>();
        }

    }
}
