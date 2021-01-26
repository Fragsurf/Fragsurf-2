using System;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Rendering {
    
    public class UIForiaPropertyBlock {

        public readonly Material material;
        public readonly MaterialPropertyBlock matBlock;
        public readonly Matrix4x4[] transformData;
        public readonly Vector4[] colorData;
        public readonly Vector4[] objectData;
        public readonly Vector4[] miscData;
        public readonly Vector4[] clipUVs;
        public readonly Vector4[] clipRects;
        public readonly Vector4[] cornerData;

        public static readonly int s_TransformDataKey = Shader.PropertyToID("_TransformData");
        public static readonly int s_ColorDataKey = Shader.PropertyToID("_ColorData");
        public static readonly int s_ObjectDataKey = Shader.PropertyToID("_ObjectData");
        public static readonly int s_MiscDataKey = Shader.PropertyToID("_MiscData");
        public static readonly int s_FontDataScales = Shader.PropertyToID("_FontScales");
        public static readonly int s_FontTextureSize = Shader.PropertyToID("_FontTextureSize");
        public static readonly int s_FontTexture = Shader.PropertyToID("_FontTexture");
        public static readonly int s_MainTextureKey = Shader.PropertyToID("_MainTexture");
        public static readonly int s_ClipTextureKey = Shader.PropertyToID("_MaskTexture");
        public static readonly int s_ClipUVKey = Shader.PropertyToID("_ClipUVs");
        public static readonly int s_ClipRectKey = Shader.PropertyToID("_ClipRects");
        public static readonly int s_CornerDataKey = Shader.PropertyToID("_CornerData");
        public static readonly int s_DPIScaleKey = Shader.PropertyToID("_DPIScale");
        public static readonly int s_ClipOffset = Shader.PropertyToID("_ClipOffset");

        public UIForiaPropertyBlock(Material material, int size) {
            this.material = material;
            this.matBlock = new MaterialPropertyBlock();
            this.transformData = new Matrix4x4[size];
            this.colorData = new Vector4[size];
            this.objectData = new Vector4[size];
            this.miscData = new Vector4[size];
            this.clipUVs = new Vector4[size];
            this.clipRects = new Vector4[size];
            this.cornerData = new Vector4[size];
        }

        public void SetData(UIForiaData data, StructList<Matrix4x4> matrices) {
            Array.Copy(matrices.array, 0, transformData, 0, matrices.size);
            Array.Copy(data.colors.array, 0, colorData, 0, data.colors.size);
            Array.Copy(data.objectData0.array, 0, objectData, 0, data.objectData0.size);
            Array.Copy(data.objectData1.array, 0, miscData, 0, data.objectData1.size);
            Array.Copy(data.clipUVs.array, 0, clipUVs, 0, data.clipUVs.size);
            Array.Copy(data.clipRects.array, 0, clipRects, 0, data.clipRects.size);
            Array.Copy(data.cornerData.array, 0, cornerData, 0, data.cornerData.size);

            matBlock.SetMatrixArray(s_TransformDataKey, transformData);
            matBlock.SetVectorArray(s_ColorDataKey, colorData);
            matBlock.SetVectorArray(s_ObjectDataKey, objectData);
            matBlock.SetVectorArray(s_MiscDataKey, miscData);
            matBlock.SetVectorArray(s_ClipUVKey, clipUVs);
            matBlock.SetVectorArray(s_ClipRectKey, clipRects);
            matBlock.SetVectorArray(s_CornerDataKey, cornerData);
            matBlock.SetFloat(s_DPIScaleKey, 1f / Application.dpiScaleFactor);
            
            if (data.mainTexture != null) {
                matBlock.SetTexture(s_MainTextureKey, data.mainTexture);
            }

            if (data.clipTexture != null) {
                matBlock.SetTexture(s_ClipTextureKey, data.clipTexture);
            }
            
            if (data.fontData.fontAsset != null) {
                FontData fontData = data.fontData;
                matBlock.SetVector(s_FontDataScales, new Vector4(fontData.gradientScale, fontData.scaleRatioA, fontData.scaleRatioB, fontData.scaleRatioC));
                matBlock.SetVector(s_FontTextureSize, new Vector4(fontData.textureWidth, fontData.textureHeight, 0, 0));
                matBlock.SetTexture(s_FontTexture, fontData.fontAsset.atlas);
            }
        }

        public void SetSDFData(UIForiaData data, StructList<Matrix4x4> matrices) {
            Array.Copy(matrices.array, 0, transformData, 0, matrices.size);
            Array.Copy(data.colors.array, 0, colorData, 0, data.colors.size);
            Array.Copy(data.objectData0.array, 0, objectData, 0, data.objectData0.size);
            
            matBlock.SetMatrixArray(s_TransformDataKey, transformData);
            matBlock.SetVectorArray(s_ColorDataKey, colorData);
            matBlock.SetVectorArray(s_ObjectDataKey, objectData);
            if (data.mainTexture != null) {
                material.SetTexture(s_MainTextureKey, data.mainTexture);
            }

            if (data.clipTexture != null) {
                material.SetTexture(s_ClipTextureKey, data.clipTexture);
            }
            
            if (data.fontData.fontAsset != null) {
                FontData fontData = data.fontData;
                matBlock.SetVector(s_FontDataScales, new Vector4(fontData.gradientScale, fontData.scaleRatioA, fontData.scaleRatioB, fontData.scaleRatioC));
                matBlock.SetVector(s_FontTextureSize, new Vector4(fontData.textureWidth, fontData.textureHeight, 0, 0));
                matBlock.SetTexture(s_FontTexture, fontData.fontAsset.atlas);
            }
            
        }

    }

}