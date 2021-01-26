using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public struct OrientedBounds {

        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;
        public Vector2 p3;

        private static readonly Vector2[] s_PolygonHolder = new Vector2[4];
        
        public bool ContainsPoint(Vector2 point) {
            s_PolygonHolder[0] = p0;
            s_PolygonHolder[1] = p1;
            s_PolygonHolder[2] = p2;
            s_PolygonHolder[3] = p3; 
            return PolygonUtil.PointInPolygon(point, s_PolygonHolder, 4);
        }

    }

}