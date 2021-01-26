using System;
using UnityEngine;

namespace UIForia.Rendering {

    public class ClipPropertyBlock {

        
        public static readonly int s_TransformDataKey = Shader.PropertyToID("_TransformData");
        public static readonly int s_ObjectDataKey = Shader.PropertyToID("_ObjectData");
        public static readonly int s_ColorDataKey = Shader.PropertyToID("_ColorData");
        public static readonly int s_MainTextureKey = Shader.PropertyToID("_MainTexture");
        
        public readonly Material material;
        public readonly MaterialPropertyBlock matBlock;
        public readonly Matrix4x4[] transformData;
        public readonly Vector4[] objectData;
        public readonly Vector4[] colorData;

        public ClipPropertyBlock(Material material, int size) {
            this.material = material;
            this.matBlock = new MaterialPropertyBlock();
            this.transformData = new Matrix4x4[size];
            this.objectData = new Vector4[size];
            this.colorData = new Vector4[size];
        }

        public void SetData(ClipBatch data) {
            
            Array.Copy(data.transforms.array, 0, transformData, 0, data.transforms.size);
            Array.Copy(data.objectData.array, 0, objectData, 0, data.objectData.size);
            Array.Copy(data.colorData.array, 0, colorData, 0, data.colorData.size);

            matBlock.SetMatrixArray(s_TransformDataKey, transformData);
            matBlock.SetVectorArray(s_ColorDataKey, colorData);
            matBlock.SetVectorArray(s_ObjectDataKey, objectData);

            if (data.texture != null) {
                material.SetTexture(s_MainTextureKey, data.texture);
            }
        }

    }

}