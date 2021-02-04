//using RealtimeCSG;
//using RealtimeCSG.Components;
using UnityEngine;

namespace Fragsurf.FSM.Actors
{
    [SelectionBase]
    [System.Serializable]
    public class FSMActor : MonoBehaviour
    {

        [Header("Actor Options")]
        public string ActorName;

        public virtual void Tick() { }
        protected virtual void _Update() { }
        protected virtual void _Awake() { }
        protected virtual void _Start() { }
        protected virtual void _OnDestroy() { }
        protected virtual void _OnDrawGizmos() { }

        private void Awake()
        {
            _Awake();
        }

        private void Start()
        {
            if (ActorName == "Ladder")
            {
                gameObject.tag = "Ladder";
            }
            EnableRenderers(false);
            _Start();
        }

        private void Update()
        {
            _Update();
        }

        private void OnDrawGizmos()
        {
            var center = transform.position;
            //var brush = GetComponentInChildren<CSGBrush>();
            //if(brush)
            //{
            //    center = BoundsUtilities.GetCenter(brush);
            //}
            DebugDraw.WorldLabel($"{GetType().Name}\n<color=#00f742>{ActorName}</color>", center, 12, Color.yellow, 30f);
            _OnDrawGizmos();
        }

        public void EnableRenderers(bool enabled)
        {
            foreach(var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = enabled;
            }
        }

    }
}

