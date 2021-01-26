using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.U2D;

namespace UIForia.Rendering {

    public static class VertigoUtil {

        [StructLayout(LayoutKind.Explicit)]
        public struct Union {

            [FieldOffset(0)] public float asFloat;
            [FieldOffset(0)] public int asInt;

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct UnionByte {

            [FieldOffset(0)] public float asFloat;
            [FieldOffset(0)] public byte b0;
            [FieldOffset(1)] public byte b1;
            [FieldOffset(2)] public byte b2;
            [FieldOffset(3)] public byte b3;

        }

        public static float BytesToFloat(byte b0, byte b1, byte b2, byte b3) {
            int color = b0 | b1 << 8 | b2 << 16 | b3 << 24;
            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = color;
            // as float get int back -> broken
            return color2Float.asFloat;
        }

        public static float ColorToFloat(Color c) {
            float r = c.r;
            float g = c.g;
            float b = c.b;
            float a = c.a;

            if (r < 0) r = 0;
            if (r > 1) r = 1;
            if (g < 0) g = 0;
            if (g > 1) g = 1;
            if (b < 0) b = 0;
            if (b > 1) b = 1;
            if (a < 0) a = 0;
            if (a > 1) a = 1;

            int color = (int) (r * 255) | (int) (g * 255) << 8 | (int) (b * 255) << 16 | (int) (a * 255) << 24;

            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = color;

            return color2Float.asFloat;
        }

        public static float ColorToFloat(Color32 c) {
            int color = c.r | c.g << 8 | c.b << 16 | c.a << 24;

            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = color;

            return color2Float.asFloat;
        }

        internal struct SpriteData {

            public string name;
            public Rect uvBounds;
            public Vector2[] uvs;
            public Vector2[] vertices;
            public ushort[] triangles;
            public Texture2D texture;

        }

        private static Dictionary<int, SpriteData> s_SpriteDataMap;
        private static Dictionary<int, SpriteData[]> s_SpriteAtlasMap;

        internal static SpriteData GetSpriteData(SpriteAtlas atlas, string spriteName) {
            if (s_SpriteAtlasMap == null) {
                s_SpriteAtlasMap = new Dictionary<int, SpriteData[]>();
            }

            if (s_SpriteAtlasMap.TryGetValue(atlas.GetInstanceID(), out SpriteData[] list)) {
                for (int i = 0; i < list.Length; i++) {
                    if (list[i].name == spriteName) {
                        return list[i];
                    }
                }

                return default;
            }

            SpriteData[] spriteDataList = new SpriteData[atlas.spriteCount];
            Sprite[] spriteList = new Sprite[atlas.spriteCount];
            atlas.GetSprites(spriteList);
            for (int i = 0; i < spriteDataList.Length; i++) {
                Sprite sprite = spriteList[i];


                Vector2[] uvs = sprite.uv;

                float minX = float.MaxValue;
                float minY = float.MaxValue;
                float maxX = float.MinValue;
                float maxY = float.MinValue;

                for (int j = 0; j < uvs.Length; j++) {
                    if (uvs[j].x < minX) minX = uvs[j].x;
                    if (uvs[j].y < minY) minY = uvs[j].y;
                    if (uvs[j].x > maxX) maxX = uvs[j].x;
                    if (uvs[j].y > maxY) maxY = uvs[j].y;
                }

                Rect uvBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
                spriteDataList[i].name = sprite.name.Replace("(Clone)", "");
                spriteDataList[i].uvBounds = uvBounds;
                spriteDataList[i].vertices = sprite.vertices;
                spriteDataList[i].uvs = uvs;
                spriteDataList[i].triangles = sprite.triangles;
                spriteDataList[i].texture = sprite.texture;
            }

            s_SpriteAtlasMap.Add(atlas.GetInstanceID(), spriteDataList);
            for (int i = 0; i < spriteDataList.Length; i++) {
                if (spriteDataList[i].name == spriteName) {
                    spriteDataList[i].texture.filterMode = FilterMode.Bilinear;
                    spriteDataList[i].texture.wrapMode = TextureWrapMode.Repeat;
                    spriteDataList[i].texture.Apply();
                    return spriteDataList[i];
                }
            }

            return default;
        }

        internal static SpriteData GetSpriteData(Sprite sprite) {
            if (s_SpriteDataMap == null) {
                s_SpriteDataMap = new Dictionary<int, SpriteData>();
            }

            SpriteData retn = new SpriteData();
            if (s_SpriteDataMap.TryGetValue(sprite.GetInstanceID(), out retn)) {
                return retn;
            }

            Vector2[] uvs = sprite.uv;

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for (int i = 0; i < uvs.Length; i++) {
                if (uvs[i].x < minX) minX = uvs[i].x;
                if (uvs[i].y < minY) minY = uvs[i].y;
                if (uvs[i].x > maxX) maxX = uvs[i].x;
                if (uvs[i].y > maxY) maxY = uvs[i].y;
            }

            Rect uvBounds = new Rect(minX, minY, maxX - minX, maxY - minY);
            retn.uvBounds = uvBounds;
            retn.vertices = sprite.vertices;
            retn.uvs = uvs;
            retn.triangles = sprite.triangles;
            s_SpriteDataMap.Add(sprite.GetInstanceID(), retn);
            return retn;
        }

        public static Rect RectFromSpriteUV(Sprite sprite) {
            if (ReferenceEquals(sprite, null)) return default;
            return GetSpriteData(sprite).uvBounds;
        }

        public static float Vector2ToFloat(Vector2 vec) {
            float x = vec.x;
            float y = vec.y;
            x = x < 0 ? 0 : 1 < x ? 1 : x;
            y = y < 0 ? 0 : 1 < y ? 1 : y;
            const int PRECISION = (1 << 12) - 1;
            return (Mathf.FloorToInt(y * PRECISION) << 12) + Mathf.FloorToInt(x * PRECISION);
        }

        public static float PackSizeVector(Vector2 size) {
            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = ((int) (size.x * 10) << 16) | ((int) (size.y * 10) & 0xffff);
            return color2Float.asFloat;
        }

        public static float PackSizeVector(float x, float y) {
            Union color2Float;
            color2Float.asFloat = 0;
            color2Float.asInt = ((int) (x * 10) << 16) | ((int) (y * 10) & 0xffff);
            return color2Float.asFloat;
        }

    }

}