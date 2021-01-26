using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fragsurf.Client
{
    [RequireComponent(typeof(Camera))]
    public class PreserveCamera : MonoBehaviour
    {

        [SerializeField]
        public Camera _copyFov;
        private Camera _camera;

        // Use this for initialization
        void Start()
        {
            _camera = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_copyFov != null)
            {
                _camera.fieldOfView = _copyFov.fieldOfView;
            }
        }
    }
}

