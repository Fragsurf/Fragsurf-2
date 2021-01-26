using UIForia.Util;
using UnityEngine;

namespace Vertigo {

    public struct GeometryData {

        public StructList<Vector3> positionList;
        public StructList<Vector4> texCoordList0;
        public StructList<Vector4> texCoordList1;
        public StructList<int> triangleList;

        public static GeometryData Create() {
            GeometryData retn = new GeometryData();
            retn.positionList = new StructList<Vector3>();
            retn.texCoordList0 = new StructList<Vector4>();
            retn.texCoordList1 = new StructList<Vector4>();
            retn.triangleList = new StructList<int>();
            return retn;
        }

        public void Clear() {
            positionList.size = 0;
            texCoordList0.size = 0;
            texCoordList1.size = 0;
            triangleList.size = 0;
        }

    }

}