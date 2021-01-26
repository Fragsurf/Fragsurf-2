using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class UIForiaData {

        public FontData fontData;
        public StructList<Vector4> colors = new StructList<Vector4>();
        public StructList<Vector4> objectData0 = new StructList<Vector4>();
        public StructList<Vector4> objectData1 = new StructList<Vector4>();
        public StructList<Vector4> clipUVs = new StructList<Vector4>();
        public StructList<Vector4> clipRects = new StructList<Vector4>();
        public StructList<Vector4> cornerData = new StructList<Vector4>();
        
        public Texture mainTexture;
        public Texture clipTexture;
        internal bool isActive;

        public UIForiaData() {
            isActive = true;
        }
        
        public void Clear() {
            mainTexture = null;
            clipTexture = null;
            fontData = default;
            colors.size = 0;
            objectData0.size = 0;
            objectData1.size = 0;
            clipUVs.size = 0;
            clipRects.size = 0;
            cornerData.size = 0;
        }

        public static LightList<UIForiaData> s_Pool = new LightList<UIForiaData>();

        public static void Release(ref UIForiaData data) {
            if (data == null || !data.isActive) return;
            data.isActive = false;
            data.Clear();
            s_Pool.Add(data);
            data = null;
        }

        public static UIForiaData Get() {
            UIForiaData retn = null;
            if (s_Pool.size > 0) {
                retn = s_Pool.RemoveLast();
            }
            else {
                retn = new UIForiaData();
            }

            retn.isActive = true;
            return retn;
        }


    }
    
}