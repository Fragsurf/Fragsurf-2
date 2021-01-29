using UnityEngine;

namespace Fragsurf.Movement
{
    public interface ISurfControllable
    {
        MoveType MoveType { get; set; }
        MoveData MoveData { get; }
        BoxCollider Collider { get; }
        GameObject GroundObject { get; set; }
        Quaternion Orientation { get; } 
        Vector3 Forward { get; }
        Vector3 Right { get; }
        Vector3 Up { get; }
        Vector3 StandingExtents { get; }
    }
}
