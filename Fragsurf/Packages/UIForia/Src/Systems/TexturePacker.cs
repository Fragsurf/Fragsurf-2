using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {

    public class TexturePacker {

        private int maxWidth;
        private int maxHeight;
        private int textureWidth;
        private int textureHeight;

        private SimpleRectPacker packer;
        private readonly StructList<TextureData> packedTextureList;
        private readonly StructList<TextureData> toDrawThisFrame;

        internal struct TextureData {

            public Texture texture;
            public SimpleRectPacker.PackedRect region;
            public int lastFrameId;
            public uint updateId;

        }

        public TexturePacker(int textureWidth, int textureHeight, int maxWidth = 256, int maxHeight = 256) {
            this.textureWidth = textureWidth;
            this.textureHeight = textureHeight;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.packedTextureList = new StructList<TextureData>(16);
            this.packer = new SimpleRectPacker(textureWidth, textureHeight, 0);
            this.toDrawThisFrame = new StructList<TextureData>();
        }

        public bool TryPackTexture(Texture texture, out Vector4 uvs) {
            if (texture == null) {
                uvs = default;
                return false;
            }

            int width = texture.width;
            int height = texture.height;

            if (texture.width > maxWidth || texture.height > maxHeight) {
                uvs = default;
                return false;
            }

            int count = packedTextureList.size;
            TextureData[] packedTextures = packedTextureList.array;
            int frameId = Time.frameCount;

            int clearCount = 0;

            for (int i = 0; i < count; i++) {
                ref TextureData packedTexture = ref packedTextures[i];

                if (frameId - packedTexture.lastFrameId >= 2) {
                    clearCount++;
                }

                if (packedTexture.texture == texture) {
                    packedTexture.lastFrameId = frameId;
                    uvs = new Vector4(
                        packedTexture.region.xMin / (float) textureWidth,
                        packedTexture.region.yMin / (float) textureHeight,
                        packedTexture.region.xMax / (float) textureWidth,
                        packedTexture.region.yMax / (float) textureHeight
                    );
                    return true;
                }
            }

            if (packer.TryPackRect(width, height, out SimpleRectPacker.PackedRect region)) {
                TextureData textureData = new TextureData();
                textureData.region = region;
                textureData.lastFrameId = frameId;
                textureData.updateId = texture.updateCount;
                textureData.texture = texture;
                uvs = new Vector4(
                    textureData.region.xMin / (float) textureWidth,
                    textureData.region.yMin / (float) textureHeight,
                    textureData.region.xMax / (float) textureWidth,
                    textureData.region.yMax / (float) textureHeight
                );
                packedTextureList.Add(textureData);
                toDrawThisFrame.Add(textureData);
                return true;
            }
            else if (clearCount > 0) {
                for (int i = 0; i < count; i++) {
                    ref TextureData packedTexture = ref packedTextures[i];

                    if (frameId - packedTexture.lastFrameId != 2) {
                        if (packedTexture.region.xMax - packedTexture.region.xMin <= width && packedTexture.region.yMax - packedTexture.region.yMin <= height) {
                            // remove this texture & replace with new one
                            packedTexture.texture = texture;
                            packedTexture.region.xMax = packedTexture.region.xMin + width;
                            packedTexture.region.xMax = packedTexture.region.yMin + height;
                            packedTexture.lastFrameId = frameId;
                            packedTexture.updateId = texture.updateCount;

                            uvs = new Vector4(
                                packedTexture.region.xMin / (float) textureWidth,
                                packedTexture.region.yMin / (float) textureHeight,
                                packedTexture.region.xMax / (float) textureWidth,
                                packedTexture.region.yMax / (float) textureHeight
                            );
                            toDrawThisFrame.Add(packedTexture);
                            return true;
                        }
                    }
                }

                // last ditch would be to try to re-sort and re-pack the map. not implementing this yet
            }

            uvs = default;
            return false;
        }

        // if we don't draw every frame our RT gets cleared which I don't understand :(
        internal void GetTexturesToRender(StructList<TextureData> textureData) {
            // comment this line back in to make drawing work
            textureData.AddRange(packedTextureList);
//            textureData.AddRange(toDrawThisFrame);
            toDrawThisFrame.QuickClear();
        }

    }

}