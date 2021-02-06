using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SourceUtils;
using SourceUtils.ValveBsp;
using SourceUtils.ValveBsp.Entities;
using UVector3 = UnityEngine.Vector3;
using UVector2 = UnityEngine.Vector2;
using SVector2 = SourceUtils.Vector2;
using SVector3 = SourceUtils.Vector3;

namespace Fragsurf.BSP
{
    public partial class BspToUnity
    {

        public Dictionary<string, GameObject> PropModels { get; private set; }

        private void GeneratePropModels()
        {
            var propModelContainer = CreateGameObject("[PropModels]");
            PropModels = new Dictionary<string, GameObject>();

            // static props
            for (int i = 0; i < _bsp.StaticProps.ModelCount; i++)
            {
                var modelName = _bsp.StaticProps.GetModelName(i);
                if (PropModels.ContainsKey(modelName)) continue;
                var obj = GeneratePropModel(modelName);
                obj.transform.SetParent(propModelContainer.transform);
                PropModels.Add(modelName, obj);
            }

            // dynamic props
            foreach (var ent in _bsp.Entities)
            {
                if (ent.ClassName.StartsWith("prop_dynamic"))
                {
                    var modelName = ent.GetRawPropertyValue("model");
                    if (PropModels.ContainsKey(modelName)) continue;
                    if (!string.IsNullOrEmpty(modelName))
                    {
                        var obj = GeneratePropModel(modelName);
                        obj.transform.SetParent(propModelContainer.transform, true);
                        obj.transform.eulerAngles = ent.Angles.TOUDirection();
                        PropModels.Add(modelName, obj);
                    }
                }
            }
        }

        private GameObject GeneratePropModel(string modelName)
        {
            if (!_resourceLoader.ContainsFile(modelName))
                return new GameObject(modelName);  // model not found

            var vvdPath = modelName.Replace(".mdl", ".vvd");
            var vtxPath = modelName.Replace(".mdl", ".dx90.vtx");

            var ssm = new SourceStudioModel();
            GameObject model;

            try
            {
                model = ssm.GenerateMesh(this, modelName, vvdPath, vtxPath, _resourceLoader);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to generate mesh: {e}\n{modelName}");
                model = new GameObject();
            }

            model.name = modelName;
            model.transform.localPosition = UVector3.zero;
            model.transform.localRotation = Quaternion.identity;
            model.transform.localScale = UVector3.one;
            model.SetActive(false);

            return model;
        }

        private void GenerateStaticProps()
        {
            var propModelContainer = CreateGameObject("[StaticModels]");
            var propMap = new Dictionary<int, GameObject>();

            // place props
            for (int i = 0; i < _bsp.StaticProps.PropCount; i++)
            {
                _bsp.StaticProps.GetPropModelSkin(i, out int modelIndex, out int skin);

                var modelName = _bsp.StaticProps.GetModelName(modelIndex);
                if (!PropModels.ContainsKey(modelName))
                    continue;

                _bsp.StaticProps.GetPropTransform(i, out SVector3 origin, out SVector3 angles, out float scale);
                _bsp.StaticProps.GetPropInfo(i, out StaticPropFlags flags, out bool solid, out uint diffuseModulation);

                var obj = GameObject.Instantiate<GameObject>(PropModels[modelName]);
                obj.SetActive(true);
                obj.transform.SetParent(propModelContainer.transform);
                obj.transform.localPosition = UVector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = UVector3.one * scale;
                obj.transform.position = origin.ToUVector() * Options.WorldScale;
                obj.transform.rotation = Quaternion.Euler(new UVector3(-angles.Z, -angles.Y, -angles.X));

                if (solid)
                {
                    foreach (var mr in obj.GetComponentsInChildren<MeshFilter>())
                    {
                        mr.gameObject.AddComponent<MeshCollider>();
                    }
                }
            }
        }
    }
}
