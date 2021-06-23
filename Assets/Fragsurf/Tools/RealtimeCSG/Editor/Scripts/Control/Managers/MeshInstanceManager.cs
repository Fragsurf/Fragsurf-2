//#define SHOW_GENERATED_MESHES
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using RealtimeCSG;
using RealtimeCSG.Foundation;
using RealtimeCSG.Components;
using UnityEditor.SceneManagement;
using SurfaceConfigurator;
using Chisel.Editors;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif


using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace InternalRealtimeCSG
{
    public sealed partial class MeshInstanceManager
    {
#if SHOW_GENERATED_MESHES
		public const HideFlags ComponentHideFlags = HideFlags.DontSaveInBuild;
#else
        public const HideFlags ComponentHideFlags = HideFlags.None
                                                    //| HideFlags.NotEditable       // when this is put into a prefab (when making a prefab containing a model for instance) this will make it impossible to delete 
                                                    //| HideFlags.HideInInspector   // apparently, when set, can cause issues with selection in sceneview 
                                                    | HideFlags.HideInHierarchy
                                                    | HideFlags.DontSaveInBuild
            ;
#endif

        internal const string MeshContainerName = "[generated-meshes]";
        private const string RenderMeshInstanceName = "[generated-render-mesh]";
        private const string ColliderMeshInstanceName = "[generated-collider-mesh]";
        private const string HelperMeshInstanceName = "[generated-helper-mesh]";

        public static void Shutdown()
        {
        }

        public static void OnDestroyed(GeneratedMeshes container)
        {
        }

        public static void Destroy(GeneratedMeshes generatedMeshes)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode
                || !generatedMeshes)
            {
                return;
            }

            if (generatedMeshes.owner)
            {
                generatedMeshes.owner.generatedMeshes = null;
            }

            generatedMeshes.owner = null;
            GameObjectExtensions.Destroy(generatedMeshes.gameObject);

            // Undo.DestroyObjectImmediate // NOTE: why was this used before?
            // can't use Undo variant here because it'll mark scenes as dirty on load ..
        }

        public static void OnCreated(GeneratedMeshes container)
        {
            ValidateGeneratedMeshesDelayed(container);
        }

        public static void OnEnable(GeneratedMeshes container)
        {
            if (container)
            {
                container.gameObject.SetActive(true);
            }
        }

        public static void OnDisable(GeneratedMeshes container)
        {
            if (container)
            {
                container.gameObject.SetActive(false);
            }
        }

        public static void OnCreated(GeneratedMeshInstance meshInstance)
        {
            var parent = meshInstance.transform.parent;
            GeneratedMeshes container = null;

            if (parent)
            {
                container = parent.GetComponent<GeneratedMeshes>();
            }

            if (!container)
            {
                GameObjectExtensions.Destroy(meshInstance.gameObject);
                return;
            }

            //EnsureValidHelperMaterials(container.owner, meshInstance);

            Initialize(container, meshInstance);

            var key = meshInstance.GenerateKey();
            if (container.GetMeshInstance(key) != meshInstance)
            {
                if (meshInstance && meshInstance.gameObject)
                {
                    GameObjectExtensions.Destroy(meshInstance.gameObject);
                }
            }
        }


        static void Initialize(GeneratedMeshes container, GeneratedMeshInstance meshInstance)
        {
            container.AddMeshInstance(meshInstance);
        }

        static void Initialize(GeneratedMeshes container, HelperSurfaceDescription helperSurface)
        {
            container.AddHelperSurface(helperSurface);
        }



        private readonly static HashSet<GeneratedMeshes> validateGeneratedMeshes = new HashSet<GeneratedMeshes>();
        private readonly static HashSet<CSGModel> validateModelWithChildren = new HashSet<CSGModel>();
        private readonly static HashSet<CSGModel> validateModelWithoutChildren = new HashSet<CSGModel>();

        public static void Update()
        {
            var models = CSGModelManager.GetAllModels();
            for (var i = 0; i < models.Length; i++)
            {
                var model = models[i];
                if (ModelTraits.IsModelEditable(model))
                {
                    MeshInstanceManager.ValidateModelDelayed(model);
                }
            }

            ignoreValidateModelDelayed = true;
            try
            {
                if (validateModelWithChildren.Count > 0)
                {
                    foreach (var model in validateModelWithChildren)
                    {
                        ValidateModelNow(model, checkChildren: true);
                    }
                    validateModelWithChildren.Clear();
                }
                if (validateModelWithoutChildren.Count > 0)
                {
                    foreach (var model in validateModelWithoutChildren)
                    {
                        ValidateModelNow(model, checkChildren: false);
                    }
                    validateModelWithoutChildren.Clear();
                }
            }
            finally
            {
                ignoreValidateModelDelayed = false;
            }

            for (var i = 0; i < models.Length; i++)
            {
                var model = models[i];
                if (!ModelTraits.IsModelEditable(model))
                    continue;

                var meshContainer = model.generatedMeshes;
                if (!meshContainer)
                    continue;

                if (!meshContainer.HasMeshInstances)
                {
                    MeshInstanceManager.ValidateGeneratedMeshesDelayed(meshContainer);
                    continue;
                }
            }

            if (validateGeneratedMeshes.Count > 0)
            {
                foreach (var generatedMeshes in validateGeneratedMeshes)
                {
                    var prevModel = generatedMeshes ? generatedMeshes.owner : null;
                    if (ValidateGeneratedMeshesNow(generatedMeshes))
                    {
                        if (prevModel)
                            prevModel.forceUpdate = true;
                    }
                }
                validateGeneratedMeshes.Clear();
            }

            MeshInstanceManager.UpdateTransforms();
        }

        public static void ValidateGeneratedMeshesDelayed(GeneratedMeshes meshContainer)
        {
            validateGeneratedMeshes.Add(meshContainer);
        }


        static bool ignoreValidateModelDelayed = false;
        internal static void ValidateModelDelayed(CSGModel model, bool checkChildren = false)
        {
            if (ignoreValidateModelDelayed)
                return;
            if (checkChildren)
            {
                validateModelWithChildren.Add(model);
                validateModelWithoutChildren.Remove(model);
            }
            else
            {
                if (!validateModelWithChildren.Contains(model))
                    validateModelWithoutChildren.Add(model);
            }
        }

        static void UpdateGeneratedMeshesFlags(CSGModel model, GeneratedMeshes generatedMeshes)
        {
            if (!generatedMeshes)
            {
                return;
            }

            if (generatedMeshes.enabled == false)
            {
                generatedMeshes.enabled = true;
            }

            var generatedMeshesGameObject = generatedMeshes.gameObject;
            var activated = (model.enabled && model.gameObject.activeSelf);
            if (generatedMeshesGameObject.activeSelf != activated)
            {
                generatedMeshesGameObject.SetActive(activated);
            }
        }

        static GeneratedMeshes EnsureOneValidGeneratedMeshesComponent(CSGModel model)
        {
            if (!model.isActiveAndEnabled)
            {
                return null;
            }

            // Find all the GeneratedMeshes inside this model
            var foundGeneratedMeshes = model.GetComponentsInChildren<GeneratedMeshes>(true);
            // If we have CSGModel components inside this CSGModel component, we ignore all the GeneratedMeshes inside those ..
            for (var i = foundGeneratedMeshes.Length - 1; i >= 0; i--)
            {
                var models = foundGeneratedMeshes[i].GetComponentsInParent<CSGModel>(includeInactive: true);
                var parentModel = models.Length == 0 ? null : models[0];
                if (parentModel != model)
                {
                    // TODO: should just swap with last element + keep track of our own count in array and use that below
                    ArrayUtility.RemoveAt(ref foundGeneratedMeshes, i);
                }
            }


            if (foundGeneratedMeshes.Length > 1)
            {
                var prevGeneratedMeshes = model.generatedMeshes;
                GeneratedMeshes newGeneratedMeshes = null;

                // Check if we have more than one GeneratedMeshes component, this can happen, for instance, 
                //	due to prefab merging issues or if the user duplicates the gameObject

                for (var i = foundGeneratedMeshes.Length - 1; i >= 0; i--)
                {
                    var generatedMeshesComponent = foundGeneratedMeshes[i];
                    if (!generatedMeshesComponent)
                        continue;

                    var generatedMeshesGameObject = generatedMeshesComponent.gameObject;
                    if (!newGeneratedMeshes ||
                        // Prefer to keep the GeneratedMesh that is currently used by the Model
                        generatedMeshesComponent == prevGeneratedMeshes)
                    {
                        // if the we already found a GeneratedMesh, and it's valid, try to see if we can destroy it
                        if (newGeneratedMeshes)
                        {
                            var newGeneratedMeshesGameObject = newGeneratedMeshes.gameObject;
                            if (GameObjectExtensions.TryDestroy(newGeneratedMeshesGameObject))
                            {
                                newGeneratedMeshes = generatedMeshesComponent;
                                continue;
                            }
                            // Fall through, we need to destroy this component after all
                        }
                        else
                        {
                            newGeneratedMeshes = generatedMeshesComponent;
                            continue;
                        }
                    }

                    // Try to destroy the GeneratedMeshes gameObject
                    if (!GameObjectExtensions.TryDestroy(generatedMeshesGameObject))
                    {
                        // If the we already found a GeneratedMesh, and it's valid, try to see if we can destroy that instead
                        var newGeneratedMeshesGameObject = newGeneratedMeshes.gameObject;
                        if (!GameObjectExtensions.TryDestroy(newGeneratedMeshesGameObject))
                        {
                            // Fall back to disabling the component instead
                            GameObjectExtensions.Destroy(newGeneratedMeshesGameObject);
                        }
                        prevGeneratedMeshes = null;
                        newGeneratedMeshes = generatedMeshesComponent;
                    }
                }

                model.generatedMeshes = newGeneratedMeshes;
                if (model.generatedMeshes)
                {
                    model.generatedMeshes.owner = model;
                    UpdateGeneratedMeshesFlags(model, model.generatedMeshes);
                    if (ValidateGeneratedMeshesNow(model.generatedMeshes, skipSiblingCheck: true) && model)
                        model.forceUpdate = true;
                    return model.generatedMeshes;
                }

                // Fall through, create a new GeneratedMeshes component
            }
            else
            if (foundGeneratedMeshes.Length == 1)
            {
                model.generatedMeshes = foundGeneratedMeshes[0];
                model.generatedMeshes.owner = model;
                UpdateGeneratedMeshesFlags(model, model.generatedMeshes);
                if (ValidateGeneratedMeshesNow(model.generatedMeshes, skipSiblingCheck: true) && model)
                    model.forceUpdate = true;
                return model.generatedMeshes;
            }


            // Create it if it doesn't exist
            var generatedMeshesObject = new GameObject(MeshContainerName);
            generatedMeshesObject.SetActive(false);

            var generatedMeshes = generatedMeshesObject.AddComponent<GeneratedMeshes>();
            generatedMeshes.owner = model;

            var generatedMeshesTransform = generatedMeshesObject.transform;
            generatedMeshesTransform.SetParent(model.transform, false);

            generatedMeshesObject.SetActive(true);

            UpdateGeneratedMeshesVisibility(generatedMeshes, model.ShowGeneratedMeshes);
            UpdateGeneratedMeshesFlags(model, generatedMeshes);
            model.generatedMeshes = generatedMeshes;
            model.generatedMeshes.owner = model;
            return generatedMeshes;
        }

        internal static bool ValidateModelNow(CSGModel model, bool checkChildren = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return true;

            if (!checkChildren &&
                model.generatedMeshes &&
                model.generatedMeshes.owner == model)
                return true;

            if (!ModelTraits.IsModelEditable(model) ||
                !model.isActiveAndEnabled)
                return false;

            EnsureOneValidGeneratedMeshesComponent(model);

            //model.forceUpdate = true; // TODO: this causes issues where models are constantly force updated
            return true;
        }

        private static bool ShouldRenderHelperSurface(RenderSurfaceType renderSurfaceType)
        {
            switch (renderSurfaceType)
            {
                case RenderSurfaceType.Hidden: return CSGSettings.ShowHiddenSurfaces;
                case RenderSurfaceType.Culled: return CSGSettings.ShowCulledSurfaces;
                case RenderSurfaceType.Collider: return CSGSettings.ShowColliderSurfaces;
                case RenderSurfaceType.Trigger: return CSGSettings.ShowTriggerSurfaces;
                case RenderSurfaceType.ShadowOnly:
                case RenderSurfaceType.CastShadows: return CSGSettings.ShowCastShadowsSurfaces;
                case RenderSurfaceType.ReceiveShadows: return CSGSettings.ShowReceiveShadowsSurfaces;
            }
            return false;
        }

        /*
		private static RenderSurfaceType EnsureValidHelperMaterials(CSGModel model, GeneratedMeshInstance meshInstance)
		{
			var surfaceType = !meshInstance.RenderMaterial ? meshInstance.RenderSurfaceType : 
								GetRenderSurfaceType(model, meshInstance);
			if (surfaceType != RenderSurfaceType.Normal)
				meshInstance.RenderMaterial = MaterialUtility.GetSurfaceMaterial(surfaceType);
			return surfaceType;
		}
		*/

        private static bool ValidMeshInstance(GeneratedMeshInstance meshInstance)
        {
            return meshInstance.IsValid();
        }

        public static bool HasVisibleMeshRenderer(GeneratedMeshInstance meshInstance)
        {
            if (!meshInstance)
                return false;
            return meshInstance.RenderSurfaceType == RenderSurfaceType.Normal;
        }

        public static bool HasRuntimeMesh(GeneratedMeshInstance meshInstance)
        {
            if (!meshInstance)
            {
                return false;
            }

            return meshInstance.RenderSurfaceType != RenderSurfaceType.Culled
                    && meshInstance.RenderSurfaceType != RenderSurfaceType.ReceiveShadows
                    && meshInstance.RenderSurfaceType != RenderSurfaceType.CastShadows
                    && meshInstance.RenderSurfaceType != RenderSurfaceType.Hidden;
        }

        public static void UpdateHelperSurfaces()
        {
            var models = InternalCSGModelManager.Models;
            for (var i = 0; i < models.Length; i++)
            {
                var model = models[i];

                if (!ModelTraits.IsModelEditable(model))
                {
                    continue;
                }

                var container = model.generatedMeshes;
                if (container.owner != model || !container.gameObject)
                {
                    continue;
                }

                if (!model.generatedMeshes)
                {
                    continue;
                }

                if (!container.HasHelperSurfaces)
                {
                    if (!container.HasMeshInstances)
                    {
                        ValidateGeneratedMeshesDelayed(container);
                    }
                    continue;
                }

                foreach (var meshInstance in container.MeshInstances)
                {
                    if (!meshInstance)
                    {
                        GameObjectExtensions.TryDestroy(meshInstance.gameObject);
                        return;
                    }
                }


                foreach (var helperSurface in container.HelperSurfaces)
                {
                    var renderSurfaceType = helperSurface.RenderSurfaceType;

                    if (!helperSurface.SharedMesh
                        || helperSurface.SharedMesh.vertexCount == 0
                        || !ShouldRenderHelperSurface(renderSurfaceType))
                    {
                        if (helperSurface.GameObject)
                        {
                            helperSurface.GameObject.SetActive(false);
                        }
                        continue;
                    }

                    if (!helperSurface.GameObject
                        || !helperSurface.MeshFilter
                        || !helperSurface.MeshRenderer
                        || helperSurface.MeshFilter.sharedMesh != helperSurface.SharedMesh)
                    {
                        MeshInstanceManager.UpdateHelperSurfaceGameObject(container, helperSurface);
                    }

                    if (!helperSurface.HasGeneratedNormals)
                    {
                        helperSurface.SharedMesh.RecalculateNormals();
                        helperSurface.HasGeneratedNormals = true;
                    }

                    helperSurface.GameObject.SetActive(true);
                    /*

					if (!showWireframe)
					{
						// "DrawMeshNow" so that it renders properly in all shading modes
						if (material.SetPass(0))
							Graphics.DrawMeshNow(meshInstance.SharedMesh,
													matrix);
					} else
					{
						Graphics.DrawMesh(meshInstance.SharedMesh,
										  matrix,
										  material,
										  layer: 0,
										  camera: camera,
										  submeshIndex: 0,
										  properties: null,
										  castShadows: false,
										  receiveShadows: false);
					}*/
                }
            }
        }

        public static UnityEngine.Object[] FindRenderers(CSGModel[] models)
        {
            var renderers = new List<UnityEngine.Object>();
            foreach (var model in models)
            {
                if (!model
                    || !model.gameObject.activeInHierarchy
                    || !model.generatedMeshes)
                {
                    continue;
                }

                foreach (var renderer in model.generatedMeshes.GetComponentsInChildren<MeshRenderer>())
                {
                    if (!renderer
                        || MaterialUtility.GetMaterialSurfaceType(renderer.sharedMaterial) == RenderSurfaceType.Normal)
                    {
                        continue;
                    }
                    renderers.Add(renderer);
                }
            }
            return renderers.ToArray();
        }

        internal static void ResetScene(Scene scene)
        {
            var meshInstances = SceneQueryUtility.GetAllComponentsInScene<GeneratedMeshInstance>(scene);
            for (int m = 0; m < meshInstances.Count; m++)
            {
                if (!meshInstances[m])
                    continue;

                var meshInstanceTransform = meshInstances[m].transform;
                var modelTransform = meshInstanceTransform ? meshInstanceTransform.parent ? meshInstanceTransform.parent.parent : null : null;
                var model = modelTransform ? modelTransform.GetComponent<CSGModel>() : null;
                if (model && !ModelTraits.IsModelEditable(model))
                    continue;
                model.generatedMeshes = null;
                meshInstances[m].hideFlags = HideFlags.None;
                var gameObject = meshInstances[m].gameObject;
                GameObjectExtensions.SanitizeGameObject(gameObject);
                GameObjectExtensions.TryDestroy(gameObject);
            }
        }

        internal static void Reset()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                var scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                    continue;
                ResetScene(scene);
            }

