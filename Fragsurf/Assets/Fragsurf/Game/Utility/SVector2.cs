using System;
using UnityEngine;

namespace Fragsurf.Utility
{
    [Serializable]
    public struct SVector2
    {

        public SVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float x;
        public float y;

        public static implicit operator Vector2(SVector2 svector)
        {
            return new Vector2(svector.x, svector.y);
        }

        public static implicit operator SVector2(Vector2 vector)
        {
            return new SVector2(vector.x, vector.y);
        }

        public static implicit operator Vector3(SVector2 svector)
        {
            return new Vector3(svector.x, svector.y, 0);
        }

        public static implicit operator SVector2(Vector3 vector)
        {
            return new SVector2(vector.x, vector.y);
        }

    }
}

