using System.Runtime.InteropServices;

namespace UIForia.Rendering {

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialId {

        [FieldOffset(0)] public readonly long id;
        [FieldOffset(0)] internal int instanceId;
        [FieldOffset(4)] internal int baseId;

        public MaterialId(long id) : this() {
            this.id = id;
        }

        public MaterialId(int baseId, int instanceId) {
            this.id = 0;
            this.instanceId = instanceId;
            this.baseId = baseId;
        }

        public static explicit operator long(MaterialId materialId) {
            return materialId.id;
        }

        public static explicit operator MaterialId(long materialId) {
            return new MaterialId(materialId);
        }

    }

}