#if UNITY_2018_4_OR_NEWER
            if (CSGPrefabUtility.AreInPrefabMode())
            {
                var currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                ResetScene(currentPrefabStage.scene);
            }
#endif
        }

        public static RenderSurfaceType GetSurfaceType(GeneratedMeshDescription meshDescription, ModelSettingsFlags modelSettings)
        {
            if (meshDescription.meshQuery.LayerQuery == LayerUsageFlags.Culled) return RenderSurfaceType.Culled;
            switch (meshDescription.meshQuery.LayerParameterIndex)
            {
                case LayerParameterIndex.LayerParameter1:
                    {
                        switch (meshDescription.meshQuery.LayerQuery)
                        {
                            case LayerUsageFlags.RenderReceiveCastShadows:
                            case LayerUsageFlags.RenderReceiveShadows:
                            case LayerUsageFlags.RenderCastShadows:
                            case LayerUsageFlags.Renderable: return RenderSurfaceType.Normal;
                            case LayerUsageFlags.ReceiveShadows: return RenderSurfaceType.Hidden;
                            case (LayerUsageFlags.CastShadows | LayerUsageFlags.ReceiveShadows):
                            case LayerUsageFlags.CastShadows: return RenderSurfaceType.ShadowOnly;
                            default:
                                return RenderSurfaceType.Culled;
                        }
                    }
                case LayerParameterIndex.LayerParameter2:
                    {
                        if ((modelSettings & ModelSettingsFlags.IsTrigger) != 0)
                        {
                            return RenderSurfaceType.Trigger;
                        }
                        else
                        {
                            return RenderSurfaceType.Collider;
                        }
                    }
                case LayerParameterIndex.None:
                    {
                        switch (meshDescription.meshQuery.LayerQuery)
                        {
                            case LayerUsageFlags.None: return RenderSurfaceType.Hidden;
                            case LayerUsageFlags.CastShadows: return RenderSurfaceType.CastShadows;
                            case LayerUsageFlags.ReceiveShadows: return RenderSurfaceType.ReceiveShadows;
                            case LayerUsageFlags.Collidable:
                                {
                                    if ((modelSettings & ModelSettingsFlags.IsTrigger) != 0)
                                    {
                                        return RenderSurfaceType.Trigger;
                                    }
                                    else
                                        return RenderSurfaceType.Collider;
                                }
                            default:
                            case LayerUsageFlags.Culled:
                                return RenderSurfaceType.Culled;
                        }
                    }
            }
            return RenderSurfaceType.Normal;
        }

        static GeneratedMeshInstance SafeGetGeneratedMeshInstance(GameObject meshInstanceGameObject)
        {
            GeneratedMeshInstance instance;
            if (meshInstanceGameObject.TryGetComponent(out instance))
                return instance;

            return meshInstanceGameObject.AddComponent<GeneratedMeshInstance>();
        }

        static void RemoveComponent<T>(GameObject gameObject) where T : UnityEngine.Component
        {
            T component;
            if (!gameObject.TryGetComponent(out component))
                return;
            UnityEngine.Object.DestroyImmediate(component);
        }

        static void RemoveMeshInstanceComponents(GameObject gameObject, GeneratedMeshDescription meshDescription)
        {
            var parameterIndex = meshDescription.meshQuery.LayerParameterIndex;
            if (parameterIndex != LayerParameterIndex.PhysicsMaterial)
            {
                RemoveComponent<MeshCollider>(gameObject);
            }
            if (parameterIndex != LayerParameterIndex.RenderMaterial)
            {
                RemoveComponent<MeshFilter>(gameObject);
                RemoveComponent<MeshRenderer>(gameObject);
            }
        }

        static void GetGameObjectAndGeneratedMeshInstance(List<GameObject> unusedInstances, GeneratedMeshDescription meshDescription, out GameObject meshInstanceGameObject, out GeneratedMeshInstance meshInstance)
        {
            if (unusedInstances == null ||
                unusedInstances.Count == 0)
            {
                meshInstanceGameObject = new GameObject();
                meshInstance = meshInstanceGameObject.AddComponent<GeneratedMeshInstance>();
                return;
            }

            for (int i = 0; i < unusedInstances.Count; i++)
            {
                GeneratedMeshInstance instance;
                if (!unusedInstances[i].TryGetComponent(out instance))
                    continue;
                if (meshDescription.meshQuery.LayerParameterIndex == instance.MeshDescription.meshQuery.LayerParameterIndex)
                {
                    meshInstanceGameObject = unusedInstances[i];
                    meshInstanceGameObject.hideFlags = HideFlags.None;
                    unusedInstances.RemoveAt(i);
                    meshInstance = SafeGetGeneratedMeshInstance(meshInstanceGameObject);
                    return;
                }
            }

            meshInstanceGameObject = unusedInstances[unusedInstances.Count - 1];
            meshInstanceGameObject.hideFlags = HideFlags.None;
            RemoveMeshInstanceComponents(meshInstanceGameObject, meshDescription);
            meshInstance = SafeGetGeneratedMeshInstance(meshInstanceGameObject);
            unusedInstances.RemoveAt(unusedInstances.Count - 1);
        }


        public static GeneratedMeshInstance CreateMeshInstance(GeneratedMeshes generatedMeshes, GeneratedMeshDescription meshDescription, ModelSettingsFlags modelSettings, RenderSurfaceType renderSurfaceType, List<GameObject> unusedInstances)
        {
            if (!generatedMeshes || !generatedMeshes.owner)
                return null;


            var generatedMeshesTransform = generatedMeshes.transform;
            GameObject meshInstanceGameObject = null;
            GeneratedMeshInstance meshInstance = null;

            GetGameObjectAndGeneratedMeshInstance(unusedInstances, meshDescription, out meshInstanceGameObject, out meshInstance);

            meshInstance.Reset();
            meshInstanceGameObject.SetActive(false);
            meshInstanceGameObject.transform.SetParent(generatedMeshesTransform, false);
            meshInstanceGameObject.transform.localPosition = MathConstants.zeroVector3;
            meshInstanceGameObject.transform.localRotation = MathConstants.identityQuaternion;
            meshInstanceGameObject.transform.localScale = MathConstants.oneVector3;


            var containerStaticFlags = GameObjectUtility.GetStaticEditorFlags(generatedMeshes.owner.gameObject);
            GameObjectUtility.SetStaticEditorFlags(meshInstanceGameObject, containerStaticFlags);


            Material renderMaterial = null;
            PhysicMaterial physicsMaterial = null;
            if (meshDescription.surfaceParameter != 0)
            {
                var obj = EditorUtility.InstanceIDToObject(meshDescription.surfaceParameter);
                if (obj)
                {
                    switch (meshDescription.meshQuery.LayerParameterIndex)
                    {
                        case LayerParameterIndex.RenderMaterial: { renderMaterial = obj as Material; break; }
                        case LayerParameterIndex.PhysicsMaterial: { physicsMaterial = obj as PhysicMaterial; break; }
                    }
                }
            }

            meshInstance.MeshDescription = meshDescription;

            // Our mesh has not been initialized yet, so make sure we reflect that fact
            meshInstance.MeshDescription.geometryHashValue = 0;
            meshInstance.MeshDescription.surfaceHashValue = 0;
            meshInstance.MeshDescription.vertexCount = 0;
            meshInstance.MeshDescription.indexCount = 0;

            meshInstance.RenderMaterial = renderMaterial;
            meshInstance.PhysicsMaterial = physicsMaterial;
            meshInstance.RenderSurfaceType = renderSurfaceType;

            meshInstanceGameObject.SetActive(true);

            Initialize(generatedMeshes, meshInstance);
            return meshInstance;
        }

        public static void UpdateHelperSurfaceGameObject(GeneratedMeshes container, HelperSurfaceDescription helperSurfaceDescription)
        {
            if (!helperSurfaceDescription.GameObject ||
                !helperSurfaceDescription.MeshFilter ||
                !helperSurfaceDescription.MeshRenderer)
            {
                if (helperSurfaceDescription.GameObject) GameObjectExtensions.Destroy(helperSurfaceDescription.GameObject);

                helperSurfaceDescription.GameObject = new GameObject(HelperMeshInstanceName);
                helperSurfaceDescription.GameObject.SetActive(false);
                helperSurfaceDescription.GameObject.transform.SetParent(container.transform, true);
                helperSurfaceDescription.GameObject.transform.localPosition = Vector3.zero;
                helperSurfaceDescription.GameObject.transform.localRotation = Quaternion.identity;
                helperSurfaceDescription.GameObject.transform.localScale = Vector3.one;
                helperSurfaceDescription.GameObject.hideFlags = HideFlags.DontSave;
                helperSurfaceDescription.MeshFilter = helperSurfaceDescription.GameObject.AddComponent<MeshFilter>();
                helperSurfaceDescription.MeshRenderer = helperSurfaceDescription.GameObject.AddComponent<MeshRenderer>();
            }
            if (helperSurfaceDescription.MeshFilter.sharedMesh != helperSurfaceDescription.SharedMesh)
                helperSurfaceDescription.MeshFilter.sharedMesh = helperSurfaceDescription.SharedMesh;

            var sharedMaterial = MaterialUtility.GetSurfaceMaterial(helperSurfaceDescription.RenderSurfaceType);
            if (helperSurfaceDescription.MeshRenderer.sharedMaterial != sharedMaterial)
                helperSurfaceDescription.MeshRenderer.sharedMaterial = sharedMaterial;
        }

        public static HelperSurfaceDescription CreateHelperSurfaceDescription(GeneratedMeshes container, ModelSettingsFlags modelSettings, GeneratedMeshDescription meshDescription, RenderSurfaceType renderSurfaceType)
        {
            var instance = new HelperSurfaceDescription
            {
                RenderSurfaceType = renderSurfaceType,
                MeshDescription = meshDescription
            };

            // Our mesh has not been initialized yet, so make sure we reflect that fact
            instance.MeshDescription.geometryHashValue = 0;
            instance.MeshDescription.surfaceHashValue = 0;
            instance.MeshDescription.vertexCount = 0;
            instance.MeshDescription.indexCount = 0;

            Initialize(container, instance);
            return instance;
        }

        internal static void ClearOrCreateMesh(string baseName, bool editorOnly, ref bool hasGeneratedNormals, ref Mesh sharedMesh)
        {
            bool recreateMeshes = CSGPrefabUtility.IsPartOfAsset(sharedMesh);

            hasGeneratedNormals = false;
            if (!recreateMeshes && sharedMesh)
            {
                sharedMesh.Clear(keepVertexLayout: true);
                return;
            }

            sharedMesh = new Mesh();
            sharedMesh.name = string.Format("<{0} generated {1}>", baseName, sharedMesh.GetInstanceID());
            sharedMesh.MarkDynamic();
            if (editorOnly)
                sharedMesh.hideFlags = HideFlags.DontSaveInBuild;
        }

        public static bool UsesLightmapUVs(CSGModel model)
        {
            var staticFlags = GameObjectUtility.GetStaticEditorFlags(model.gameObject);
#if UNITY_2019_2_OR_NEWER
            if ((staticFlags & StaticEditorFlags.ContributeGI) != StaticEditorFlags.ContributeGI)
                return false;
#else
            if ((staticFlags & StaticEditorFlags.LightmapStatic) != StaticEditorFlags.LightmapStatic)
                return false;            
#endif
            return true;
        }

        public static bool NeedToGenerateLightmapUVsForModel(CSGModel model)
        {
            if (!ModelTraits.IsModelEditable(model))
                return false;

            if (!model.generatedMeshes)
                return false;

            var container = model.generatedMeshes;
            if (!container || container.owner != model)
                return false;

            if (!container.HasMeshInstances)
                return false;

            if (!UsesLightmapUVs(model))
                return false;

            foreach (var instance in container.MeshInstances)
            {
                if (!instance)
                    continue;

                if (NeedToGenerateLightmapUVsForInstance(instance))
                    return true;
            }
            return false;
        }

        public static void GenerateSurfaceDataForModel(CSGModel model, List<SurfaceDatabase> dbs)
        {
            if (dbs == null || dbs.Count == 0)
            {
                return;
            }

            var container = model.generatedMeshes;
            if (!container || !container.owner)
                return;

            if (!container.HasMeshInstances)
                return;

            foreach (var instance in container.MeshInstances)
            {
                if (instance.RenderSurfaceType == RenderSurfaceType.Normal)
                {
                    if (!instance.gameObject.TryGetComponent(out SurfaceTypeIdentifier id))
                    {
                        id = instance.gameObject.AddComponent<SurfaceTypeIdentifier>();
                    }
                    var cfg = dbs[0].FindSurfaceConfig(instance.RenderMaterial);
                    if (cfg != null)
                    {
                        id.SurfaceType = cfg.SurfaceType;
                    }
                    if (!instance.gameObject.TryGetComponent(out SteamAudio.SteamAudioGeometry geo))
                    {
                        geo = instance.gameObject.AddComponent<SteamAudio.SteamAudioGeometry>();
                    }
                    if (!instance.gameObject.TryGetComponent(out SteamAudio.SteamAudioMaterial audiomat))
                    {
                        audiomat = instance.gameObject.AddComponent<SteamAudio.SteamAudioMaterial>();
                    }
                    audiomat.Preset = SurfaceTypeToMaterialPreset(id.SurfaceType);
                }
            }
        }

        public static void GenerateLightmapUVsForModel(CSGModel model)
        {
            if (!ModelTraits.IsModelEditable(model))
                return;

            if (!model.generatedMeshes)
                return;

            var container = model.generatedMeshes;
            if (!container || !container.owner)
                return;

            if (!container.HasMeshInstances)
                return;

            var uvGenerationSettings = new UnityEditor.UnwrapParam
            {
                angleError = Mathf.Clamp(model.angleError, CSGModel.MinAngleError, CSGModel.MaxAngleError),
                areaError = Mathf.Clamp(model.areaError, CSGModel.MinAreaError, CSGModel.MaxAreaError),
                hardAngle = model.hardAngle,
                packMargin = model.packMargin / 1024.0f
            };

            foreach (var instance in container.MeshInstances)
            {
                if (!instance)
                    continue;
                if (!instance.SharedMesh)
                {
                    instance.FindMissingSharedMesh();
                    if (!instance.SharedMesh)
                        continue;
                }

                GenerateLightmapUVsForInstance(instance, model, uvGenerationSettings);
            }
        }

        private static void GenerateLightmapUVsForInstance(GeneratedMeshInstance instance, CSGModel model, UnwrapParam param)
        {
            var meshRendererComponent = instance.CachedMeshRenderer;
            if (!meshRendererComponent)
            {
                var gameObject = instance.gameObject;
                meshRendererComponent = gameObject.GetComponent<MeshRenderer>();
                instance.CachedMeshRendererSO = null;
            }

            if (!meshRendererComponent)
                return;

            meshRendererComponent.realtimeLightmapIndex = -1;
            meshRendererComponent.lightmapIndex = -1;

            var oldVertices = instance.SharedMesh.vertices;
            if (oldVertices.Length == 0)
                return;

            var tempMesh = instance.SharedMesh.Clone();
            instance.SharedMesh = tempMesh;

            UnityEngine.Object pingObject = model;
            if (model.ShowGeneratedMeshes) pingObject = instance;
            UnityEngine.Debug.Log("Generating lightmap UVs (by Unity) for the mesh " + instance.name + " of the Model named \"" + model.name + "\"\n", pingObject);
            //var optimizeTime = EditorApplication.timeSinceStartup;
            //MeshUtility.Optimize(instance.SharedMesh);
            //optimizeTime = EditorApplication.timeSinceStartup - optimizeTime;

            var lightmapGenerationTime = EditorApplication.timeSinceStartup;
            MeshUtility.Optimize(instance.SharedMesh);
            Unwrapping.GenerateSecondaryUVSet(instance.SharedMesh, param);
            //xatlas.Unwrap(instance.SharedMesh, param);
            lightmapGenerationTime = EditorApplication.timeSinceStartup - lightmapGenerationTime;

            UnityEngine.Debug.Log(//"\tMesh optimizing in " + (optimizeTime * 1000) + " ms\n"+
                      "\tUV generation in " + (lightmapGenerationTime * 1000) + " ms\n", model);

            EditorSceneManager.MarkSceneDirty(instance.gameObject.scene);
            instance.LightingHashValue = instance.MeshDescription.geometryHashValue;
            instance.HasUV2 = true;
        }

        private static bool NeedToGenerateLightmapUVsForInstance(GeneratedMeshInstance instance)
        {
            return !instance.HasUV2 && instance.RenderSurfaceType == RenderSurfaceType.Normal;
        }

        private static bool NeedCollider(GeneratedMeshInstance instance)
        {
            return (instance.RenderSurfaceType == RenderSurfaceType.Collider
                || instance.RenderSurfaceType == RenderSurfaceType.Trigger)
                && instance.SharedMesh.bounds.size.magnitude > MathConstants.EqualityEpsilon;
        }

        private static bool NeedMeshRenderer(RenderSurfaceType renderSurfaceType)
        {
            return (renderSurfaceType == RenderSurfaceType.Normal ||
                    renderSurfaceType == RenderSurfaceType.ShadowOnly);
        }

        static StaticEditorFlags FilterStaticEditorFlags(StaticEditorFlags modelStaticFlags, RenderSurfaceType renderSurfaceType)
        {
            if (!NeedMeshRenderer(renderSurfaceType))
            {
                return (StaticEditorFlags)0;
            }

            var meshStaticFlags = modelStaticFlags;
            var walkable = renderSurfaceType != RenderSurfaceType.Hidden &&
                            renderSurfaceType != RenderSurfaceType.ShadowOnly &&
                            renderSurfaceType != RenderSurfaceType.Culled &&
                            renderSurfaceType != RenderSurfaceType.Trigger;

            if (!walkable)
            {
                meshStaticFlags = meshStaticFlags & ~StaticEditorFlags.NavigationStatic;
            }


            // This fixes a bug in 2018.3 where it tries to generate lightmaps for ShadowOnly surfaces ..
            // .. but then rage quits because it doesn't have any normals
#if UNITY_2019_2_OR_NEWER
            if (renderSurfaceType == RenderSurfaceType.ShadowOnly)
                meshStaticFlags = meshStaticFlags & ~(StaticEditorFlags.ContributeGI | StaticEditorFlags.ReflectionProbeStatic);
#else
            if (renderSurfaceType == RenderSurfaceType.ShadowOnly)
                meshStaticFlags = meshStaticFlags & ~(StaticEditorFlags.LightmapStatic | StaticEditorFlags.ReflectionProbeStatic);            
#endif

            return meshStaticFlags & modelStaticFlags;
        }

        static string MaterialToString(Material mat)
        {
            if (ReferenceEquals(mat, null))
                return "null";
            if (!mat)
                return "invalid";
            return mat.name + " " + mat.GetInstanceID().ToString();
        }

        public static void ClearUVs(CSGModel model)
        {
            if (!model.generatedMeshes)
                return;

            var container = model.generatedMeshes;
            if (!container || !container.owner)
                return;

            foreach (var instance in container.MeshInstances)
            {
                if (!instance)
                    continue;

                Refresh(instance, model, onlyFastRefreshes: false, skipAssetDatabaseUpdate: true);
                ClearUVs(instance);
            }
        }

        public static void ClearUVs(GeneratedMeshInstance instance)
        {
            var meshRendererComponent = instance.CachedMeshRenderer;
            if (meshRendererComponent)
            {
                meshRendererComponent.realtimeLightmapIndex = -1;
                meshRendererComponent.lightmapIndex = -1;
            }
            instance.LightingHashValue = 0;
            instance.HasUV2 = false;
        }

        public static void Refresh(CSGModel model, bool postProcessScene = false, bool onlyFastRefreshes = true)
        {
            if (!ModelTraits.IsModelEditable(model))
            {
                return;
            }

            var generatedMeshes = model.generatedMeshes;
            if (!generatedMeshes || generatedMeshes.owner != model)
            {
                return;
            }

            foreach (var instance in generatedMeshes.MeshInstances)
            {
                if (!instance)
                {
                    continue;
                }
                Refresh(instance, model, postProcessScene, onlyFastRefreshes, skipAssetDatabaseUpdate: true);
            }
        }

        private static SteamAudio.MaterialPreset SurfaceTypeToMaterialPreset(SurfaceType surface)
        {
            switch (surface)
            {
                case SurfaceType.Carpet:
                    return SteamAudio.MaterialPreset.Carpet;
                case SurfaceType.Concrete:
                case SurfaceType.Tile:
                    return SteamAudio.MaterialPreset.Concrete;
                case SurfaceType.Glass:
                    return SteamAudio.MaterialPreset.Glass;
                case SurfaceType.Gravel:
                    return SteamAudio.MaterialPreset.Gravel;
                case SurfaceType.Ladder:
                case SurfaceType.Metal:
                case SurfaceType.MetalGrate:
                    return SteamAudio.MaterialPreset.Metal;
                case SurfaceType.Wood:
                    return SteamAudio.MaterialPreset.Wood;
                case SurfaceType.Dirt:
                case SurfaceType.Flesh:
                case SurfaceType.Grass:
                case SurfaceType.Mud:
                case SurfaceType.Plastic:
                case SurfaceType.Sand:
                    return SteamAudio.MaterialPreset.Generic;
            }
            return SteamAudio.MaterialPreset.Generic;
        }

        //		internal static double updateMeshColliderMeshTime = 0.0;
        public static void Refresh(GeneratedMeshInstance instance, CSGModel owner, bool postProcessScene = false, bool onlyFastRefreshes = true, bool skipAssetDatabaseUpdate = true)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode
                || !instance
                || !instance.SharedMesh)
            {
                return;
            }

            if (postProcessScene)
            {
                onlyFastRefreshes = false;
            }

            // Update the flags
            var oldRenderSurfaceType = instance.RenderSurfaceType;
            instance.RenderSurfaceType = GetSurfaceType(instance.MeshDescription, owner.Settings);
            instance.Dirty = instance.Dirty || (oldRenderSurfaceType != instance.RenderSurfaceType);

            // Update the transform, if incorrect
            var gameObject = instance.gameObject;
            if (gameObject.transform.localPosition != MathConstants.zeroVector3) gameObject.transform.localPosition = MathConstants.zeroVector3;
            if (gameObject.transform.localRotation != MathConstants.identityQuaternion) gameObject.transform.localRotation = MathConstants.identityQuaternion;
            if (gameObject.transform.localScale != MathConstants.oneVector3) gameObject.transform.localScale = MathConstants.oneVector3;


