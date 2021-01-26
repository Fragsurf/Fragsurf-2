using System;
using System.Runtime.InteropServices;
using UIForia.Rendering;
using UnityEditor;
using UnityEngine;

namespace UIForia {

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialProperty {

        // this must align with StyleProperty.object
        public MaterialPropertyValue2 value;
        public MaterialPropertyType propertyType;
        public MaterialId materialId;
        public int propertyId;

    }
    
    [Serializable]
    public struct CurveReference {

        public string name;
        public AnimationCurve animationCurve;

    }

    [Serializable]
    public struct MaterialReference {

        public string name;
        public Material material;
        public bool isShared;

    }

    public enum MaterialPropertyType {

        Color,
        Float,
        Vector,
        Range,
        Texture,

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialPropertyValue2 {
        
        [FieldOffset(0)] public Texture texture;
        [FieldOffset(8)] public float floatValue;
        [FieldOffset(8)] public Color colorValue;
        [FieldOffset(8)] public Vector4 vectorValue;

    }

    public struct MaterialValueOverride {

        public MaterialPropertyValue2 value;
        public MaterialPropertyType propertyType;
        public int propertyId;

    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MaterialPropertyValue {
        
        [FieldOffset(0)] public Texture texture;
        [FieldOffset(8)] public int shaderPropertyId;
        [FieldOffset(12)] public MaterialPropertyType propertyType;
        [FieldOffset(16)] public float floatValue;
        [FieldOffset(16)] public Color colorValue;
        [FieldOffset(16)] public Vector4 vectorValue;

    }

    public struct MaterialPropertyInfo {

        public int propertyId;
        public string propertyName;
        public MaterialPropertyType propertyType;

    }

    public struct MaterialInfo {

        public string materialName;
        public string[] keywords;
        public Material material;
        public RangeInt propertyRange;

    }

    public class UIForiaAssets : MonoBehaviour {

        public MaterialReference[] materialReferences;
        public CurveReference[] animationCurveReferences;

    }

}