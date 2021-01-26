using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace UIForia.Compilers {

    public static class MaterialAssetBuilder {

#if UNITY_EDITOR
        public static MaterialDatabase BuildMaterialDatabase(MaterialReference[] materialReferences) {
            if (true || materialReferences == null) {
                return new MaterialDatabase(new MaterialInfo[0], new MaterialPropertyInfo[0]);
            }

            MaterialInfo[] materialInfos = new MaterialInfo[materialReferences.Length];

            int totalPropertyCount = 0;

            for (int i = 0; i < materialReferences.Length; i++) {
                Shader shader = materialReferences[i].material.shader;
                int count = ShaderUtil.GetPropertyCount(shader);

                materialInfos[i] = new MaterialInfo() {
                    material = materialReferences[i].material,
                    materialName = materialReferences[i].name,
                    propertyRange = new RangeInt(totalPropertyCount, count)
                };

                totalPropertyCount += count;
            }

            MaterialPropertyInfo[] propertyInfos = new MaterialPropertyInfo[totalPropertyCount];

            int idx = 0;

            for (int i = 0; i < materialReferences.Length; i++) {

                Material material = materialReferences[i].material;
                Shader shader = material.shader;
                int count = materialInfos[i].propertyRange.length;

                for (int j = 0; j < count; j++) {

                    ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, j);
                    string propertyName = ShaderUtil.GetPropertyName(shader, j);
                    // todo -- use string intern system to handle names instead
                    
                    MaterialPropertyValue materialValue = new MaterialPropertyValue {
                        shaderPropertyId = Shader.PropertyToID(propertyName),
                        propertyType = ConvertPropertyType(type)
                    };
                    
                    switch (type) {

                        case ShaderUtil.ShaderPropertyType.Color:
                            materialValue.colorValue = material.GetColor(materialValue.shaderPropertyId);
                            break;

                        case ShaderUtil.ShaderPropertyType.Vector:
                            materialValue.vectorValue = material.GetVector(materialValue.shaderPropertyId);
                            break;

                        case ShaderUtil.ShaderPropertyType.Float:
                            materialValue.floatValue = material.GetFloat(materialValue.shaderPropertyId);
                            break;

                        case ShaderUtil.ShaderPropertyType.Range:
                            break;

                        case ShaderUtil.ShaderPropertyType.TexEnv:
                            materialValue.texture = material.GetTexture(materialValue.shaderPropertyId);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    propertyInfos[idx++] = new MaterialPropertyInfo() {
                        propertyName = propertyName,
                        propertyType = ConvertPropertyType(type),
                        propertyId = Shader.PropertyToID(propertyName)
                    };

                }

            }

            return new MaterialDatabase(materialInfos, propertyInfos);
        }

        private static MaterialPropertyType ConvertPropertyType(ShaderUtil.ShaderPropertyType type) {
            switch (type) {

                case ShaderUtil.ShaderPropertyType.Color:
                    return MaterialPropertyType.Color;

                case ShaderUtil.ShaderPropertyType.Vector:
                    return MaterialPropertyType.Vector;

                case ShaderUtil.ShaderPropertyType.Float:
                    return MaterialPropertyType.Float;

                case ShaderUtil.ShaderPropertyType.Range:
                    return MaterialPropertyType.Range;

                case ShaderUtil.ShaderPropertyType.TexEnv:
                    return MaterialPropertyType.Texture;

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
#else
        public static MaterialDatabase BuildMaterialDatabase(MaterialReference[] materialReferences) {
            return new MaterialDatabase(new MaterialInfo[0], new MaterialPropertyInfo[0]);
        }
#endif

    }

}