#if SHOW_GENERATED_MESHES
			var meshInstanceFlags   = HideFlags.None;
			var transformFlags      = HideFlags.None;
			var gameObjectFlags     = HideFlags.None;
#else
            var meshInstanceFlags = HideFlags.DontSaveInBuild;// | HideFlags.NotEditable;
            var transformFlags = HideFlags.HideInInspector;// | HideFlags.NotEditable;
            var gameObjectFlags = HideFlags.None;
#endif

            if (gameObject.transform.hideFlags != transformFlags) { gameObject.transform.hideFlags = transformFlags; }
            if (gameObject.hideFlags != gameObjectFlags) { gameObject.hideFlags = gameObjectFlags; }
            if (instance.hideFlags != meshInstanceFlags) { instance.hideFlags = meshInstanceFlags; }


            var showVisibleSurfaces = instance.RenderSurfaceType != RenderSurfaceType.Normal ||
                                        (RealtimeCSG.CSGSettings.VisibleHelperSurfaces & HelperSurfaceFlags.ShowVisibleSurfaces) != 0;

            CSGVisibilityUtility.SetGameObjectVisibility(gameObject, showVisibleSurfaces);
            if (!instance.enabled) instance.enabled = true;


            // Update navigation on mesh
            var oldStaticFlags = GameObjectUtility.GetStaticEditorFlags(gameObject);
            var newStaticFlags = FilterStaticEditorFlags(GameObjectUtility.GetStaticEditorFlags(owner.gameObject), instance.RenderSurfaceType);
            if (newStaticFlags != oldStaticFlags)
            {
                GameObjectUtility.SetStaticEditorFlags(gameObject, newStaticFlags);
            }

            var meshFilterComponent = instance.CachedMeshFilter;
            var meshRendererComponent = instance.CachedMeshRenderer;

            var needMeshRenderer = NeedMeshRenderer(instance.RenderSurfaceType);
            if (needMeshRenderer)
            {
                if (!meshRendererComponent)
                {
                    meshRendererComponent = gameObject.GetComponent<MeshRenderer>();
                    instance.CachedMeshRendererSO = null;
                }
                if (!meshFilterComponent)
                {
                    meshFilterComponent = gameObject.GetComponent<MeshFilter>();
                    if (!meshFilterComponent)
                    {
                        meshFilterComponent = gameObject.AddComponent<MeshFilter>();
                        instance.CachedMeshRendererSO = null;
                        instance.Dirty = true;
                    }
                }

                //				var ownerReceiveShadows = owner.ReceiveShadows;
                //				var shadowCastingMode	= owner.ShadowCastingModeFlags;
                var ownerReceiveShadows = true;
                var shadowCastingMode = owner.IsTwoSidedShadows ? UnityEngine.Rendering.ShadowCastingMode.TwoSided : UnityEngine.Rendering.ShadowCastingMode.On;
                if (instance.RenderSurfaceType == RenderSurfaceType.ShadowOnly)
                {
                    shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }


                switch (instance.MeshDescription.meshQuery.LayerQuery)
                {
                    case LayerUsageFlags.RenderReceiveCastShadows:
                        {
                            break;
                        }
                    case LayerUsageFlags.RenderReceiveShadows:
                        {
                            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            break;
                        }
                    case LayerUsageFlags.RenderCastShadows:
                        {
                            ownerReceiveShadows = false;
                            break;
                        }
                    case LayerUsageFlags.Renderable:
                        {
                            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                            ownerReceiveShadows = false;
                            break;
                        }
                    case LayerUsageFlags.CastShadows:
                        {
                            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                            ownerReceiveShadows = false;
                            break;
                        }
                }


                var requiredMaterial = instance.RenderMaterial;
                if (shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly)
                    // Note: need non-transparent material here
                    requiredMaterial = MaterialUtility.DefaultMaterial;

                if (!requiredMaterial)
                    requiredMaterial = MaterialUtility.MissingMaterial;

                if (!meshRendererComponent)
                {
                    meshRendererComponent = gameObject.AddComponent<MeshRenderer>();
                    meshRendererComponent.sharedMaterial = requiredMaterial;
                    meshRendererComponent.gameObject.name = RenderMeshInstanceName;
                    instance.CachedMeshRendererSO = null;
                    instance.Dirty = true;
                    // we don't actually want the unity style of rendering a wireframe 
                    // for our meshes, so we turn it off
                    //*
                    EditorUtility.SetSelectedRenderState(meshRendererComponent, EditorSelectedRenderState.Hidden);
                    //*/
                }

                if ((meshFilterComponent.hideFlags & HideFlags.HideInHierarchy) == 0)
                {
                    meshFilterComponent.hideFlags |= HideFlags.HideInHierarchy;
                }

                if ((meshRendererComponent.hideFlags & HideFlags.HideInHierarchy) == 0)
                {
                    meshRendererComponent.hideFlags |= HideFlags.HideInHierarchy;
                }

                if (instance.RenderSurfaceType != RenderSurfaceType.ShadowOnly)
                {
                    if (instance.HasUV2 &&
                        (instance.LightingHashValue != instance.MeshDescription.geometryHashValue) && meshRendererComponent)
                    {
                        instance.ResetUVTime = Time.realtimeSinceStartup;
                        if (instance.HasUV2)
                            ClearUVs(instance);
                    }

                    if ((owner.AutoRebuildUVs || postProcessScene))
                    {
                        if ((float.IsPositiveInfinity(instance.ResetUVTime) || ((Time.realtimeSinceStartup - instance.ResetUVTime) > 2.0f)) &&
                            NeedToGenerateLightmapUVsForModel(owner))
                        {
                            GenerateLightmapUVsForModel(owner);
                        }
                    }
                }

                if (!postProcessScene &&
                    meshFilterComponent.sharedMesh != instance.SharedMesh)
                {
                    meshFilterComponent.sharedMesh = instance.SharedMesh;
                }

                if (meshRendererComponent &&
                    meshRendererComponent.shadowCastingMode != shadowCastingMode)
                {
                    meshRendererComponent.shadowCastingMode = shadowCastingMode;
                    instance.Dirty = true;
                }

                if (meshRendererComponent &&
                    meshRendererComponent.receiveShadows != ownerReceiveShadows)
                {
                    meshRendererComponent.receiveShadows = ownerReceiveShadows;
                    instance.Dirty = true;
                }


                //*
                if (!onlyFastRefreshes)
                {
                    var meshRendererComponentSO = instance.CachedMeshRendererSO as UnityEditor.SerializedObject;
                    if (meshRendererComponentSO == null)
                    {
                        if (meshRendererComponent)
                        {
                            instance.CachedMeshRendererSO =
                            meshRendererComponentSO = new SerializedObject(meshRendererComponent);
                        }
                    }
                    else
                    if (!meshRendererComponent)
                    {
                        instance.CachedMeshRendererSO =
                        meshRendererComponentSO = null;
                    }
                    if (meshRendererComponentSO != null)
                    {
                        bool SOModified = false;
                        meshRendererComponentSO.Update();
                        var scaleInLightmapProperty = meshRendererComponentSO.FindProperty("m_ScaleInLightmap");
                        var scaleInLightmap = owner.scaleInLightmap;
                        if (scaleInLightmapProperty != null &&
                            scaleInLightmapProperty.floatValue != scaleInLightmap)
                        {
                            scaleInLightmapProperty.floatValue = scaleInLightmap;
                            SOModified = true;
                        }

                        var autoUVMaxDistanceProperty = meshRendererComponentSO.FindProperty("m_AutoUVMaxDistance");
                        var autoUVMaxDistance = owner.autoUVMaxDistance;
                        if (autoUVMaxDistanceProperty != null &&
                            autoUVMaxDistanceProperty.floatValue != autoUVMaxDistance)
                        {
                            autoUVMaxDistanceProperty.floatValue = autoUVMaxDistance;
                            SOModified = true;
                        }

                        var autoUVMaxAngleProperty = meshRendererComponentSO.FindProperty("m_AutoUVMaxAngle");
                        var autoUVMaxAngle = owner.autoUVMaxAngle;
                        if (autoUVMaxAngleProperty != null &&
                            autoUVMaxAngleProperty.floatValue != autoUVMaxAngle)
                        {
                            autoUVMaxAngleProperty.floatValue = autoUVMaxAngle;
                            SOModified = true;
                        }

                        var ignoreNormalsProperty = meshRendererComponentSO.FindProperty("m_IgnoreNormalsForChartDetection");
                        var ignoreNormals = owner.IgnoreNormals;
                        if (ignoreNormalsProperty != null &&
                            ignoreNormalsProperty.boolValue != ignoreNormals)
                        {
                            ignoreNormalsProperty.boolValue = ignoreNormals;
                            SOModified = true;
                        }

                        var minimumChartSizeProperty = meshRendererComponentSO.FindProperty("m_MinimumChartSize");
                        var minimumChartSize = owner.minimumChartSize;
                        if (minimumChartSizeProperty != null &&
                            minimumChartSizeProperty.intValue != minimumChartSize)
                        {
                            minimumChartSizeProperty.intValue = minimumChartSize;
                            SOModified = true;
                        }

                        var preserveUVsProperty = meshRendererComponentSO.FindProperty("m_PreserveUVs");
                        var preserveUVs = owner.PreserveUVs;
                        if (preserveUVsProperty != null &&
                            preserveUVsProperty.boolValue != preserveUVs)
                        {
                            preserveUVsProperty.boolValue = preserveUVs;
                            SOModified = true;
                        }

#if UNITY_2017_2_OR_NEWER
                        var stitchLightmapSeamsProperty = meshRendererComponentSO.FindProperty("m_StitchLightmapSeams");
                        var stitchLightmapSeams = owner.StitchLightmapSeams;
                        if (stitchLightmapSeamsProperty != null && // Note that some alpha/beta versions of 2017.2 had a different name
                            stitchLightmapSeamsProperty.boolValue != stitchLightmapSeams)
                        {
                            stitchLightmapSeamsProperty.boolValue = stitchLightmapSeams;
                            SOModified = true;
                        }
#endif

                        if (SOModified)
                            meshRendererComponentSO.ApplyModifiedProperties();
                    }
                }
                //*/

#if UNITY_2019_2_OR_NEWER
                var receiveGI = owner.ReceiveGI;
                if (meshRendererComponent &&
                    meshRendererComponent.receiveGI != receiveGI)
                {
                    meshRendererComponent.receiveGI = receiveGI;
                    instance.Dirty = true;
                }
#endif

                if (meshRendererComponent &&
                    meshRendererComponent.sharedMaterial != requiredMaterial)
                {
                    meshRendererComponent.sharedMaterial = requiredMaterial;
                    instance.Dirty = true;
                }

            }
            else
            {
                if (meshFilterComponent)
                {
                    meshFilterComponent.hideFlags = HideFlags.None;
                    UnityEngine.Object.DestroyImmediate(meshFilterComponent);
                    instance.Dirty = true;
                }
                if (meshRendererComponent)
                {
                    meshRendererComponent.hideFlags = HideFlags.None;
                    UnityEngine.Object.DestroyImmediate(meshRendererComponent);
                    instance.Dirty = true;
                }
                instance.LightingHashValue = instance.MeshDescription.geometryHashValue;
                meshFilterComponent = null;
                meshRendererComponent = null;
                instance.CachedMeshRendererSO = null;
            }

            instance.CachedMeshFilter = meshFilterComponent;
            instance.CachedMeshRenderer = meshRendererComponent;

            // TODO:	navmesh specific mesh
            // TODO:	occludee/reflection probe static

            var meshColliderComponent = instance.CachedMeshCollider;
            var isTrigger = instance.RenderSurfaceType == RenderSurfaceType.Trigger || owner.IsTrigger;
            var needMeshCollider = NeedCollider(instance);
            var hasCompoundCollider = owner.Settings.HasFlag(ModelSettingsFlags.GenerateCompoundCollider);
            var generateCompoundCollider = needMeshCollider
                && hasCompoundCollider
                && (instance.Dirty || !instance.CachedCompoundContainer);

            if (needMeshCollider)
            {
                if (!meshColliderComponent)
                {
                    meshColliderComponent = gameObject.GetComponent<MeshCollider>();
                }

                if (meshColliderComponent && !meshColliderComponent.enabled)
                {
                    meshColliderComponent.enabled = true;
                }

                if (!meshColliderComponent)
                {
                    meshColliderComponent = gameObject.AddComponent<MeshCollider>();
                    meshColliderComponent.gameObject.name = ColliderMeshInstanceName;
                    instance.Dirty = true;
                }

                // stops it from rendering wireframe in scene
                if ((meshColliderComponent.hideFlags & HideFlags.HideInHierarchy) == 0)
                {
                    meshColliderComponent.hideFlags |= HideFlags.HideInHierarchy;
                }

                var currentPhyicsMaterial = instance.PhysicsMaterial ?? owner.DefaultPhysicsMaterial;
                if (meshColliderComponent.sharedMaterial != currentPhyicsMaterial)
                {
                    meshColliderComponent.sharedMaterial = currentPhyicsMaterial;
                    instance.Dirty = true;
                }

                var setToConvex = owner.SetColliderConvex;
                if (meshColliderComponent.convex != setToConvex)
                {
                    meshColliderComponent.convex = setToConvex;
                    instance.Dirty = true;
                }

#if UNITY_2017_3_OR_NEWER
                var cookingOptions = owner.MeshColliderCookingOptions;
                if (meshColliderComponent.cookingOptions != cookingOptions)
                {
                    meshColliderComponent.cookingOptions = cookingOptions;
                    instance.Dirty = true;
                }
#endif

                if (meshColliderComponent.isTrigger != isTrigger)
                {
                    meshColliderComponent.isTrigger = isTrigger;
                    instance.Dirty = true;
                }

                if (meshColliderComponent.sharedMesh != instance.SharedMesh)
                {
                    meshColliderComponent.sharedMesh = instance.SharedMesh;
                }

                // .. for some reason this fixes mesh-colliders not being found with ray-casts in the editor?
#if UNITY_EDITOR
                if (instance.Dirty)
                {
                    meshColliderComponent.enabled = false;
                    meshColliderComponent.enabled = true;
                }
#endif
            }
            else
            {
                if (meshColliderComponent)
                {
                    meshColliderComponent.hideFlags = HideFlags.None;
                    UnityEngine.Object.DestroyImmediate(meshColliderComponent);
                    instance.Dirty = true;
                }
                meshColliderComponent = null;
            }
            instance.CachedMeshCollider = meshColliderComponent;


            if (generateCompoundCollider)
            {
                GenerateCompoundCollider(instance, owner, isTrigger);
            }
            else if (!hasCompoundCollider && instance.CachedCompoundContainer)
            {
                GameObject.DestroyImmediate(instance.CachedCompoundContainer);
            }

            if (hasCompoundCollider && meshColliderComponent)
            {
                meshColliderComponent.enabled = false;
            }

            if (!postProcessScene)
            {
#if SHOW_GENERATED_MESHES
				if (instance.Dirty)
					UpdateName(instance);
#else
                /*
				if (needMeshRenderer)
				{
					if (instance.name != RenderMeshInstanceName)
						instance.name = RenderMeshInstanceName;
				} else
				if (needMeshCollider)
				{
					if (instance.name != ColliderMeshInstanceName)
						instance.name = ColliderMeshInstanceName;
				}
				*/
#endif
                instance.Dirty = false;
            }
        }

        private static void DestroyCompoundCollider(GeneratedMeshInstance instance)
        {
            foreach (Transform tr in instance.gameObject.transform)
            {
                if (tr.name == "Compound Collider")
                {
                    GameObject.DestroyImmediate(tr.gameObject);
                }
            }
        }

        private static void GenerateCompoundCollider(GeneratedMeshInstance instance, CSGModel owner, bool isTrigger)
        {
            DestroyCompoundCollider(instance);

            instance.CachedCompoundContainer = new GameObject("Compound Collider");
            instance.CachedCompoundContainer.transform.SetParent(instance.gameObject.transform);

            foreach (var brush in owner.GetComponentsInChildren<CSGBrush>())
            {
                if (brush.OperationType != CSGOperationType.Additive)
                {
                    continue;
                }

                var obj = new GameObject("Convex Solid");
                obj.transform.SetParent(instance.CachedCompoundContainer.transform);
                obj.transform.position = brush.transform.position;
                obj.transform.rotation = brush.transform.rotation;
                obj.transform.localScale = brush.transform.localScale;

                var mf = obj.AddComponent<MeshFilter>();
                mf.sharedMesh = new Mesh();

                var controlState = new ControlMeshState(brush);
                var points = new List<Vector3>();
                controlState.UpdatePoints(brush.ControlMesh);
                controlState.UpdateMesh(brush.ControlMesh);

                foreach (var p in controlState.PolygonPointIndices)
                {
                    foreach (var i in p)
                    {
                        points.Add(brush.ControlMesh.Vertices[i]);
                        //indices.Add(i);
                        //verts.Add(brush.ControlMesh.Vertices[i]);
                    }
                }

                if (points.Count < 4)
                {
                    continue;
                }

                var tris = new List<int>();
                var verts = new List<Vector3>();
                var normals = new List<Vector3>();

                new GK.ConvexHullCalculator().GenerateHull(points, false, ref verts, ref tris, ref normals);

                mf.sharedMesh.SetVertices(verts);
                mf.sharedMesh.SetTriangles(tris, 0);
                mf.sharedMesh.SetNormals(normals);
                mf.sharedMesh.RecalculateBounds();

                var collider = obj.AddComponent<MeshCollider>();
                collider.convex = true;
                collider.isTrigger = isTrigger;
            }
        }

