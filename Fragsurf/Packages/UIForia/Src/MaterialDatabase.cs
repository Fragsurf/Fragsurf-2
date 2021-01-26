using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia {

    public class MaterialDatabase {

        private MaterialInfo[] baseMaterialInfos;
        private MaterialPropertyInfo[] materialProperties;
        public Dictionary<long, MaterialInfo> materialMap;
        private int materialIdGenerator;
        private Dictionary<int, LightList<MaterialProperty>> instanceProperties;

        public MaterialDatabase(MaterialInfo[] baseMaterialInfos, MaterialPropertyInfo[] materialProperties) {
            materialIdGenerator = 1;
            this.baseMaterialInfos = baseMaterialInfos;
            this.materialProperties = materialProperties;
            this.materialMap = new Dictionary<long, MaterialInfo>();
            this.instanceProperties = new Dictionary<int, LightList<MaterialProperty>>();
            for (int i = 0; i < baseMaterialInfos.Length; i++) {
                materialMap.Add(new MaterialId(i + 1, 0).id, baseMaterialInfos[i]);
            }

        }

        public void Destroy() {

            foreach (KeyValuePair<long, MaterialInfo> kvp in materialMap) {
                MaterialId materialId = (MaterialId) kvp.Key;
                if (materialId.instanceId != 0) {
                    try {
                        UnityEngine.Object.Destroy(kvp.Value.material);
                    }
                    catch (Exception e) { }
                }
            }

        }

        public void OnElementDestroyed(UIElement element) {
            if (instanceProperties.TryGetValue(element.id, out LightList<MaterialProperty> list)) {
                list.Release();
                instanceProperties.Remove(element.id);
            }
        }

        public bool TryGetBaseMaterialId(string idSpan, out MaterialId materialId) {
            for (int i = 0; i < baseMaterialInfos.Length; i++) {
                if (baseMaterialInfos[i].materialName == idSpan) {
                    materialId = new MaterialId(i + 1, 0);
                    return true;
                }
            }

            materialId = default;
            return false;

        }

        public bool TryGetBaseMaterialId(CharSpan idSpan, out MaterialId materialId) {
            for (int i = 0; i < baseMaterialInfos.Length; i++) {
                if (baseMaterialInfos[i].materialName == idSpan) {
                    materialId = new MaterialId(i + 1, 0);
                    return true;
                }
            }

            materialId = default;
            return false;

        }

        public bool TryGetMaterialProperty(MaterialId materialId, string propertySpan, out MaterialPropertyInfo materialPropertyInfo) {

            MaterialInfo materialInfo = baseMaterialInfos[materialId.baseId - 1];
            for (int i = materialInfo.propertyRange.start; i < materialInfo.propertyRange.end; i++) {
                if (materialProperties[i].propertyName == propertySpan) {
                    materialPropertyInfo = materialProperties[i];
                    return true;
                }
            }

            materialPropertyInfo = default;
            return false;

        }

        public bool TryGetMaterialProperty(MaterialId materialId, CharSpan propertySpan, out MaterialPropertyInfo materialPropertyInfo) {
            MaterialInfo materialInfo = baseMaterialInfos[materialId.baseId - 1];
            for (int i = materialInfo.propertyRange.start; i < materialInfo.propertyRange.end; i++) {
                if (materialProperties[i].propertyName == propertySpan) {
                    materialPropertyInfo = materialProperties[i];
                    return true;
                }
            }

            materialPropertyInfo = default;
            return false;

        }

        // base materials (the ones we imported)
        // static materials (the ones we overrode in styles, if any)
        // instance materials (the ones we override per element (also animations) )

        public MaterialId CreateStaticMaterialOverride(MaterialId baseId, IList<MaterialValueOverride> propertyOverrides) {

            // get the base material (for now always a defined one)

            // make sure we actually override properties, if not dont create it

            // make new material clone of base
            // foreach property, set it
            // assign unique id to this material

            Material baseMaterial = baseMaterialInfos[baseId.baseId - 1].material;

            if (propertyOverrides.Count == 0) {
                return baseId;
            }

            Material material = new Material(baseMaterial);

            RangeInt range = new RangeInt(materialProperties.Length, 0);

            for (int i = 0; i < propertyOverrides.Count; i++) {
                SetProperty(material, propertyOverrides[i]);
            }

            MaterialId newId = new MaterialId(baseId.baseId, materialIdGenerator++);

            MaterialInfo newInfo = new MaterialInfo() {
                keywords = default,
                material = material,
                materialName = material.name,
                propertyRange = range
            };

            materialMap.Add(newId.id, newInfo);

            return newId;
        }

        private void SetProperty(Material material, MaterialValueOverride propertyOverride) {

            switch (propertyOverride.propertyType) {

                case MaterialPropertyType.Color:
                    material.SetColor(propertyOverride.propertyId, propertyOverride.value.colorValue);
                    break;

                case MaterialPropertyType.Float:
                    material.SetFloat(propertyOverride.propertyId, propertyOverride.value.floatValue);
                    break;

                case MaterialPropertyType.Vector:
                    material.SetVector(propertyOverride.propertyId, propertyOverride.value.vectorValue);
                    break;

                case MaterialPropertyType.Range:
                    throw new NotImplementedException();

                case MaterialPropertyType.Texture:
                    material.SetTexture(propertyOverride.propertyId, propertyOverride.value.texture);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public bool TryGetMaterial(MaterialId materialId, out MaterialInfo materialInfo) {

            if (materialMap.TryGetValue(materialId.id, out MaterialInfo info)) {
                materialInfo = info;
                return true;
            }

            materialInfo = default;
            return false;
        }

        public void SetInstanceProperty(int elementId, string materialName, string propertyName, in MaterialPropertyValue2 value) {
            for (int i = 0; i < baseMaterialInfos.Length; i++) {
                if (baseMaterialInfos[i].materialName == materialName) {
                    SetInstanceProperty(elementId, new MaterialId(i + 1, 0), propertyName, value);
                }
            }
        }

        public void SetInstanceProperty(int elementId, MaterialId materialId, string propertyName, in MaterialPropertyValue2 value) {

            // only care about base id here
            if (!materialMap.TryGetValue(materialId.baseId, out MaterialInfo info)) {
                return;
            }

            MaterialPropertyType propertyType = default;
            int propertyId = default;

            for (int i = info.propertyRange.start; i < info.propertyRange.end; i++) {
                if (materialProperties[i].propertyName == propertyName) {
                    propertyType = materialProperties[i].propertyType;
                    propertyId = materialProperties[i].propertyId;
                }
            }

            // could be per-instance array but i dont think we'll enough of these to merit the memory overhead
            if (!instanceProperties.TryGetValue(elementId, out LightList<MaterialProperty> properties)) {
                properties = LightList<MaterialProperty>.Get();
                instanceProperties.Add(elementId, properties);
            }

            for (int i = 0; i < properties.size; i++) {

                if (properties[i].materialId.baseId == materialId.baseId && properties[i].propertyId == propertyId) {
                    properties.array[i].value = value;
                }

            }

            properties.Add(new MaterialProperty() {
                value = value,
                materialId = materialId,
                propertyId = propertyId,
                propertyType = propertyType
            });

        }

        public int GetInstanceProperties(int elementId, MaterialId materialId, IList<MaterialProperty> output) {
            if (!instanceProperties.TryGetValue(elementId, out LightList<MaterialProperty> properties)) {
                return 0;
            }

            int cnt = 0;

            for (int i = 0; i < properties.size; i++) {
                ref MaterialProperty property = ref properties.array[i];
                if (property.materialId.baseId == materialId.baseId) {
                    cnt++;
                    output.Add(property);
                }
            }

            return cnt;
        }

        public int GetInstanceProperties(int elementId, MaterialId materialId, Material material) {
            if (!instanceProperties.TryGetValue(elementId, out LightList<MaterialProperty> properties)) {
                return 0;
            }

            int cnt = 0;

            for (int i = 0; i < properties.size; i++) {
                ref MaterialProperty property = ref properties.array[i];
                if (property.materialId.baseId == materialId.baseId) {
                    cnt++;
                    switch (property.propertyType) {

                        case MaterialPropertyType.Color:
                            material.SetColor(property.propertyId, property.value.colorValue);
                            break;

                        case MaterialPropertyType.Float:
                            material.SetFloat(property.propertyId, property.value.floatValue);
                            break;

                        case MaterialPropertyType.Vector:
                            material.SetVector(property.propertyId, property.value.vectorValue);
                            break;

                        case MaterialPropertyType.Range:
                            break;

                        case MaterialPropertyType.Texture:
                            material.SetTexture(property.propertyId, property.value.texture);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return cnt;
        }

        public int GetInstanceProperties(int elementId, MaterialId materialId, MaterialPropertyBlock propertyBlock) {
            if (!instanceProperties.TryGetValue(elementId, out LightList<MaterialProperty> properties)) {
                return 0;
            }

            int cnt = 0;

            for (int i = 0; i < properties.size; i++) {
                ref MaterialProperty property = ref properties.array[i];
                if (property.materialId.baseId == materialId.baseId) {
                    cnt++;
                    switch (property.propertyType) {

                        case MaterialPropertyType.Color:
                            propertyBlock.SetColor(property.propertyId, property.value.colorValue);
                            break;

                        case MaterialPropertyType.Float:
                            propertyBlock.SetFloat(property.propertyId, property.value.floatValue);
                            break;

                        case MaterialPropertyType.Vector:
                            propertyBlock.SetVector(property.propertyId, property.value.vectorValue);
                            break;

                        case MaterialPropertyType.Range:
                            break;

                        case MaterialPropertyType.Texture:
                            propertyBlock.SetTexture(property.propertyId, property.value.texture);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return cnt;
        }

    }

}