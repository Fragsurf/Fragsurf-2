using UnityEngine;

namespace Fragsurf.Movement
{

    //public delegate void CollisionHandler(Collision collision);
    public delegate void ColliderHandler(Collider collision);

    public class CollisionRaiser : MonoBehaviour
    {

        ///// Fields /////

        //public event CollisionHandler CollisionEnter;
        //public event CollisionHandler CollisionStay;
        //public event CollisionHandler CollisionExit;

        public event ColliderHandler TriggerEnter;
        public event ColliderHandler TriggerStay;
        public event ColliderHandler TriggerExit;

        ///// Methods /////

        private void OnDestroy()
        {
            TriggerEnter = null;
            TriggerStay = null;
            TriggerExit = null;
        }

        //private void OnCollisionEnter(Collision collision)
        //{
        //    TriggerEnter?.Invoke(collision.collider);
        //}

        //private void OnCollisionStay(Collision collision)
        //{
        //    TriggerStay?.Invoke(collision.collider);
        //}

        //private void OnCollisionExit(Collision collision)
        //{
        //    TriggerExit?.Invoke(collision.collider);
        //}

        public void RaiseTriggerEnter(Collider other)
        {
            TriggerEnter?.Invoke(other);   
        }

        public void RaiseTriggerStay(Collider other)
        {
            TriggerStay?.Invoke(other);
        }

        public void RaiseTriggerExit(Collider other)
        {
            TriggerExit?.Invoke(other);
        }

    }
}