#if SHOW_GENERATED_MESHES
		private static void UpdateName(GeneratedMeshInstance instance)
		{
			var renderMaterial			= instance.RenderMaterial;
			var parentObject			= instance.gameObject;

			var builder = new System.Text.StringBuilder();
			builder.Append(instance.RenderSurfaceType);
			builder.Append(' ');
			builder.Append(instance.GetInstanceID());

			if (instance.PhysicsMaterial)
			{
				var physicmaterialName = ((!instance.PhysicsMaterial) ? "default" : instance.PhysicsMaterial.name);
				if (builder.Length > 0) builder.Append(' ');
				builder.AppendFormat(" Physics [{0}]", physicmaterialName);
			}
			if (renderMaterial)
			{
				builder.AppendFormat(" Material [{0} {1}]", renderMaterial.name, renderMaterial.GetInstanceID());
			}

			//builder.AppendFormat(" Key {0}", instance.GenerateKey().GetHashCode());

			var objectName = builder.ToString();
			if (parentObject.name != objectName) parentObject.name = objectName;
			if (instance.SharedMesh &&
				instance.SharedMesh.name != objectName)
				instance.SharedMesh.name = objectName;
		}
#endif

        static int RefreshModelCounter = 0;

        public static void UpdateHelperSurfaceVisibility(bool force = false)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            //			updateMeshColliderMeshTime = 0.0;
            var models = InternalCSGModelManager.Models;
            var currentRefreshModelCount = 0;
            for (var i = 0; i < models.Length; i++)
            {
                var model = models[i];
                if (!ModelTraits.IsModelEditable(model))
                    continue;

                var generatedMeshes = model.generatedMeshes;
                if (!generatedMeshes || generatedMeshes.owner != model)
                    continue;

                if (force ||
                    RefreshModelCounter == currentRefreshModelCount)
                {
                    UpdateContainerFlags(generatedMeshes);
                    foreach (var instance in generatedMeshes.MeshInstances)
                    {
                        if (!instance)
                        {
                            ValidateGeneratedMeshesDelayed(generatedMeshes);
                            continue;
                        }

                        Refresh(instance, generatedMeshes.owner, onlyFastRefreshes: !force, skipAssetDatabaseUpdate: false);
                    }
                }
                currentRefreshModelCount++;
            }

            if (RefreshModelCounter < currentRefreshModelCount)
                RefreshModelCounter++;
            else
                RefreshModelCounter = 0;

            UpdateHelperSurfaces();
        }

        private static void AssignLayerToChildren(GameObject gameObject)
        {
            if (!gameObject)
                return;
            var layer = gameObject.layer;
            foreach (var transform in gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (transform.GetComponent<CSGNode>())
                    transform.gameObject.layer = layer;
            }
        }

        public static void UpdateGeneratedMeshesVisibility(CSGModel model)
        {
            if (!model.generatedMeshes)
                return;

            UpdateGeneratedMeshesVisibility(model.generatedMeshes, model.ShowGeneratedMeshes);
        }

        public static void UpdateGeneratedMeshesVisibility(GeneratedMeshes container, bool visible)
        {
            if (!ModelTraits.IsModelEditable(container.owner) ||
                ModelTraits.IsDefaultModel(container.owner))
                return;



            var containerGameObject = container.gameObject;

            HideFlags gameObjectFlags;
            HideFlags transformFlags;
#if SHOW_GENERATED_MESHES
			gameObjectFlags = HideFlags.None;
#else
            if (visible)
            {
                gameObjectFlags = HideFlags.HideInInspector;
            }
            else
            {
                gameObjectFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
#endif
            transformFlags = gameObjectFlags | HideFlags.NotEditable;

            if (containerGameObject.hideFlags != gameObjectFlags)
            {
                containerGameObject.hideFlags = gameObjectFlags;
            }

            if (container.transform.hideFlags != transformFlags)
            {
                container.transform.hideFlags = transformFlags;
                container.hideFlags = transformFlags | ComponentHideFlags;
            }
        }

        static void AutoUpdateRigidBody(GeneratedMeshes container)
        {
            var model = container.owner;
            var gameObject = model.gameObject;
            if (ModelTraits.NeedsRigidBody(model))
            {
                var rigidBody = container.CachedRigidBody;
                if (!rigidBody)
                    rigidBody = model.GetComponent<Rigidbody>();
                if (!rigidBody)
                    rigidBody = gameObject.AddComponent<Rigidbody>();

                if (rigidBody.hideFlags != HideFlags.None)
                {
                    rigidBody.hideFlags = HideFlags.None;
                }

                RigidbodyConstraints constraints;
                bool isKinematic;
                bool useGravity;
                if (ModelTraits.NeedsStaticRigidBody(model))
                {
                    isKinematic = true;
                    useGravity = false;
                    constraints = RigidbodyConstraints.FreezeAll;
                }
                else
                {
                    isKinematic = false;
                    useGravity = true;
                    constraints = RigidbodyConstraints.None;
                }

                if (rigidBody.isKinematic != isKinematic) rigidBody.isKinematic = isKinematic;
                if (rigidBody.useGravity != useGravity) rigidBody.useGravity = useGravity;
                if (rigidBody.constraints != constraints) rigidBody.constraints = constraints;
                container.CachedRigidBody = rigidBody;
            }
            else
            {
                var rigidBody = container.CachedRigidBody;
                if (!rigidBody)
                    rigidBody = model.GetComponent<Rigidbody>();
                if (rigidBody)
                {
                    rigidBody.hideFlags = HideFlags.None;
                    UnityEngine.Object.DestroyImmediate(rigidBody);
                }
                container.CachedRigidBody = null;
            }
        }

        public static void RemoveIfEmpty(GameObject gameObject)
        {
            var allComponents = gameObject.GetComponents<Component>();
            for (var i = 0; i < allComponents.Length; i++)
            {
                if (allComponents[i] is Transform)
                    continue;
                if (allComponents[i] is GeneratedMeshInstance)
                    continue;

                return;
            }
            GameObjectExtensions.Destroy(gameObject);
        }

        static readonly List<GeneratedMeshInstance> s_foundMeshInstances = new List<GeneratedMeshInstance>();


        public static bool ValidateGeneratedMeshesNow(GeneratedMeshes generatedMeshes, bool skipSiblingCheck = false)
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return false;

            if (!generatedMeshes)
                return true;

            if (generatedMeshes.owner && generatedMeshes.gameObject)
            {
                if (!skipSiblingCheck)
                {
                    ValidateModelNow(generatedMeshes.owner, true);
                    if (!generatedMeshes)
                        return true;
                }
            }
            else
            {
                GameObjectExtensions.Destroy(generatedMeshes.gameObject);
                return true;
            }

            var generatedMeshesGameObject = generatedMeshes.gameObject;
            var generatedMeshesTransform = generatedMeshesGameObject.transform;

            s_foundMeshInstances.Clear();
            for (var i = 0; i < generatedMeshesTransform.childCount; i++)
            {
                var meshInstanceTransform = generatedMeshesTransform.GetChild(i);
                var meshInstance = meshInstanceTransform.GetComponent<GeneratedMeshInstance>();
                if (!meshInstance)
                {
                    if (meshInstanceTransform.gameObject &&
                        meshInstanceTransform.hideFlags != HideFlags.DontSave)
                        GameObjectExtensions.Destroy(meshInstanceTransform.gameObject);
                    continue;
                }
                var key = meshInstance.GenerateKey();
                if (!generatedMeshes.HasMeshInstance(key))
                {
                    GameObjectExtensions.Destroy(meshInstanceTransform.gameObject);
                    continue;
                }

                /*
				if (meshInstance.RenderSurfaceType == RenderSurfaceType.Normal && !meshInstance.RenderMaterial)
				{
					GameObjectExtensions.Destroy(meshInstanceTransform.gameObject);
					continue; 
				}
				*/
                if (!ValidMeshInstance(meshInstance))
                {
                    GameObjectExtensions.Destroy(meshInstanceTransform.gameObject);
                    continue;
                }

                s_foundMeshInstances.Add(meshInstance);
            }

            generatedMeshes.SetMeshInstances(s_foundMeshInstances);

            if (string.IsNullOrEmpty(generatedMeshesGameObject.name))
            {
                var flags = generatedMeshesGameObject.hideFlags;

                if (generatedMeshesGameObject.hideFlags != HideFlags.None)
                {
                    generatedMeshesGameObject.hideFlags = HideFlags.None;
                }

                generatedMeshesGameObject.name = MeshContainerName;

                if (generatedMeshesGameObject.hideFlags != flags)
                {
                    generatedMeshesGameObject.hideFlags = flags;
                }
            }

            if (generatedMeshes.owner)
                UpdateGeneratedMeshesVisibility(generatedMeshes, generatedMeshes.owner.ShowGeneratedMeshes);

            if (generatedMeshes.owner)
            {
                var modelTransform = generatedMeshes.owner.transform;
                if (generatedMeshesTransform.parent != modelTransform)
                    generatedMeshesTransform.parent.SetParent(modelTransform, true);
            }
            return false;
        }

        public static GeneratedMeshInstance[] GetAllModelMeshInstances(GeneratedMeshes container)
        {
            if (!container.HasMeshInstances)
                return null;

            return container.MeshInstances;
        }

        public static GeneratedMeshInstance GetMeshInstance(GeneratedMeshes container, GeneratedMeshDescription meshDescription, ModelSettingsFlags modelSettings, RenderSurfaceType renderSurfaceType)
        {
            var key = MeshInstanceKey.GenerateKey(meshDescription);
            var instance = container.GetMeshInstance(key);
            if (instance && instance.SharedMesh)
                return instance;
            return null;
        }

        public static HelperSurfaceDescription GetHelperSurfaceDescription(GeneratedMeshes container, ModelSettingsFlags modelSettings, GeneratedMeshDescription meshDescription, RenderSurfaceType renderSurfaceType)
        {
            var key = MeshInstanceKey.GenerateKey(meshDescription);
            HelperSurfaceDescription instance = container.GetHelperSurface(key);
            if (instance != null)
            {
                instance.RenderSurfaceType = renderSurfaceType;
                return instance;
            }

            return CreateHelperSurfaceDescription(container, modelSettings, meshDescription, renderSurfaceType);
        }

        public static bool IsObjectGenerated(UnityEngine.Object obj)
        {
            if (!obj)
                return false;

            var gameObject = obj as GameObject;
            if (Equals(gameObject, null))
                return false;

            if (gameObject.name == MeshContainerName)
                return true;

            var parent = gameObject.transform.parent;
            if (Equals(parent, null))
                return false;

            return parent.name == MeshContainerName;
        }

        #region UpdateTransform
        public static void UpdateTransforms()
        {
            var models = InternalCSGModelManager.Models;
            for (var i = 0; i < models.Length; i++)
            {
                var model = models[i];
                if (!ModelTraits.IsModelEditable(model))
                    continue;

                UpdateTransform(model.generatedMeshes);
            }
        }

        static void UpdateTransform(GeneratedMeshes container)
        {
            if (!container || !container.owner)
            {
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            // TODO: make sure outlines are updated when models move

            var containerTransform = container.transform;
            if (containerTransform.localPosition != MathConstants.zeroVector3 ||
                containerTransform.localRotation != MathConstants.identityQuaternion ||
                containerTransform.localScale != MathConstants.oneVector3)
            {
                containerTransform.localPosition = MathConstants.zeroVector3;
                containerTransform.localRotation = MathConstants.identityQuaternion;
                containerTransform.localScale = MathConstants.oneVector3;
                SceneToolRenderer.SetOutlineDirty();
            }
        }
        #endregion

        #region UpdateContainerComponents
        static readonly List<GameObject> __notfoundGameObjects = new List<GameObject>();
        public static List<GameObject> FindUnusedMeshInstances(GeneratedMeshes container,
                                                               HashSet<GeneratedMeshInstance> foundInstances)
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return null;

            if (!container || !container.owner)
                return null;

            __notfoundGameObjects.Clear();
            if (container.HasMeshInstances)
            {
                var instances = container.GetComponentsInChildren<GeneratedMeshInstance>(true);
                if (foundInstances == null)
                {
                    for (int i = 0; i < instances.Length; i++)
                        __notfoundGameObjects.Add(instances[i].gameObject);
                }
                else
                {
                    for (int i = 0; i < instances.Length; i++)
                    {
                        var instance = instances[i];
                        if (!foundInstances.Contains(instance))
                        {
                            __notfoundGameObjects.Add(instance.gameObject);
                            continue;
                        }
                    }
                }
            }

            var generatedMeshesTransform = container.transform;
            for (int i = 0; i < generatedMeshesTransform.childCount; i++)
            {
                var childTransform = generatedMeshesTransform.GetChild(i);
                var childGameObject = childTransform.gameObject;

                if (childGameObject.activeSelf)
                    continue;

                __notfoundGameObjects.Add(childGameObject);
            }
            return __notfoundGameObjects;
        }

        static readonly List<GeneratedMeshInstance> __notfoundInstances = new List<GeneratedMeshInstance>();
        static MeshInstanceKey[] __removeMeshInstances = new MeshInstanceKey[0];

        public static void UpdateContainerComponents(GeneratedMeshes container,
                                                     HashSet<GeneratedMeshInstance> foundInstances,
                                                     HashSet<HelperSurfaceDescription> foundHelperSurfaces)
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (!container || !container.owner)
                return;

            if (!container.HasMeshInstances)
            {
                var prevModel = container ? container.owner : null;
                if (ValidateGeneratedMeshesNow(container) && prevModel)
                    prevModel.forceUpdate = true;
            }

            var oldMeshes = new HashSet<Mesh>();
            foreach (var helperSurface in container.HelperSurfaces)
            {
                if (helperSurface.SharedMesh) oldMeshes.Add(helperSurface.SharedMesh);
            }
            foreach (var meshInstance in container.MeshInstances)
            {
                if (meshInstance.SharedMesh) oldMeshes.Add(meshInstance.SharedMesh);
            }

            var newMeshes = new HashSet<Mesh>();
            foreach (var helperSurface in foundHelperSurfaces)
            {
                if (!helperSurface.SharedMesh)
                    continue;
                newMeshes.Add(helperSurface.SharedMesh);
            }
            foreach (var meshInstance in foundInstances)
            {
                if (!meshInstance.SharedMesh)
                    continue;
                newMeshes.Add(meshInstance.SharedMesh);
            }

            container.SetHelperSurfaces(foundHelperSurfaces);
            container.SetMeshInstances(foundInstances);

            __notfoundInstances.Clear();
            var instances = container.GetComponentsInChildren<GeneratedMeshInstance>(true);
            if (foundInstances == null)
            {
                __notfoundInstances.AddRange(instances);
            }
            else
            {
                for (int i = 0; i < instances.Length; i++)
                {
                    var instance = instances[i];
                    if (!foundInstances.Contains(instance))
                    {
                        __notfoundInstances.Add(instance);
                        continue;
                    }

                    container.AddMeshInstance(instance);
                }
            }

            for (int i = 0; i < __notfoundInstances.Count; i++)
            {
                var meshInstance = __notfoundInstances[i];
                if (meshInstance && meshInstance.gameObject)
                {
                    //var key = meshInstance.GenerateKey();
                    //var keyObj = EditorUtility.InstanceIDToObject(key.SurfaceParameter);
                    GameObjectExtensions.Destroy(meshInstance.gameObject);
                }

                if (__removeMeshInstances.Length < container.MeshInstances.Length)
                {
                    __removeMeshInstances = new MeshInstanceKey[container.MeshInstances.Length];
                }

                int removeMeshInstancesCount = 0;
                foreach (var item in container.MeshInstances)
                {
                    if (!item ||
                        item == meshInstance)
                    {
                        __removeMeshInstances[removeMeshInstancesCount] = item.GenerateKey();
                        removeMeshInstancesCount++;
                    }
                }
                if (removeMeshInstancesCount > 0)
                {
                    container.RemoveMeshInstances(__removeMeshInstances, removeMeshInstancesCount);
                }
            }

            if (!container.owner)
                return;

            UpdateTransform(container);
        }
        #endregion

        #region UpdateContainerFlags
        private static void UpdateContainerFlags(GeneratedMeshes generatedMeshes)
        {
            if (!generatedMeshes)
                return;
            if (generatedMeshes.owner)
            {
                if (CSGPrefabUtility.IsPrefabAsset(generatedMeshes.gameObject) ||
                    CSGPrefabUtility.IsPrefabAsset(generatedMeshes.owner.gameObject))
                    return;

                if (!CSGPrefabUtility.IsPrefabInstance(generatedMeshes.gameObject) &&
                    !CSGPrefabUtility.IsPrefabInstance(generatedMeshes.owner.gameObject))
                {
                    var ownerTransform = generatedMeshes.owner.transform;
                    if (generatedMeshes.transform.parent != ownerTransform)
                    {
                        generatedMeshes.transform.SetParent(ownerTransform, false);
                    }

                    if (!generatedMeshes)
                        return;
                }
            }

            //var isTrigger			= container.owner.IsTrigger;
            //var collidable		= container.owner.HaveCollider || isTrigger;
            var ownerGameObject = generatedMeshes.owner.gameObject;
            var ownerStaticFlags = GameObjectUtility.GetStaticEditorFlags(ownerGameObject);
            var previousStaticFlags = GameObjectUtility.GetStaticEditorFlags(generatedMeshes.gameObject);

            var containerLayer = ownerGameObject.layer;

            var showVisibleSurfaces = (RealtimeCSG.CSGSettings.VisibleHelperSurfaces & HelperSurfaceFlags.ShowVisibleSurfaces) != 0;


            if (ownerStaticFlags != previousStaticFlags ||
                !ownerGameObject.CompareTag(generatedMeshes.gameObject.tag) ||
                containerLayer != generatedMeshes.gameObject.layer)
            {
                var containerTag = ownerGameObject.tag;
                foreach (var meshInstance in generatedMeshes.MeshInstances)
                {
                    if (!meshInstance)
                        continue;

                    if (meshInstance.RenderSurfaceType == RenderSurfaceType.Normal)
                        CSGVisibilityUtility.SetGameObjectVisibility(meshInstance.gameObject, showVisibleSurfaces);

                    var oldStaticFlags = GameObjectUtility.GetStaticEditorFlags(meshInstance.gameObject);
                    var newStaticFlags = FilterStaticEditorFlags(oldStaticFlags, meshInstance.RenderSurfaceType);

                    foreach (var transform in meshInstance.GetComponentsInChildren<Transform>(true))
                    {
                        var gameObject = transform.gameObject;
                        if (oldStaticFlags != newStaticFlags)
                            GameObjectUtility.SetStaticEditorFlags(gameObject, newStaticFlags);
                        if (!gameObject.CompareTag(containerTag))
                            gameObject.tag = containerTag;
                        if (gameObject.layer != containerLayer)
                            gameObject.layer = containerLayer;
                    }
                }
            }

            if (generatedMeshes.owner.NeedAutoUpdateRigidBody)
                AutoUpdateRigidBody(generatedMeshes);
        }
        #endregion
    }
}




