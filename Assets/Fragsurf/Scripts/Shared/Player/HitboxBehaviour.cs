using UnityEngine;

namespace Fragsurf.Shared.Player
{
    public enum HitboxArea
    {
        None,
        Head,
        Neck,
        UpperChest,
        LowerChest,
        UpperLeg,
        LowerLeg,
        UpperArm,
        LowerArm,
        Hand,
        Foot
    }
    public class HitboxBehaviour : MonoBehaviour
    {
        public HitboxArea Area;
        public int EntityId;

        private void Start()
        {
            if(TryGetComponent(out Collider collider))
            {
                collider.isTrigger = true;
            }
        }
    }
}
