using Fragsurf.Shared;
using Fragsurf.Shared.Entity;
using Fragsurf.Utility;
using UnityEngine;

namespace Fragsurf
{
    public class GameCamera : SingletonComponent<GameCamera>
    {

        private static Camera _camera;
        public static Camera Camera => GetCamera();

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private static Camera GetCamera()
        {
            if (!_camera)
            {
                GameObject.Instantiate(Resources.Load<GameObject>("GameCamera"));
            }
            return _camera;
        }

    }
}