/**
 * Copyright 2019 Oskar Sigvardsson
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

//#define DEBUG_QUICKHULL

namespace GK
{
    /// <summary>
    ///   An implementation of the quickhull algorithm for generating 3d convex
    ///   hulls.
    ///
    ///   The algorithm works like this: you start with an initial "seed" hull,
    ///   that is just a simple tetrahedron made up of four points in the point
    ///   cloud. This seed hull is then grown until it all the points in the
    ///   point cloud is inside of it, at which point it will be the convex hull
    ///   for the entire set.
    ///
    ///   All of the points in the point cloud is divided into two parts, the
    ///   "open set" and the "closed set". The open set consists of all the
    ///   points outside of the tetrahedron, and the closed set is all of the
    ///   points inside the tetrahedron. After each iteration of the algorithm,
    ///   the closed set gets bigger and the open set get smaller. When the open
    ///   set is empty, the algorithm is finished.
    ///
    ///   Each point in the open set is assigned to a face that it lies outside
    ///   of. To grow the hull, the point in the open set which is farthest from
    ///   it's face is chosen. All faces which are facing that point (I call
    ///   them "lit faces" in the code, because if you imagine the point as a
    ///   point light, it's the set of points which would be lit by that point
    ///   light) are removed, and a "horizon" of edges is found from where the
    ///   faces were removed. From this horizon, new faces are constructed in a
    ///   "cone" like fashion connecting the point to the edges.
    ///
    ///   To keep track of the faces, I use a struct for each face which
    ///   contains the three vertices of the face in CCW order, as well as the
    ///   three triangles which share an edge. I was considering doing a
    ///   half-edge structure to store the mesh, but it's not needed. Using a
    ///   struct for each face and neighbors simplify the algorithm and makes it
    ///   easy to export it as a mesh.
    ///
    ///   The most subtle part of the algorithm is finding the horizon. In order
    ///   to properly construct the cone so that all neighbors are kept
    ///   consistent, you can do a depth-first search from the first lit face.
    ///   If the depth-first search always proceeeds in a counter-clockwise
    ///   fashion, it guarantees that the horizon will be found in a
    ///   counter-clockwise order, which makes it easy to construct the cone of
    ///   new faces.
    ///
    ///   A note: the code uses a right-handed coordinate system, where the
    ///   cross-product uses the right-hand rule and the faces are in CCW order.
    ///   At the end of the algorithm, the hull is exported in a Unity-friendly
    ///   fashion, with a left-handed mesh.
    /// </summary>
    public class ConvexHullCalculator
    {

        /// <summary>
        ///   Constant representing a point that has yet to be assigned to a
        ///   face. It's only used immediately after constructing the seed hull.
        /// </summary>
        const int UNASSIGNED = -2;

        /// <summary>
        ///   Constant representing a point that is inside the convex hull, and
        ///   thus is behind all faces. In the openSet array, all points with
        ///   INSIDE are at the end of the array, with indexes larger
        ///   openSetTail.
        /// </summary>
        const int INSIDE = -1;

        /// <summary>
        ///   Epsilon value. If the coordinates of the point space are
        ///   exceptionally close to each other, this value might need to be
        ///   adjusted.
        /// </summary>
        const float EPSILON = 0.0001f;

        /// <summary>
        ///   Struct representing a single face.
        ///
        ///   Vertex0, Vertex1 and Vertex2 are the vertices in CCW order. They
        ///   acutal points are stored in the points array, these are just
        ///   indexes into that array.
        ///
        ///   Opposite0, Opposite1 and Opposite2 are the keys to the faces which
        ///   share an edge with this face. Opposite0 is the face opposite
        ///   Vertex0 (so it has an edge with Vertex2 and Vertex1), etc.
        ///
        ///   Normal is (unsurprisingly) the normal of the triangle.
        /// </summary>
        struct Face
        {
            public int Vertex0;
            public int Vertex1;
            public int Vertex2;

            public int Opposite0;
            public int Opposite1;
            public int Opposite2;

            public Vector3 Normal;

            public Face(int v0, int v1, int v2, int o0, int o1, int o2, Vector3 normal)
            {
                Vertex0 = v0;
                Vertex1 = v1;
                Vertex2 = v2;
                Opposite0 = o0;
                Opposite1 = o1;
                Opposite2 = o2;
                Normal = normal;
            }

            public bool Equals(Face other)
            {
                return (this.Vertex0 == other.Vertex0)
                    && (this.Vertex1 == other.Vertex1)
                    && (this.Vertex2 == other.Vertex2)
                    && (this.Opposite0 == other.Opposite0)
                    && (this.Opposite1 == other.Opposite1)
                    && (this.Opposite2 == other.Opposite2)
                    && (this.Normal == other.Normal);
            }
        }

        /// <summary>
        ///   Struct representing a mapping between a point and a face. These
        ///   are used in the openSet array.
        ///
        ///   Point is the index of the point in the points array, Face is the
        ///   key of the face in the Key dictionary, Distance is the distance
        ///   from the face to the point.
        /// </summary>
        struct PointFace
        {
            public int Point;
            public int Face;
            public float Distance;

            public PointFace(int p, int f, float d)
            {
                Point = p;
                Face = f;
                Distance = d;
            }
        }

        /// <summary>
        ///   Struct representing a single edge in the horizon.
        ///
        ///   Edge0 and Edge1 are the vertexes of edge in CCW order, Face is the
        ///   face on the other side of the horizon.
        ///
        ///   TODO Edge1 isn't actually needed, you can just index the next item
        ///   in the horizon array.
        /// </summary>
        struct HorizonEdge
        {
            public int Face;
            public int Edge0;
            public int Edge1;
        }

        /// <summary>
        ///   A dictionary storing the faces of the currently generated convex
        ///   hull. The key is the id of the face, used in the Face, PointFace
        ///   and HorizonEdge struct.
        ///
        ///   This is a Dictionary, because we need both random access to it,
        ///   the ability to loop through it, and ability to quickly delete
        ///   faces (in the ConstructCone method), and Dictionary is the obvious
        ///   candidate that can do all of those things.
        ///
        ///   I'm wondering if using a Dictionary is best idea, though. It might
        ///   be better to just have them in a List<Face> and mark a face as
        ///   deleted by adding a field to the Face struct. The downside is that
        ///   we would need an extra field in the Face struct, and when we're
        ///   looping through the points in openSet, we would have to loop
        ///   through all the Faces EVER created in the algorithm, and skip the
        ///   ones that have been marked as deleted. However, looping through a
        ///   list is fairly fast, and it might be worth it to avoid Dictionary
        ///   overhead.
        ///
        ///   TODO test converting to a List<Face> instead.
        /// </summary>
        Dictionary<int, Face> faces;

        /// <summary>
        ///   The set of points to be processed. "openSet" is a misleading name,
        ///   because it's both the open set (points which are still outside the
        ///   convex hull) and the closed set (points that are inside the convex
        ///   hull). The first part of the array (with indexes <= openSetTail)
        ///   is the openSet, the last part of the array (with indexes >
        ///   openSetTail) are the closed set, with Face set to INSIDE. The
        ///   closed set is largely irrelevant to the algorithm, the open set is
        ///   what matters.
        ///
        ///   Storing the entire open set in one big list has a downside: when
        ///   we're reassigning points after ConstructCone, we only need to
        ///   reassign points that belong to the faces that have been removed,
        ///   but storing it in one array, we have to loop through the entire
        ///   list, and checking litFaces to determine which we can skip and
        ///   which need to be reassigned.
        ///
        ///   The alternative here is to give each face in Face array it's own
        ///   openSet. I don't like that solution, because then you have to
        ///   juggle so many more heap-allocated List<T>'s, we'd have to use
        ///   object pools and such. It would do a lot more allocation, and it
        ///   would have worse locality. I should maybe test that solution, but
        ///   it probably wont be faster enough (if at all) to justify the extra
        ///   allocations.
        /// </summary>
        List<PointFace> openSet;

        /// <summary>
        ///   Set of faces which are "lit" by the current point in the set. This
        ///   is used in the FindHorizon() DFS search to keep track of which
        ///   faces we've already visited, and in the ReassignPoints() method to
        ///   know which points need to be reassigned.
        /// </summary>
        HashSet<int> litFaces;

        /// <summary>
        ///   The current horizon. Generated by the FindHorizon() DFS search,
        ///   and used in ConstructCone to construct new faces. The list of
        ///   edges are in CCW order.
        /// </summary>
        List<HorizonEdge> horizon;

        /// <summary>
        ///   If SplitVerts is false, this Dictionary is used to keep track of
        ///   which points we've added to the final mesh.
        /// </summary>
        Dictionary<int, int> hullVerts;

        /// <summary>
        ///   The "tail" of the openSet, the last index of a vertex that has
        ///   been assigned to a face.
        /// </summary>
        int openSetTail = -1;

        /// <summary>
        ///   When adding a new face to the faces Dictionary, use this for the
        ///   key and then increment it.
        /// </summary>
        int faceCount = 0;

        /// <summary>
        ///   Generate a convex hull from points in points array, and store the
        ///   mesh in Unity-friendly format in verts and tris. If splitVerts is
        ///   true, the the verts will be split, if false, the same vert will be
        ///   used for more than one triangle.
        /// </summary>
        public void GenerateHull(
            List<Vector3> points,
            bool splitVerts,
            ref List<Vector3> verts,
            ref List<int> tris,
            ref List<Vector3> normals)
        {
            if (points.Count < 4)
            {
                throw new System.ArgumentException("Need at least 4 points to generate a convex hull");
            }

            Initialize(points, splitVerts);

            GenerateInitialHull(points);

            while (openSetTail >= 0)
            {
                GrowHull(points);
            }

            ExportMesh(points, splitVerts, ref verts, ref tris, ref normals);
            VerifyMesh(points, ref verts, ref tris);
        }

        /// <summary>
        ///   Make sure all the buffers and variables needed for the algorithm
        ///   are initialized.
        /// </summary>
        void Initialize(List<Vector3> points, bool splitVerts)
        {
            faceCount = 0;
            openSetTail = -1;

            if (faces == null)
            {
                faces = new Dictionary<int, Face>();
                litFaces = new HashSet<int>();
                horizon = new List<HorizonEdge>();
                openSet = new List<PointFace>(points.Count);
            }
            else
            {
                faces.Clear();
                litFaces.Clear();
                horizon.Clear();
                openSet.Clear();

                if (openSet.Capacity < points.Count)
                {
                    // i wonder if this is a good idea... if you call
                    // GenerateHull over and over with slightly increasing
                    // points counts, it's going to reallocate every time. Maybe
                    // i should just use .Add(), and let the List<T> manage the
                    // capacity, increasing it geometrically every time we need
                    // to reallocate.

                    // maybe do
                    //   openSet.Capacity = Mathf.NextPowerOfTwo(points.Count)
                    // instead?

                    openSet.Capacity = points.Count;
                }
            }

            if (!splitVerts)
            {
                if (hullVerts == null)
                {
                    hullVerts = new Dictionary<int, int>();
                }
                else
                {
                    hullVerts.Clear();
                }
            }
        }

        /// <summary>
        ///   Create initial seed hull.
        /// </summary>
        void GenerateInitialHull(List<Vector3> points)
        {
            // Find points suitable for use as the seed hull. Some varieties of
            // this algorithm pick extreme points here, but I'm not convinced
            // you gain all that much from that. Currently what it does is just
            // find the first four points that are not coplanar.
            int b0, b1, b2, b3;
            FindInitialHullIndices(points, out b0, out b1, out b2, out b3);

            var v0 = points[b0];
            var v1 = points[b1];
            var v2 = points[b2];
            var v3 = points[b3];

            var above = Dot(v3 - v1, Cross(v1 - v0, v2 - v0)) > 0.0f;

            // Create the faces of the seed hull. You need to draw a diagram
            // here, otherwise it's impossible to know what's going on :)

            // Basically: there are two different possible start-tetrahedrons,
            // depending on whether the fourth point is above or below the base
            // triangle. If you draw a tetrahedron with these coordinates (in a
            // right-handed coordinate-system):

            //   b0 = (0,0,0)
            //   b1 = (1,0,0)
            //   b2 = (0,1,0)
            //   b3 = (0,0,1)

            // you can see the first case (set b3 = (0,0,-1) for the second
            // case). The faces are added with the proper references to the
            // faces opposite each vertex

            faceCount = 0;
            if (above)
            {
                faces[faceCount++] = new Face(b0, b2, b1, 3, 1, 2, Normal(points[b0], points[b2], points[b1]));
                faces[faceCount++] = new Face(b0, b1, b3, 3, 2, 0, Normal(points[b0], points[b1], points[b3]));
                faces[faceCount++] = new Face(b0, b3, b2, 3, 0, 1, Normal(points[b0], points[b3], points[b2]));
                faces[faceCount++] = new Face(b1, b2, b3, 2, 1, 0, Normal(points[b1], points[b2], points[b3]));
            }
            else
            {
                faces[faceCount++] = new Face(b0, b1, b2, 3, 2, 1, Normal(points[b0], points[b1], points[b2]));
                faces[faceCount++] = new Face(b0, b3, b1, 3, 0, 2, Normal(points[b0], points[b3], points[b1]));
                faces[faceCount++] = new Face(b0, b2, b3, 3, 1, 0, Normal(points[b0], points[b2], points[b3]));
                faces[faceCount++] = new Face(b1, b3, b2, 2, 0, 1, Normal(points[b1], points[b3], points[b2]));
            }

            VerifyFaces(points);

            // Create the openSet. Add all points except the points of the seed
            // hull.
            for (int i = 0; i < points.Count; i++)
            {
                if (i == b0 || i == b1 || i == b2 || i == b3) continue;

                openSet.Add(new PointFace(i, UNASSIGNED, 0.0f));
            }

            // Add the seed hull verts to the tail of the list.
            openSet.Add(new PointFace(b0, INSIDE, float.NaN));
            openSet.Add(new PointFace(b1, INSIDE, float.NaN));
            openSet.Add(new PointFace(b2, INSIDE, float.NaN));
            openSet.Add(new PointFace(b3, INSIDE, float.NaN));

            // Set the openSetTail value. Last item in the array is
            // openSet.Count - 1, but four of the points (the verts of the seed
            // hull) are part of the closed set, so move openSetTail to just
            // before those.
            openSetTail = openSet.Count - 5;

            Assert(openSet.Count == points.Count);

            // Assign all points of the open set. This does basically the same
            // thing as ReassignPoints()
            for (int i = 0; i <= openSetTail; i++)
            {
                Assert(openSet[i].Face == UNASSIGNED);
                Assert(openSet[openSetTail].Face == UNASSIGNED);
                Assert(openSet[openSetTail + 1].Face == INSIDE);

                var assigned = false;
                var fp = openSet[i];

                Assert(faces.Count == 4);
                Assert(faces.Count == faceCount);
                for (int j = 0; j < 4; j++)
                {
                    Assert(faces.ContainsKey(j));

                    var face = faces[j];

                    var dist = PointFaceDistance(points[fp.Point], points[face.Vertex0], face);

                    if (dist > 0)
                    {
                        fp.Face = j;
                        fp.Distance = dist;
                        openSet[i] = fp;

                        assigned = true;
                        break;
                    }
                }

                if (!assigned)
                {
                    // Point is inside
                    fp.Face = INSIDE;
                    fp.Distance = float.NaN;

                    // Point is inside seed hull: swap point with tail, and move
                    // openSetTail back. We also have to decrement i, because
                    // there's a new item at openSet[i], and we need to process
                    // it next iteration
                    openSet[i] = openSet[openSetTail];
                    openSet[openSetTail] = fp;

                    openSetTail -= 1;
                    i -= 1;
                }
            }

            VerifyOpenSet(points);
        }

        /// <summary>
        ///   Find four points in the point cloud that are not coplanar for the
        ///   seed hull
        /// </summary>
        void FindInitialHullIndices(List<Vector3> points, out int b0, out int b1, out int b2, out int b3)
        {
            var count = points.Count;

            for (int i0 = 0; i0 < count - 3; i0++)
            {
                for (int i1 = i0 + 1; i1 < count - 2; i1++)
                {
                    var p0 = points[i0];
                    var p1 = points[i1];

                    if (AreCoincident(p0, p1)) continue;

                    for (int i2 = i1 + 1; i2 < count - 1; i2++)
                    {
                        var p2 = points[i2];

                        if (AreCollinear(p0, p1, p2)) continue;

                        for (int i3 = i2 + 1; i3 < count - 0; i3++)
                        {
                            var p3 = points[i3];

                            if (AreCoplanar(p0, p1, p2, p3)) continue;

                            b0 = i0;
                            b1 = i1;
                            b2 = i2;
                            b3 = i3;
                            return;
                        }
                    }
                }
            }

            throw new System.ArgumentException("Can't generate hull, points are coplanar");
        }

        /// <summary>
        ///   Grow the hull. This method takes the current hull, and expands it
        ///   to encompass the point in openSet with the point furthest away
        ///   from its face.
        /// </summary>
        void GrowHull(List<Vector3> points)
        {
            Assert(openSetTail >= 0);
            Assert(openSet[0].Face != INSIDE);

            // Find farthest point and first lit face.
            var farthestPoint = 0;
            var dist = openSet[0].Distance;

            for (int i = 1; i <= openSetTail; i++)
            {
                if (openSet[i].Distance > dist)
                {
                    farthestPoint = i;
                    dist = openSet[i].Distance;
                }
            }

            // Use lit face to find horizon and the rest of the lit
            // faces.
            FindHorizon(
                points,
                points[openSet[farthestPoint].Point],
                openSet[farthestPoint].Face,
                faces[openSet[farthestPoint].Face]);

            VerifyHorizon();

            // Construct new cone from horizon
            ConstructCone(points, openSet[farthestPoint].Point);

            VerifyFaces(points);

            // Reassign points
            ReassignPoints(points);
        }

        /// <summary>
        ///   Start the search for the horizon.
        ///
        ///   The search is a DFS search that searches neighboring triangles in
        ///   a counter-clockwise fashion. When it find a neighbor which is not
        ///   lit, that edge will be a line on the horizon. If the search always
        ///   proceeds counter-clockwise, the edges of the horizon will be found
        ///   in counter-clockwise order.
        ///
        ///   The heart of the search can be found in the recursive
        ///   SearchHorizon() method, but the the first iteration of the search
        ///   is special, because it has to visit three neighbors (all the
        ///   neighbors of the initial triangle), while the rest of the search
        ///   only has to visit two (because one of them has already been
        ///   visited, the one you came from).
        /// </summary>
        void FindHorizon(List<Vector3> points, Vector3 point, int fi, Face face)
        {
            // TODO should I use epsilon in the PointFaceDistance comparisons?

            litFaces.Clear();
            horizon.Clear();

            litFaces.Add(fi);

            Assert(PointFaceDistance(point, points[face.Vertex0], face) > 0.0f);

            // For the rest of the recursive search calls, we first check if the
            // triangle has already been visited and is part of litFaces.
            // However, in this first call we can skip that because we know it
            // can't possibly have been visited yet, since the only thing in
            // litFaces is the current triangle.
            {
                var oppositeFace = faces[face.Opposite0];

                var dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite0,
                        Edge0 = face.Vertex1,
                        Edge1 = face.Vertex2,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite0, oppositeFace);
                }
            }

            if (!litFaces.Contains(face.Opposite1))
            {
                var oppositeFace = faces[face.Opposite1];

                var dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite1,
                        Edge0 = face.Vertex2,
                        Edge1 = face.Vertex0,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite1, oppositeFace);
                }
            }

            if (!litFaces.Contains(face.Opposite2))
            {
                var oppositeFace = faces[face.Opposite2];

                var dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = face.Opposite2,
                        Edge0 = face.Vertex0,
                        Edge1 = face.Vertex1,
                    });
                }
                else
                {
                    SearchHorizon(points, point, fi, face.Opposite2, oppositeFace);
                }
            }
        }

        /// <summary>
        ///   Recursively search to find the horizon or lit set.
        /// </summary>
        void SearchHorizon(List<Vector3> points, Vector3 point, int prevFaceIndex, int faceCount, Face face)
        {
            Assert(prevFaceIndex >= 0);
            Assert(litFaces.Contains(prevFaceIndex));
            Assert(!litFaces.Contains(faceCount));
            Assert(faces[faceCount].Equals(face));

            litFaces.Add(faceCount);

            // Use prevFaceIndex to determine what the next face to search will
            // be, and what edges we need to cross to get there. It's important
            // that the search proceeds in counter-clockwise order from the
            // previous face.
            int nextFaceIndex0;
            int nextFaceIndex1;
            int edge0;
            int edge1;
            int edge2;

            if (prevFaceIndex == face.Opposite0)
            {
                nextFaceIndex0 = face.Opposite1;
                nextFaceIndex1 = face.Opposite2;

                edge0 = face.Vertex2;
                edge1 = face.Vertex0;
                edge2 = face.Vertex1;
            }
            else if (prevFaceIndex == face.Opposite1)
            {
                nextFaceIndex0 = face.Opposite2;
                nextFaceIndex1 = face.Opposite0;

                edge0 = face.Vertex0;
                edge1 = face.Vertex1;
                edge2 = face.Vertex2;
            }
            else
            {
                Assert(prevFaceIndex == face.Opposite2);

                nextFaceIndex0 = face.Opposite0;
                nextFaceIndex1 = face.Opposite1;

                edge0 = face.Vertex1;
                edge1 = face.Vertex2;
                edge2 = face.Vertex0;
            }

            if (!litFaces.Contains(nextFaceIndex0))
            {
                var oppositeFace = faces[nextFaceIndex0];

                var dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex0,
                        Edge0 = edge0,
                        Edge1 = edge1,
                    });
                }
                else
                {
                    SearchHorizon(points, point, faceCount, nextFaceIndex0, oppositeFace);
                }
            }

            if (!litFaces.Contains(nextFaceIndex1))
            {
                var oppositeFace = faces[nextFaceIndex1];

                var dist = PointFaceDistance(
                    point,
                    points[oppositeFace.Vertex0],
                    oppositeFace);

                if (dist <= 0.0f)
                {
                    horizon.Add(new HorizonEdge
                    {
                        Face = nextFaceIndex1,
                        Edge0 = edge1,
                        Edge1 = edge2,
                    });
                }
                else
                {
                    SearchHorizon(points, point, faceCount, nextFaceIndex1, oppositeFace);
                }
            }
        }

        /// <summary>
        ///   Remove all lit faces and construct new faces from the horizon in a
        ///   "cone-like" fashion.
        ///
        ///   This is a relatively straight-forward procedure, given that the
        ///   horizon is handed to it in already sorted counter-clockwise. The
        ///   neighbors of the new faces are easy to find: they're the previous
        ///   and next faces to be constructed in the cone, as well as the face
        ///   on the other side of the horizon. We also have to update the face
        ///   on the other side of the horizon to reflect it's new neighbor from
        ///   the cone.
        /// </summary>
        void ConstructCone(List<Vector3> points, int farthestPoint)
        {
            foreach (var fi in litFaces)
            {
                Assert(faces.ContainsKey(fi));
                faces.Remove(fi);
            }

            var firstNewFace = faceCount;

            for (int i = 0; i < horizon.Count; i++)
            {
                // Vertices of the new face, the farthest point as well as the
                // edge on the horizon. Horizon edge is CCW, so the triangle
                // should be as well.
                var v0 = farthestPoint;
                var v1 = horizon[i].Edge0;
                var v2 = horizon[i].Edge1;

                // Opposite faces of the triangle. First, the edge on the other
                // side of the horizon, then the next/prev faces on the new cone
                var o0 = horizon[i].Face;
                var o1 = (i == horizon.Count - 1) ? firstNewFace : firstNewFace + i + 1;
                var o2 = (i == 0) ? (firstNewFace + horizon.Count - 1) : firstNewFace + i - 1;

                var fi = faceCount++;

                faces[fi] = new Face(
                    v0, v1, v2,
                    o0, o1, o2,
                    Normal(points[v0], points[v1], points[v2]));

                var horizonFace = faces[horizon[i].Face];

                if (horizonFace.Vertex0 == v1)
                {
                    Assert(v2 == horizonFace.Vertex2);
                    horizonFace.Opposite1 = fi;
                }
                else if (horizonFace.Vertex1 == v1)
                {
                    Assert(v2 == horizonFace.Vertex0);
                    horizonFace.Opposite2 = fi;
                }
                else
                {
                    Assert(v1 == horizonFace.Vertex2);
                    Assert(v2 == horizonFace.Vertex1);
                    horizonFace.Opposite0 = fi;
                }

                faces[horizon[i].Face] = horizonFace;
            }
        }

        /// <summary>
        ///   Reassign points based on the new faces added by ConstructCone().
        ///
        ///   Only points that were previous assigned to a removed face need to
        ///   be updated, so check litFaces while looping through the open set.
        ///
        ///   There is a potential optimization here: there's no reason to loop
        ///   through the entire openSet here. If each face had it's own
        ///   openSet, we could just loop through the openSets in the removed
        ///   faces. That would make the loop here shorter.
        ///
        ///   However, to do that, we would have to juggle A LOT more List<T>'s,
        ///   and we would need an object pool to manage them all without
        ///   generating a whole bunch of garbage. I don't think it's worth
        ///   doing that to make this loop shorter, a straight for-loop through
        ///   a list is pretty darn fast. Still, it might be worth trying
        /// </summary>
        void ReassignPoints(List<Vector3> points)
        {
            for (int i = 0; i <= openSetTail; i++)
            {
                var fp = openSet[i];

                if (litFaces.Contains(fp.Face))
                {
                    var assigned = false;
                    var point = points[fp.Point];

                    foreach (var kvp in faces)
                    {
                        var fi = kvp.Key;
                        var face = kvp.Value;

                        var dist = PointFaceDistance(
                            point,
                            points[face.Vertex0],
                            face);

                        if (dist > EPSILON)
                        {
                            assigned = true;

                            fp.Face = fi;
                            fp.Distance = dist;

                            openSet[i] = fp;
                            break;
                        }
                    }

                    if (!assigned)
                    {
                        // If point hasn't been assigned, then it's inside the
                        // convex hull. Swap it with openSetTail, and decrement
                        // openSetTail. We also have to decrement i, because
                        // there's now a new thing in openSet[i], so we need i
                        // to remain the same the next iteration of the loop.
                        fp.Face = INSIDE;
                        fp.Distance = float.NaN;

                        openSet[i] = openSet[openSetTail];
                        openSet[openSetTail] = fp;

                        i--;
                        openSetTail--;
                    }
                }
            }
        }

        /// <summary>
        ///   Final step in algorithm, export the faces of the convex hull in a
        ///   mesh-friendly format.
        ///
        ///   TODO normals calculation for non-split vertices. Right now it just
        ///   leaves the normal array empty.
        /// </summary>
        void ExportMesh(
            List<Vector3> points,
            bool splitVerts,
            ref List<Vector3> verts,
            ref List<int> tris,
            ref List<Vector3> normals)
        {
            if (verts == null)
            {
                verts = new List<Vector3>();
            }
            else
            {
                verts.Clear();
            }

            if (tris == null)
            {
                tris = new List<int>();
            }
            else
            {
                tris.Clear();
            }

            if (normals == null)
            {
                normals = new List<Vector3>();
            }
            else
            {
                normals.Clear();
            }

            foreach (var face in faces.Values)
            {
                int vi0, vi1, vi2;

                if (splitVerts)
                {
                    vi0 = verts.Count; verts.Add(points[face.Vertex0]);
                    vi1 = verts.Count; verts.Add(points[face.Vertex1]);
                    vi2 = verts.Count; verts.Add(points[face.Vertex2]);

                    normals.Add(face.Normal);
                    normals.Add(face.Normal);
                    normals.Add(face.Normal);
                }
                else
                {
                    if (!hullVerts.TryGetValue(face.Vertex0, out vi0))
                    {
                        vi0 = verts.Count;
                        hullVerts[face.Vertex0] = vi0;
                        verts.Add(points[face.Vertex0]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex1, out vi1))
                    {
                        vi1 = verts.Count;
                        hullVerts[face.Vertex1] = vi1;
                        verts.Add(points[face.Vertex1]);
                    }

                    if (!hullVerts.TryGetValue(face.Vertex2, out vi2))
                    {
                        vi2 = verts.Count;
                        hullVerts[face.Vertex2] = vi2;
                        verts.Add(points[face.Vertex2]);
                    }
                }

                tris.Add(vi0);
                tris.Add(vi1);
                tris.Add(vi2);
            }
        }

        /// <summary>
        ///   Signed distance from face to point (a positive number means that
        ///   the point is above the face)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float PointFaceDistance(Vector3 point, Vector3 pointOnFace, Face face)
        {
            return Dot(face.Normal, point - pointOnFace);
        }

        /// <summary>
        ///   Calculate normal for triangle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector3 Normal(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return Cross(v1 - v0, v2 - v0).normalized;
        }

        /// <summary>
        ///   Dot product, for convenience.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Dot(Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        /// <summary>
        ///   Vector3.Cross i left-handed, the algorithm is right-handed. Also,
        ///   i wanna test to see if using aggressive inlining makes any
        ///   difference here.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x);
        }

        /// <summary>
        ///   Check if two points are coincident
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AreCoincident(Vector3 a, Vector3 b)
        {
            return (a - b).magnitude <= EPSILON;
        }

        /// <summary>
        ///   Check if three points are collinear
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AreCollinear(Vector3 a, Vector3 b, Vector3 c)
        {
            return Cross(c - a, c - b).magnitude <= EPSILON;
        }

        /// <summary>
        ///   Check if four points are coplanar
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool AreCoplanar(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var n1 = Cross(c - a, c - b);
            var n2 = Cross(d - a, d - b);

            var m1 = n1.magnitude;
            var m2 = n2.magnitude;

            return m1 <= EPSILON
                || m2 <= EPSILON
                || AreCollinear(Vector3.zero,
                    (1.0f / m1) * n1,
                    (1.0f / m2) * n2);
        }

        /// <summary>
        ///   Method used for debugging, verifies that the openSet is in a
        ///   sensible state. Conditionally compiled if DEBUG_QUICKHULL if
        ///   defined.
        /// </summary>
        [Conditional("DEBUG_QUICKHULL")]
        void VerifyOpenSet(List<Vector3> points)
        {
            for (int i = 0; i < openSet.Count; i++)
            {
                if (i > openSetTail)
                {
                    Assert(openSet[i].Face == INSIDE);
                }
                else
                {
                    Assert(openSet[i].Face != INSIDE);
                    Assert(openSet[i].Face != UNASSIGNED);

                    Assert(PointFaceDistance(
                            points[openSet[i].Point],
                            points[faces[openSet[i].Face].Vertex0],
                            faces[openSet[i].Face]) > 0.0f);
                }
            }
        }

        /// <summary>
        ///   Method used for debugging, verifies that the horizon is in a
        ///   sensible state. Conditionally compiled if DEBUG_QUICKHULL if
        ///   defined.
        /// </summary>
        [Conditional("DEBUG_QUICKHULL")]
        void VerifyHorizon()
        {
            for (int i = 0; i < horizon.Count; i++)
            {
                var prev = i == 0 ? horizon.Count - 1 : i - 1;

                Assert(horizon[prev].Edge1 == horizon[i].Edge0);
                Assert(HasEdge(faces[horizon[i].Face], horizon[i].Edge1, horizon[i].Edge0));
            }
        }

        /// <summary>
        ///   Method used for debugging, verifies that the faces array is in a
        ///   sensible state. Conditionally compiled if DEBUG_QUICKHULL if
        ///   defined.
        /// </summary>
        [Conditional("DEBUG_QUICKHULL")]
        void VerifyFaces(List<Vector3> points)
        {
            foreach (var kvp in faces)
            {
                var fi = kvp.Key;
                var face = kvp.Value;

                Assert(faces.ContainsKey(face.Opposite0));
                Assert(faces.ContainsKey(face.Opposite1));
                Assert(faces.ContainsKey(face.Opposite2));

                Assert(face.Opposite0 != fi);
                Assert(face.Opposite1 != fi);
                Assert(face.Opposite2 != fi);

                Assert(face.Vertex0 != face.Vertex1);
                Assert(face.Vertex0 != face.Vertex2);
                Assert(face.Vertex1 != face.Vertex2);

                Assert(HasEdge(faces[face.Opposite0], face.Vertex2, face.Vertex1));
                Assert(HasEdge(faces[face.Opposite1], face.Vertex0, face.Vertex2));
                Assert(HasEdge(faces[face.Opposite2], face.Vertex1, face.Vertex0));

                Assert((face.Normal - Normal(
                            points[face.Vertex0],
                            points[face.Vertex1],
                            points[face.Vertex2])).magnitude < EPSILON);
            }
        }

        /// <summary>
        ///   Method used for debugging, verifies that the final mesh is
        ///   actually a convex hull of all the points. Conditionally compiled
        ///   if DEBUG_QUICKHULL if defined.
        /// </summary>
        [Conditional("DEBUG_QUICKHULL")]
        void VerifyMesh(List<Vector3> points, ref List<Vector3> verts, ref List<int> tris)
        {
            Assert(tris.Count % 3 == 0);

            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < tris.Count; j += 3)
                {
                    var t0 = verts[tris[j]];
                    var t1 = verts[tris[j + 1]];
                    var t2 = verts[tris[j + 2]];

                    Assert(Dot(points[i] - t0, Vector3.Cross(t1 - t0, t2 - t0)) <= EPSILON);
                }

            }
        }

        /// <summary>
        ///   Does face f have a face with vertexes e0 and e1? Used only for
        ///   debugging.
        /// </summary>
        bool HasEdge(Face f, int e0, int e1)
        {
            return (f.Vertex0 == e0 && f.Vertex1 == e1)
                || (f.Vertex1 == e0 && f.Vertex2 == e1)
                || (f.Vertex2 == e0 && f.Vertex0 == e1);
        }

        /// <summary>
        ///   Assert method, conditionally compiled with DEBUG_QUICKHULL.
        ///
        ///   I could just use Debug.Assert or the Assertions class, but I like
        ///   the idea of just writing Assert(something), and I also want it to
        ///   be conditionally compiled out with the same #define as the other
        ///   debug methods.
        /// </summary>
        [Conditional("DEBUG_QUICKHULL")]
        static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new UnityEngine.Assertions.AssertionException("Assertion failed", "");
            }
        }
    }
}