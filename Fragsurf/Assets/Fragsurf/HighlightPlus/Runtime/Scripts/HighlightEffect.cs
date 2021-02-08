/// <summary>
/// Highlight Plus - (c) 2018-2021 Kronnect Technologies SL
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightPlus {

    public delegate bool OnObjectHighlightStartEvent(GameObject obj);
    public delegate void OnObjectHighlightEndEvent(GameObject obj);
    public delegate bool OnRendererHighlightEvent(Renderer renderer);

    /// <summary>
    /// Triggers when target effect animation occurs
    /// </summary>
    /// <param name="t">A value from 0 to 1 that represent the animation time from start to end, based on target duration and start time</param>
    public delegate void OnTargetAnimatesEvent(ref Vector3 center, ref Quaternion rotation, ref Vector3 scale, float t);

    public enum NormalsOption {
        Smooth = 0,
        PreserveOriginal = 1,
        Reorient = 2
    }

    public enum SeeThroughMode {
        WhenHighlighted = 0,
        AlwaysWhenOccluded = 1,
        Never = 2
    }

    public enum QualityLevel {
        Fastest = 0,
        High = 1,
        Highest = 2,
        Medium = 3
    }

    public static class QualityLevelExtensions {
        public static bool UsesMultipleOffsets(this QualityLevel qualityLevel) {
            return qualityLevel == QualityLevel.Medium || qualityLevel == QualityLevel.High;
        }
    }

    public enum TargetOptions {
        Children,
        OnlyThisObject,
        RootToChildren,
        LayerInScene,
        LayerInChildren,
        Scripting
    }

    public enum Visibility {
        Normal,
        AlwaysOnTop,
        OnlyWhenOccluded
    }

    [Serializable]
    public struct GlowPassData {
        public float offset;
        public float alpha;
        public Color color;
    }

    [ExecuteInEditMode]
    public partial class HighlightEffect : MonoBehaviour {

        /// <summary>
        /// Gets or sets the current profile. To load a profile and apply its settings at runtime, please use ProfileLoad() method.
        /// </summary>
        public HighlightProfile profile;

        /// <summary>
        /// Sets if changes to the original profile should propagate to this effect.
        /// </summary>
        [Tooltip("If enabled, settings will be synced with profile.")]
        public bool profileSync;

        /// <summary>
        /// Makes the effects visible in the SceneView.
        /// </summary>
        public bool previewInEditor = true;

        /// <summary>
        /// Specifies which objects are affected by this effect.
        /// </summary>
        public TargetOptions effectGroup = TargetOptions.Children;

        /// <summary>
        /// The layer that contains the affected objects by this effect when effectGroup is set to LayerMask.
        /// </summary>
        [Tooltip("The layer that contains the affected objects by this effect when effectGroup is set to LayerMask.")]
        public LayerMask effectGroupLayer = -1;

        /// <summary>
        /// Optional object name filter
        /// </summary>
        [Tooltip("Only include objects whose names contains this text.")]
        public string effectNameFilter;

        /// <summary>
        /// Combine objects into a single mesh
        /// </summary>
        [Tooltip("Combine meshes of all objects in this group affected by Highlight Effect reducing draw calls.")]
        public bool combineMeshes;

        /// <summary>
        /// The alpha threshold for transparent cutout objects. Pixels with alpha below this value will be discarded.
        /// </summary>
        [Tooltip("The alpha threshold for transparent cutout objects. Pixels with alpha below this value will be discarded.")]
        [Range(0, 1)]
        public float alphaCutOff;

        /// <summary>
        /// If back facing triangles are ignored. Backfaces triangles are not visible but you may set this property to false to force highlight effects to act on those triangles as well.
        /// </summary>
        [Tooltip("If back facing triangles are ignored.Backfaces triangles are not visible but you may set this property to false to force highlight effects to act on those triangles as well.")]
        public bool cullBackFaces = true;

        /// <summary>
        /// Show highlight effects even if the object is currently not visible. This option is useful if the affected objects are rendered using GPU instancing tools which render directly to the GPU without creating real game object geometry in CPU.
        /// </summary>
        [Tooltip("Show highlight effects even if the object is not visible. If this object or its children use GPU Instancing tools, the MeshRenderer can be disabled although the object is visible. In this case, this option is useful to enable highlighting.")]
        public bool ignoreObjectVisibility;

        /// <summary>
        /// Enable to support reflection probes
        /// </summary>
        [Tooltip("Support reflection probes. Enable only if you want the effects to be visible in reflections.")]
        public bool reflectionProbes;

        /// <summary>
        /// Enable to support reflection probes
        /// </summary>
        [Tooltip("Enables GPU instancing. Reduces draw calls in outline and outer glow effects on platforms that support GPU instancing. Should be enabled by default.")]
        public bool GPUInstancing = true;

        [Tooltip("Normals handling option.")]
        public NormalsOption normalsOption;

        /// <summary>
        /// Ignores highlight effects on this object.
        /// </summary>
        [Tooltip("Ignore highlighting on this object.")]
        public bool ignore;

        [SerializeField]
        bool _highlighted;

        public bool highlighted { get { return _highlighted; } set { SetHighlighted(value); } }

        public float fadeInDuration;
        public float fadeOutDuration;

#if UNITY_2019_OR_NEWER
        public bool flipY = true;

#else
        public bool flipY;
#endif

        [Tooltip("Keeps the outline/glow size unaffected by object distance.")]
        public bool constantWidth = true;

        [Tooltip("Mask to include or exclude certain submeshes. By default, all submeshes are included.")]
        public int subMeshMask = -1;

        [Range(0, 1)]
        public float overlay = 0.5f;
        [ColorUsage(true, true)] public Color overlayColor = Color.yellow;
        public float overlayAnimationSpeed = 1f;
        [Range(0, 1)]
        public float overlayMinIntensity = 0.5f;
        [Range(0, 1)]
        public float overlayBlending = 1.0f;

        [Range(0, 1)]
        public float outline = 1f;
        [ColorUsage(true, true)] public Color outlineColor = Color.black;
        public float outlineWidth = 0.45f;
        public QualityLevel outlineQuality = QualityLevel.Medium;
        [Range(1, 8)]
        public int outlineDownsampling = 2;
        public Visibility outlineVisibility = Visibility.Normal;
        public bool outlineBlitDebug;
        public bool outlineIndependent;

        [Range(0, 5)]
        public float glow = 1f;
        public float glowWidth = 0.4f;
        public QualityLevel glowQuality = QualityLevel.Medium;
        [Range(1, 8)]
        public int glowDownsampling = 2;
        [ColorUsage(true, true)] public Color glowHQColor = new Color(0.64f, 1f, 0f, 1f);
        [Tooltip("When enabled, outer glow renders with dithering. When disabled, glow appears as a solid color.")]
        public bool glowDithering = true;
        [Tooltip("Seed for the dithering effect")]
        public float glowMagicNumber1 = 0.75f;
        [Tooltip("Another seed for the dithering effect that combines with first seed to create different patterns")]
        public float glowMagicNumber2 = 0.5f;
        public float glowAnimationSpeed = 1f;
        public Visibility glowVisibility = Visibility.Normal;
        public bool glowBlitDebug;
        public bool glowBlendPasses = true;
        public GlowPassData[] glowPasses;

        [Range(0, 5f)]
        public float innerGlow;
        [Range(0, 2)]
        public float innerGlowWidth = 1f;
        [ColorUsage(true, true)] public Color innerGlowColor = Color.white;
        public Visibility innerGlowVisibility = Visibility.Normal;

        public bool targetFX;
        public Texture2D targetFXTexture;
        [ColorUsage(true, true)] public Color targetFXColor = Color.white;
        public Transform targetFXCenter;
        public float targetFXRotationSpeed = 50f;
        public float targetFXInitialScale = 4f;
        public float targetFXEndScale = 1.5f;
        public float targetFXTransitionDuration = 0.5f;
        public float targetFXStayDuration = 1.5f;

        public event OnObjectHighlightStartEvent OnObjectHighlightStart;
        public event OnObjectHighlightEndEvent OnObjectHighlightEnd;
        public event OnRendererHighlightEvent OnRendererHighlightStart;
        public event OnTargetAnimatesEvent OnTargetAnimates;

        public SeeThroughMode seeThrough;
        [Range(0, 5f)]
        public float seeThroughIntensity = 0.8f;
        [Range(0, 1)]
        public float seeThroughTintAlpha = 0.5f;
        [ColorUsage(true, true)] public Color seeThroughTintColor = Color.red;
        [Range(0, 1)]
        public float seeThroughNoise = 1f;
        [Range(0, 1)] public float seeThroughBorder;
        public Color seeThroughBorderColor = Color.black;
        public float seeThroughBorderWidth = 0.45f;

        struct ModelMaterials {
            public bool render; // if this object can render this frame
            public Transform transform;
            public bool renderWasVisibleDuringSetup;
            public Mesh mesh, originalMesh;
            public Renderer renderer;
            public bool isSkinnedMesh;
            public Material[] fxMatMask, fxMatSolidColor, fxMatSeeThroughInner, fxMatSeeThroughBorder, fxMatOverlay, fxMatInnerGlow;
            public NormalsOption normalsOption;
            public Matrix4x4 renderingMatrix;

            public bool isCombined;
            public bool preserveOriginalMesh { get { return !isCombined && normalsOption == NormalsOption.PreserveOriginal; } }

            public void Init() {
                render = false;
                transform = null;
                mesh = originalMesh = null;
                renderer = null;
                isSkinnedMesh = false;
                normalsOption = NormalsOption.Smooth;
                isCombined = false;
            }
        }

        enum FadingState {
            FadingOut = -1,
            NoFading = 0,
            FadingIn = 1
        }

        [SerializeField, HideInInspector]
        ModelMaterials[] rms;
        [SerializeField, HideInInspector]
        int rmsCount;

#if UNITY_EDITOR
        /// <summary>
        /// True if there's some static children
        /// </summary>
        [NonSerialized]
        public bool staticChildren;
#endif

        [NonSerialized]
        public Transform target;

        // Time in which the highlight started
        [NonSerialized]
        public float highlightStartTime;

        // Time in which the target fx started
        [NonSerialized]
        public float targetFxStartTime;

        const float TAU = 0.70711f;

        // Reference materials. These are instanced per object (rms).
        static Material fxMatMask, fxMatSolidColor, fxMatSeeThrough, fxMatSeeThroughBorder, fxMatOverlay, fxMatClearStencil;

        // Per-object materials
        Material fxMatGlowTemplate, fxMatInnerGlow, fxMatOutlineTemplate, fxMatTarget;
        Material fxMatComposeGlow, fxMatComposeOutline, fxMatBlurGlow, fxMatBlurOutline;
        Material[] fxMatOutline, fxMatGlow;

        static Vector4[] offsets;

        float fadeStartTime;
        FadingState fading = FadingState.NoFading;
        CommandBuffer cbHighlight, cbSeeThrough, cbOverlay, cbInnerGlow;
        int[] mipGlowBuffers, mipOutlineBuffers;
        int glowRT, outlineRT;
        static Mesh quadMesh;
        int sourceRT;
        Matrix4x4 quadGlowMatrix, quadOutlineMatrix;
        Vector3[] corners;
        RenderTextureDescriptor sourceDesc;
        Color debugColor, blackColor;
        Visibility lastOutlineVisibility;
        bool requireUpdateMaterial;

        [NonSerialized]
        public static List<HighlightEffect> instances = new List<HighlightEffect>();

        //bool usingPipeline;
        bool useSmoothGlow, useSmoothOutline, useSmoothBlend;
        bool useGPUInstancing;

        MaterialPropertyBlock glowPropertyBlock, outlinePropertyBlock;
        static readonly List<Vector4> matDataDirection = new List<Vector4>();
        static readonly List<Vector4> matDataGlow = new List<Vector4>();
        static readonly List<Vector4> matDataColor = new List<Vector4>();
        static Matrix4x4[] matrices;

        int outlineOffsetsMin, outlineOffsetsMax;
        int glowOffsetsMin, glowOffsetsMax;
        static CombineInstance[] combineInstances;
        static Matrix4x4 Matrix4x4Identity = Matrix4x4.identity;

        void OnEnable() {
            lastOutlineVisibility = outlineVisibility;
            debugColor = new Color(1f, 0f, 0f, 0.5f);
            blackColor = new Color(0, 0, 0, 0);
            if (offsets == null || offsets.Length != 8) {
                offsets = new Vector4[] {
                    new Vector4(0,1),
                    new Vector4(1,0),
                    new Vector4(0,-1),
                    new Vector4(-1,0),
                    new Vector4 (-TAU, TAU),
                    new Vector4 (TAU, TAU),
                    new Vector4 (TAU, -TAU),
                    new Vector4 (-TAU, -TAU)
                };
            }
            if (corners == null || corners.Length != 8) {
                corners = new Vector3[8];
            }
            if (quadMesh == null) {
                BuildQuad();
            }
            if (target == null) {
                target = transform;
            }
            if (profileSync && profile != null) {
                profile.Load(this);
            }
            if (glowPasses == null || glowPasses.Length == 0) {
                glowPasses = new GlowPassData[4];
                glowPasses[0] = new GlowPassData() { offset = 4, alpha = 0.1f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[1] = new GlowPassData() { offset = 3, alpha = 0.2f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[2] = new GlowPassData() { offset = 2, alpha = 0.3f, color = new Color(0.64f, 1f, 0f, 1f) };
                glowPasses[3] = new GlowPassData() { offset = 1, alpha = 0.4f, color = new Color(0.64f, 1f, 0f, 1f) };
            }
            sourceRT = Shader.PropertyToID("_HPSourceRT");
            useGPUInstancing = GPUInstancing && SystemInfo.supportsInstancing;
            if (useGPUInstancing) {
                if (glowPropertyBlock == null) {
                    glowPropertyBlock = new MaterialPropertyBlock();
                }
                if (outlinePropertyBlock == null) {
                    outlinePropertyBlock = new MaterialPropertyBlock();
                }
            }

            CheckGeometrySupportDependencies();
            SetupMaterial();

            instances.Add(this);
        }

        void OnDisable() {
            UpdateMaterialProperties();
            if (instances != null) {
                int k = instances.IndexOf(this);
                if (k >= 0) {
                    instances.RemoveAt(k);
                }
            }
        }


        void DestroyMaterial(Material mat) {
            if (mat != null) DestroyImmediate(mat);
        }

        void DestroyMaterialArray(Material[] mm) {
            if (mm == null) return;
            for (int k = 0; k < mm.Length; k++) {
                DestroyMaterial(mm[k]);
            }
        }

        void OnDestroy() {
            if (rms != null) {
                for (int k = 0; k < rms.Length; k++) {
                    DestroyMaterialArray(rms[k].fxMatMask);
                    DestroyMaterialArray(rms[k].fxMatSolidColor);
                    DestroyMaterialArray(rms[k].fxMatSeeThroughInner);
                    DestroyMaterialArray(rms[k].fxMatSeeThroughBorder);
                    DestroyMaterialArray(rms[k].fxMatOverlay);
                    DestroyMaterialArray(rms[k].fxMatInnerGlow);
                }
            }

            DestroyMaterial(fxMatGlowTemplate);
            DestroyMaterial(fxMatInnerGlow);
            DestroyMaterial(fxMatOutlineTemplate);
            DestroyMaterial(fxMatTarget);
            DestroyMaterial(fxMatComposeGlow);
            DestroyMaterial(fxMatComposeOutline);
            DestroyMaterial(fxMatBlurGlow);
            DestroyMaterial(fxMatBlurOutline);
            DestroyMaterialArray(fxMatOutline);
            DestroyMaterialArray(fxMatGlow);

            if (combinedMeshes.ContainsKey(combinedMeshesHashId)) {
                combinedMeshes.Remove(combinedMeshesHashId);
            }
        }

        void Reset() {
            SetupMaterial();
        }


        /// <summary>
        /// Loads a profile into this effect
        /// </summary>
        public void ProfileLoad(HighlightProfile profile) {
            if (profile != null) {
                profile.Load(this);
            }
        }

        /// <summary>
        /// Reloads currently assigned profile
        /// </summary>
        public void ProfileReload() {
            if (profile != null) {
                profile.Load(this);
            }
        }


        /// <summary>
        /// Save current settings into given profile
        /// </summary>
        public void ProfileSaveChanges(HighlightProfile profile) {
            if (profile != null) {
                profile.Save(this);
            }
        }

        /// <summary>
        /// Save current settings into current profile
        /// </summary>
        public void ProfileSaveChanges() {
            if (profile != null) {
                profile.Save(this);
            }
        }


        public void Refresh() {
            if (enabled) {
                SetupMaterial();
            }
        }

        RenderTargetIdentifier colorAttachmentBuffer, depthAttachmentBuffer;

        public CommandBuffer GetCommandBuffer(Camera cam, RenderTargetIdentifier colorAttachmentBuffer, RenderTargetIdentifier depthAttachmentBuffer) {
            //usingPipeline = true;
            this.colorAttachmentBuffer = colorAttachmentBuffer;
            this.depthAttachmentBuffer = depthAttachmentBuffer;
            BuildCommandBuffer(cam);
            return cbHighlight;
        }

        void BuildCommandBuffer(Camera cam) {

            if (colorAttachmentBuffer == 0) {
                colorAttachmentBuffer = BuiltinRenderTextureType.CameraTarget;
            }
            if (depthAttachmentBuffer == 0) {
                depthAttachmentBuffer = BuiltinRenderTextureType.CameraTarget;
            }

            if (cam == null || cbHighlight == null) return;

            cbHighlight.Clear();

#if UNITY_EDITOR
            if (!previewInEditor && !Application.isPlaying) {
                return;
            }
#endif
            if (requireUpdateMaterial) {
                requireUpdateMaterial = false;
                UpdateMaterialProperties();
            }

            bool seeThroughReal = seeThroughIntensity > 0 && (this.seeThrough == SeeThroughMode.AlwaysWhenOccluded || (this.seeThrough == SeeThroughMode.WhenHighlighted && _highlighted));
            if (seeThroughReal) {
                RenderOccluders(cbHighlight, cam);
            }
            if (cancelSeeThroughThisFrame) {
                cancelSeeThroughThisFrame = false;
                seeThroughReal = false;
            }
            if (!_highlighted && !seeThroughReal && !hitActive) {
                return;
            }

            // Check camera culling mask
            int cullingMask = cam.cullingMask;

            // Ensure renderers are valid and visible (in case LODgroup has changed active renderer)
            if (!ignoreObjectVisibility) {
                for (int k = 0; k < rmsCount; k++) {
                    if (rms[k].renderer != null && rms[k].renderer.isVisible != rms[k].renderWasVisibleDuringSetup) {
                        SetupMaterial();
                        break;
                    }
                }
            }

            // Apply effect
            float glowReal = _highlighted ? this.glow : 0;
            int layer = gameObject.layer;

            if (fxMatMask == null)
                return;

            // Check smooth blend ztesting capability
            Visibility smoothGlowVisibility = glowVisibility;
            Visibility smoothOutlineVisibility = outlineVisibility;

            float aspect = cam.aspect;
            bool somePartVisible = false;

            // First create masks
            bool independentFullScreenNotExecuted = true;
            for (int k = 0; k < rmsCount; k++) {
                rms[k].render = false;
                Transform t = rms[k].transform;
                if (t == null)
                    continue;
                Mesh mesh = rms[k].mesh;
                if (mesh == null)
                    continue;
                if (((1 << t.gameObject.layer) & cullingMask) == 0)
                    continue;
                if (!rms[k].renderer.isVisible && !ignoreObjectVisibility)
                    continue;
                if (!reflectionProbes && cam.cameraType == CameraType.Reflection)
                    continue;
                rms[k].render = true;
                somePartVisible = true;

                if (rms[k].isCombined) {
                    rms[k].renderingMatrix = t.localToWorldMatrix;
                }

                if (outlineIndependent) {
                    if (useSmoothBlend && independentFullScreenNotExecuted) {
                        independentFullScreenNotExecuted = false;
                        cbHighlight.DrawMesh(quadMesh, Matrix4x4Identity, fxMatClearStencil);
                    } else if (outline > 0) {
                        for (int l = 0; l < mesh.subMeshCount; l++) {
                            if (((1 << l) & subMeshMask) == 0) continue;
                            if (outlineQuality.UsesMultipleOffsets()) {
                                for (int o = outlineOffsetsMin; o <= outlineOffsetsMax; o++) {
                                    Vector4 direction = offsets[o] * (outlineWidth / 100f);
                                    direction.y *= aspect;
                                    fxMatOutline[o].SetVector(ShaderParams.OutlineDirection, direction);
                                    if (rms[k].isCombined) {
                                        cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, fxMatOutline[o], l, 1);
                                    } else {
                                        cbHighlight.DrawRenderer(rms[k].renderer, fxMatOutline[o], l, 1);
                                    }
                                }
                            } else {
                                fxMatOutline[0].SetVector(ShaderParams.OutlineDirection, Vector4.zero);
                                if (rms[k].isCombined) {
                                    cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, fxMatOutline[0], l, 1);
                                } else {
                                    cbHighlight.DrawRenderer(rms[k].renderer, fxMatOutline[0], l, 1);
                                }
                            }
                        }
                    }
                }


                for (int l = 0; l < mesh.subMeshCount; l++) {
                    if (((1 << l) & subMeshMask) == 0) continue;
                    if (_highlighted && ((outline > 0 && smoothOutlineVisibility != Visibility.Normal) || (glow > 0 && smoothGlowVisibility != Visibility.Normal) || (innerGlow > 0 && innerGlowVisibility != Visibility.Normal))) {
                        rms[k].fxMatMask[l].SetInt(ShaderParams.ZTest, (int)CompareFunction.Always);
                    } else {
                        rms[k].fxMatMask[l].SetInt(ShaderParams.ZTest, (int)CompareFunction.LessEqual);
                    }
                    if (rms[k].isCombined) {
                        cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatMask[l], l);
                    } else {
                        cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatMask[l], l);
                    }
                }
            }

            // Compute tweening
            float fade = 1f;
            if (fading != FadingState.NoFading) {
                if (fading == FadingState.FadingIn) {
                    if (fadeInDuration > 0) {
                        fade = (Time.time - fadeStartTime) / fadeInDuration;
                        if (fade > 1f) {
                            fade = 1f;
                            fading = FadingState.NoFading;
                        }
                    }
                } else if (fadeOutDuration > 0) {
                    fade = 1f - (Time.time - fadeStartTime) / fadeOutDuration;
                    if (fade < 0f) {
                        fade = 0f;
                        fading = FadingState.NoFading;
                        _highlighted = false;
                        if (OnObjectHighlightEnd != null) {
                            OnObjectHighlightEnd(gameObject);
                        }
                        SendMessage("HighlightEnd", null, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            if (glowQuality == QualityLevel.High) {
                glowReal *= 0.25f;
            } else if (glowQuality == QualityLevel.Medium) {
                glowReal *= 0.5f;
            }

            bool targetEffectRendered = false;
            // Add mesh effects
            for (int k = 0; k < rmsCount; k++) {
                if (!rms[k].render)
                    continue;
                Mesh mesh = rms[k].mesh;

                // See-Through
                if (seeThroughReal) {
                    bool usesSeeThroughBorder = seeThroughBorder * seeThroughBorderWidth > 0;
                    for (int l = 0; l < mesh.subMeshCount; l++) {
                        if (((1 << l) & subMeshMask) == 0) continue;
                        if (l < rms[k].fxMatSeeThroughInner.Length && rms[k].fxMatSeeThroughInner[l] != null) {
                            if (rms[k].isCombined) {
                                cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatSeeThroughInner[l], l);
                            } else {
                                cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatSeeThroughInner[l], l);
                            }
                            if (usesSeeThroughBorder) {
                                if (rms[k].isCombined) {
                                    cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatSeeThroughBorder[l], l);
                                } else {
                                    cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatSeeThroughBorder[l], l);
                                }
                            }
                        }
                    }
                }

                if (_highlighted || hitActive) {
                    // Hit FX
                    Color overlayColor = this.overlayColor;
                    float overlayMinIntensity = this.overlayMinIntensity;
                    float overlayBlending = this.overlayBlending;
                    if (hitActive) {
                        float t = hitFadeOutDuration > 0 ? (Time.time - hitStartTime) / hitFadeOutDuration : 1f;
                        if (t >= 1f) {
                            hitActive = false;
                            overlayColor.a = _highlighted ? overlay : 0;
                        } else {
                            bool lerpToCurrentOverlay = _highlighted && overlay > 0;
                            overlayColor = lerpToCurrentOverlay ? Color.Lerp(hitColor, overlayColor, t) : hitColor;
                            overlayColor.a = lerpToCurrentOverlay ? Mathf.Lerp(1f - t, overlay, t) : 1f - t;
                            overlayColor.a *= hitInitialIntensity;
                            overlayMinIntensity = 1f;
                            overlayBlending = 0;
                        }
                    } else {
                        overlayColor.a = overlay * fade;
                    }

                    // Overlay
                    if (overlayColor.a > 0) {
                        for (int l = 0; l < mesh.subMeshCount; l++) {
                            if (((1 << l) & subMeshMask) == 0) continue;
                            if (l < rms[k].fxMatOverlay.Length && rms[k].fxMatOverlay[l] != null) {
                                rms[k].fxMatOverlay[l].color = overlayColor;
                                rms[k].fxMatOverlay[l].SetVector(ShaderParams.OverlayData, new Vector4(overlayAnimationSpeed, overlayMinIntensity, overlayBlending));
                                if (rms[k].isCombined) {
                                    cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatOverlay[l], l);
                                } else {
                                    cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatOverlay[l], l);
                                }
                            }
                        }
                    }
                }

                if (!_highlighted)
                    continue;

                bool allowGPUInstancing = useGPUInstancing && !rms[k].isSkinnedMesh;

                for (int l = 0; l < mesh.subMeshCount; l++) {
                    if (((1 << l) & subMeshMask) == 0) continue;

                    // Glow
                    if (glow > 0 && glowQuality != QualityLevel.Highest) {
                        matDataGlow.Clear();
                        matDataColor.Clear();
                        matDataDirection.Clear();
                        for (int glowPass = 0; glowPass < glowPasses.Length; glowPass++) {
                            if (glowQuality.UsesMultipleOffsets()) {
                                for (int o = glowOffsetsMin; o <= glowOffsetsMax; o++) {
                                    Vector4 direction = offsets[o];
                                    direction.y *= aspect;
                                    Color dataColor = glowPasses[glowPass].color;
                                    Vector4 dataGlow = new Vector4(fade * glowReal * glowPasses[glowPass].alpha, glowPasses[glowPass].offset * glowWidth / 100f, glowMagicNumber1, glowMagicNumber2);
                                    if (allowGPUInstancing) {
                                        matDataDirection.Add(direction);
                                        matDataGlow.Add(dataGlow);
                                        matDataColor.Add(new Vector4(dataColor.r, dataColor.g, dataColor.b, dataColor.a));
                                    } else {
                                        int matIndex = glowPass * 8 + o;
                                        fxMatGlow[matIndex].SetVector(ShaderParams.GlowDirection, direction);
                                        fxMatGlow[matIndex].SetColor(ShaderParams.GlowColor, dataColor);
                                        fxMatGlow[matIndex].SetVector(ShaderParams.Glow, dataGlow);
                                        if (rms[k].isCombined) {
                                            cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, fxMatGlow[matIndex], l);
                                        } else {
                                            cbHighlight.DrawRenderer(rms[k].renderer, fxMatGlow[matIndex], l);
                                        }
                                    }
                                }
                            } else {
                                Vector4 dataGlow = new Vector4(fade * glowReal * glowPasses[glowPass].alpha, glowPasses[glowPass].offset * glowWidth / 100f, glowMagicNumber1, glowMagicNumber2);
                                Color dataColor = glowPasses[glowPass].color;
                                if (allowGPUInstancing) {
                                    matDataDirection.Add(Vector4.zero);
                                    matDataGlow.Add(dataGlow);
                                    matDataColor.Add(new Vector4(dataColor.r, dataColor.g, dataColor.b, dataColor.a));
                                } else {
                                    int matIndex = glowPass * 8;
                                    fxMatGlow[matIndex].SetColor(ShaderParams.GlowColor, dataColor);
                                    fxMatGlow[matIndex].SetVector(ShaderParams.Glow, dataGlow);
                                    fxMatGlow[matIndex].SetVector(ShaderParams.GlowDirection, Vector4.zero);
                                    if (rms[k].isCombined) {
                                        cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, fxMatGlow[matIndex], l);
                                    } else {
                                        cbHighlight.DrawRenderer(rms[k].renderer, fxMatGlow[matIndex], l);
                                    }
                                }
                            }
                        }
                        if (allowGPUInstancing) {
                            int instanceCount = matDataDirection.Count;
                            if (instanceCount > 0) {
                                glowPropertyBlock.Clear();
                                glowPropertyBlock.SetVectorArray(ShaderParams.GlowDirection, matDataDirection);
                                glowPropertyBlock.SetVectorArray(ShaderParams.GlowColor, matDataColor);
                                glowPropertyBlock.SetVectorArray(ShaderParams.Glow, matDataGlow);
                                if (matrices == null || matrices.Length < instanceCount) {
                                    matrices = new Matrix4x4[instanceCount];
                                }
                                if (rms[k].isCombined) {
                                    for (int m = 0; m < instanceCount; m++) {
                                        matrices[m] = rms[k].renderingMatrix;
                                    }
                                } else {
                                    Matrix4x4 objectToWorld = rms[k].transform.localToWorldMatrix;
                                    for (int m = 0; m < instanceCount; m++) {
                                        matrices[m] = objectToWorld;
                                    }
                                }
                                cbHighlight.DrawMeshInstanced(mesh, l, fxMatGlow[glowOffsetsMin], 0, matrices, instanceCount, glowPropertyBlock);
                            }
                        }
                    }

                    // Outline
                    if (outline > 0 && outlineQuality != QualityLevel.Highest) {
                        Color outlineColor = this.outlineColor;
                        outlineColor.a = outline * fade;
                        if (outlineQuality.UsesMultipleOffsets()) {
                            matDataDirection.Clear();
                            for (int o = outlineOffsetsMin; o <= outlineOffsetsMax; o++) {
                                fxMatOutline[o].SetColor(ShaderParams.OutlineColor, outlineColor);
                                Vector4 direction = offsets[o] * (outlineWidth / 100f);
                                direction.y *= aspect;
                                if (allowGPUInstancing) {
                                    matDataDirection.Add(direction);
                                } else {
                                    fxMatOutline[o].SetVector(ShaderParams.OutlineDirection, direction);
                                    if (rms[k].isCombined) {
                                        cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, fxMatOutline[o], l, 0);
                                    } else {
                                        cbHighlight.DrawRenderer(rms[k].renderer, fxMatOutline[o], l, 0);
                                    }
                                }
                            }
                            if (allowGPUInstancing) {
                                int instanceCount = matDataDirection.Count;
                                if (instanceCount > 0) {
                                    outlinePropertyBlock.Clear();
                                    outlinePropertyBlock.SetVectorArray(ShaderParams.OutlineDirection, matDataDirection);
                                    if (matrices == null || matrices.Length < instanceCount) {
                                        matrices = new Matrix4x4[instanceCount];
                                    }
                                    if (rms[k].isCombined) {
                                        for (int m = 0; m < instanceCount; m++) {
                                            matrices[m] = rms[k].renderingMatrix;
                                        }
                                    } else {
                                        Matrix4x4 objectToWorld = rms[k].transform.localToWorldMatrix;
                                        for (int m = 0; m < instanceCount; m++) {
                                            matrices[m] = objectToWorld;
                                        }
                                    }
                                    cbHighlight.DrawMeshInstanced(mesh, l, fxMatOutline[outlineOffsetsMin], 0, matrices, instanceCount, outlinePropertyBlock);
                                }
                            }
                        } else {
                            fxMatOutline[0].SetColor(ShaderParams.OutlineColor, outlineColor);
                            fxMatOutline[0].SetVector(ShaderParams.OutlineDirection, Vector4.zero);
                            if (rms[k].isSkinnedMesh) {
                                cbHighlight.DrawRenderer(rms[k].renderer, fxMatOutline[0], l, 0);
                            } else {
                                // keep this because normals may be reoriented / smoothed
                                cbHighlight.DrawMesh(mesh, rms[k].transform.localToWorldMatrix, fxMatOutline[0], l, 0);
                            }
                        }
                    }

                    // Inner Glow
                    if (innerGlow > 0 && innerGlowWidth > 0) {
                        if (l < rms[k].fxMatInnerGlow.Length && rms[k].fxMatInnerGlow[l] != null) {
                            Color innerGlowColorA = innerGlowColor;
                            innerGlowColorA.a = innerGlow * fade;
                            rms[k].fxMatInnerGlow[l].SetColor(ShaderParams.Color, innerGlowColorA);
                            if (rms[k].isCombined) {
                                cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatInnerGlow[l], l);
                            } else {
                                cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatInnerGlow[l], l);
                            }
                        }
                    }
                }

                // Target
                if (targetFX) {
                    float fadeOut = 1f;
                    if (Application.isPlaying) {
                        fadeOut = (Time.time - targetFxStartTime);
                        if (fadeOut >= targetFXStayDuration) {
                            fadeOut -= targetFXStayDuration;
                            fadeOut = 1f - fadeOut;
                        }
                        if (fadeOut > 1f) {
                            fadeOut = 1f;
                        }
                    }
                    bool usesTarget = targetFXCenter != null;
                    if (fadeOut > 0 && !(targetEffectRendered && usesTarget)) {
                        targetEffectRendered = true;
                        float scaleT = 1f;
                        float time;
                        float normalizedTime = 0;
                        if (Application.isPlaying) {
                            normalizedTime = (Time.time - targetFxStartTime) / targetFXTransitionDuration;
                            if (normalizedTime > 1f) {
                                normalizedTime = 1f;
                            }
                            scaleT = Mathf.Sin(normalizedTime * Mathf.PI * 0.5f);
                            time = Time.time;
                        } else {
                            time = (float)DateTime.Now.Subtract(DateTime.Today).TotalSeconds;
                        }
                        Bounds bounds = rms[k].renderer.bounds;
                        Vector3 scale = bounds.size;
                        float minSize = scale.x;
                        if (scale.y < minSize) {
                            minSize = scale.y;
                        }
                        if (scale.z < minSize) {
                            minSize = scale.z;
                        }
                        scale.x = scale.y = scale.z = minSize;
                        scale = Vector3.Lerp(scale * targetFXInitialScale, scale * targetFXEndScale, scaleT);
                        Quaternion rotation = Quaternion.LookRotation(cam.transform.position - rms[k].transform.position);
                        Quaternion animationRot = Quaternion.Euler(0, 0, time * targetFXRotationSpeed);
                        rotation *= animationRot;
                        Vector3 center = usesTarget ? targetFXCenter.transform.position : bounds.center;
                        if (OnTargetAnimates != null) {
                            OnTargetAnimates(ref center, ref rotation, ref scale, normalizedTime);
                        }
                        Matrix4x4 m = Matrix4x4.TRS(center, rotation, scale);
                        Color color = targetFXColor;
                        color.a *= fade * fadeOut;
                        fxMatTarget.color = color;
                        cbHighlight.DrawMesh(quadMesh, m, fxMatTarget, 0);
                    }
                }
            }

            if (useSmoothBlend && _highlighted && somePartVisible) {

                int smoothRTWidth = 0;
                int smoothRTHeight = 0;
                Bounds smoothBounds = new Bounds();

                // Prepare smooth outer glow / outline target
                smoothRTWidth = cam.pixelWidth;
                smoothRTHeight = cam.pixelHeight;
                if (smoothRTHeight <= 0) {
                    smoothRTHeight = 1;
                }
                if (UnityEngine.XR.XRSettings.enabled && Application.isPlaying) {
                    sourceDesc = UnityEngine.XR.XRSettings.eyeTextureDesc;
                } else {
                    sourceDesc = new RenderTextureDescriptor(smoothRTWidth, smoothRTHeight, Application.isMobilePlatform ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
                    sourceDesc.volumeDepth = 1;
                }
                sourceDesc.msaaSamples = 1;
                sourceDesc.useMipMap = false;
                sourceDesc.depthBufferBits = 0;

                cbHighlight.GetTemporaryRT(sourceRT, sourceDesc, FilterMode.Bilinear);
                cbHighlight.SetRenderTarget(sourceRT);
                cbHighlight.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));

                for (int k = 0; k < rmsCount; k++) {
                    if (!rms[k].render)
                        continue;
                    if (k == 0) {
                        smoothBounds = rms[k].renderer.bounds;
                    } else {
                        smoothBounds.Encapsulate(rms[k].renderer.bounds);
                    }
                    Mesh mesh = rms[k].mesh;

                    // Render object body for glow/outline highest quality
                    for (int l = 0; l < mesh.subMeshCount; l++) {
                        if (((1 << l) & subMeshMask) == 0) continue;
                        if (l < rms[k].fxMatSolidColor.Length) {
                            if (rms[k].isCombined) {
                                cbHighlight.DrawMesh(rms[k].mesh, rms[k].renderingMatrix, rms[k].fxMatSolidColor[l], l);
                            } else {
                                cbHighlight.DrawRenderer(rms[k].renderer, rms[k].fxMatSolidColor[l], l);
                            }
                        }
                    }
                }

                if (ComputeSmoothQuadMatrix(cam, smoothBounds)) {
                    // Smooth Glow
                    if (useSmoothGlow) {
                        float intensity = glow * fade;
                        fxMatComposeGlow.color = new Color(glowHQColor.r * intensity, glowHQColor.g * intensity, glowHQColor.b * intensity, glowHQColor.a * intensity);
                        SmoothGlow(smoothRTWidth / glowDownsampling, smoothRTHeight / glowDownsampling);
                    }

                    // Smooth Outline
                    if (useSmoothOutline) {
                        float intensity = outline * fade;
                        fxMatComposeOutline.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, outlineColor.a * intensity * 10f);
                        SmoothOutline(smoothRTWidth / outlineDownsampling, smoothRTHeight / outlineDownsampling);
                    }

                    // Bit result
                    ComposeSmoothBlend(smoothGlowVisibility, smoothOutlineVisibility);
                }
            }
        }

        bool ComputeSmoothQuadMatrix(Camera cam, Bounds bounds) {
            // Compute bounds in screen space and enlarge for glow space
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(max.x, min.y, min.z);
            corners[3] = new Vector3(max.x, min.y, max.z);
            corners[4] = new Vector3(min.x, max.y, min.z);
            corners[5] = new Vector3(min.x, max.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = new Vector3(max.x, max.y, max.z);
            Vector3 scrMin = new Vector3(float.MaxValue, float.MaxValue, 0);
            Vector3 scrMax = new Vector3(float.MinValue, float.MinValue, 0);
            float distance = float.MaxValue;
            for (int k = 0; k < corners.Length; k++) {
                corners[k] = cam.WorldToScreenPoint(corners[k]);
                if (corners[k].x < scrMin.x) {
                    scrMin.x = corners[k].x;
                }
                if (corners[k].y < scrMin.y) {
                    scrMin.y = corners[k].y;
                }
                if (corners[k].x > scrMax.x) {
                    scrMax.x = corners[k].x;
                }
                if (corners[k].y > scrMax.y) {
                    scrMax.y = corners[k].y;
                }
                if (corners[k].z < distance) {
                    distance = corners[k].z;
                    if (distance < cam.nearClipPlane) {
                        scrMin.x = scrMin.y = 0;
                        scrMax.x = cam.pixelWidth;
                        scrMax.y = cam.pixelHeight;
                        break;
                    }
                }
            }
            if (scrMax.y == scrMin.y)
                return false;

            if (distance < cam.nearClipPlane) {
                distance = cam.nearClipPlane + 0.01f;
            }
            scrMin.z = scrMax.z = distance;
            if (outline > 0) {
                BuildMatrix(cam, scrMin, scrMax, (int)(10 + 20 * outlineWidth + 5 * outlineDownsampling), ref quadOutlineMatrix);
            }
            if (glow > 0) {
                BuildMatrix(cam, scrMin, scrMax, (int)(20 + 30 * glowWidth + 10 * glowDownsampling), ref quadGlowMatrix);
            }
            return true;
        }

        void BuildMatrix(Camera cam, Vector3 scrMin, Vector3 scrMax, int border, ref Matrix4x4 quadMatrix) {

            // Insert padding to make room for effects
            scrMin.x -= border;
            scrMin.y -= border;
            scrMax.x += border;
            scrMax.y += border;

            // Back to world space
            Vector3 third = new Vector3(scrMax.x, scrMin.y, scrMin.z);
            scrMin = cam.ScreenToWorldPoint(scrMin);
            scrMax = cam.ScreenToWorldPoint(scrMax);
            third = cam.ScreenToWorldPoint(third);

            float width = Vector3.Distance(scrMin, third);
            float height = Vector3.Distance(scrMax, third);

            quadMatrix = Matrix4x4.TRS((scrMin + scrMax) * 0.5f, cam.transform.rotation, new Vector3(width, height, 1f));
        }

        void SmoothGlow(int rtWidth, int rtHeight) {
            const int blurPasses = 4;

            // Blur buffers
            int bufferCount = blurPasses * 2;
            if (mipGlowBuffers == null || mipGlowBuffers.Length != bufferCount) {
                mipGlowBuffers = new int[bufferCount];
                for (int k = 0; k < bufferCount; k++) {
                    mipGlowBuffers[k] = Shader.PropertyToID("_HPSmoothGlowTemp" + k);
                }
                glowRT = Shader.PropertyToID("_HPComposeGlowFinal");
                mipGlowBuffers[bufferCount - 2] = glowRT;
            }
            RenderTextureDescriptor glowDesc = sourceDesc;
            glowDesc.depthBufferBits = 0;
            if (glowDesc.vrUsage == VRTextureUsage.TwoEyes) {
                glowDesc.vrUsage = VRTextureUsage.None;
                fxMatBlurGlow.SetFloat(ShaderParams.StereoRendering, 0.5f);
                fxMatComposeGlow.SetFloat(ShaderParams.StereoRendering, 0.5f);
            } else {
                fxMatBlurGlow.SetFloat(ShaderParams.StereoRendering, 1f);
                fxMatComposeGlow.SetFloat(ShaderParams.StereoRendering, 1f);
            }

            for (int k = 0; k < bufferCount; k++) {
                float reduction = k / 2 + 2;
                int reducedWidth = (int)(rtWidth / reduction);
                int reducedHeight = (int)(rtHeight / reduction);
                if (reducedWidth <= 0) {
                    reducedWidth = 1;
                }
                if (reducedHeight <= 0) {
                    reducedHeight = 1;
                }
                glowDesc.width = reducedWidth;
                glowDesc.height = reducedHeight;
                cbHighlight.GetTemporaryRT(mipGlowBuffers[k], glowDesc, FilterMode.Bilinear);
            }

            for (int k = 0; k < bufferCount - 1; k += 2) {
                if (k == 0) {
                    cbHighlight.Blit(sourceRT, mipGlowBuffers[k + 1], fxMatBlurGlow, 0);
                } else {
                    cbHighlight.Blit(mipGlowBuffers[k], mipGlowBuffers[k + 1], fxMatBlurGlow, 0);
                }
                cbHighlight.Blit(mipGlowBuffers[k + 1], mipGlowBuffers[k], fxMatBlurGlow, 1);

                if (k < bufferCount - 2) {
                    cbHighlight.Blit(mipGlowBuffers[k], mipGlowBuffers[k + 2], fxMatBlurGlow, 2);
                }
            }
        }

        void SmoothOutline(int rtWidth, int rtHeight) {
            const int blurPasses = 2;

            // Blur buffers
            int bufferCount = blurPasses * 2;
            if (mipOutlineBuffers == null || mipOutlineBuffers.Length != bufferCount) {
                mipOutlineBuffers = new int[bufferCount];
                for (int k = 0; k < bufferCount; k++) {
                    mipOutlineBuffers[k] = Shader.PropertyToID("_HPSmoothOutlineTemp" + k);
                }
                outlineRT = Shader.PropertyToID("_HPComposeOutlineFinal");
                mipOutlineBuffers[bufferCount - 2] = outlineRT;
            }
            RenderTextureDescriptor outlineDesc = sourceDesc;
            outlineDesc.depthBufferBits = 0;
            if (outlineDesc.vrUsage == VRTextureUsage.TwoEyes) {
                outlineDesc.vrUsage = VRTextureUsage.None;
                fxMatBlurOutline.SetFloat(ShaderParams.StereoRendering, 0.5f);
                fxMatComposeOutline.SetFloat(ShaderParams.StereoRendering, 0.5f);
            } else {
                fxMatBlurOutline.SetFloat(ShaderParams.StereoRendering, 1f);
                fxMatComposeOutline.SetFloat(ShaderParams.StereoRendering, 1f);
            }

            for (int k = 0; k < bufferCount; k++) {
                float reduction = k / 2 + 2;
                int reducedWidth = (int)(rtWidth / reduction);
                int reducedHeight = (int)(rtHeight / reduction);
                if (reducedWidth <= 0) {
                    reducedWidth = 1;
                }
                if (reducedHeight <= 0) {
                    reducedHeight = 1;
                }
                outlineDesc.width = reducedWidth;
                outlineDesc.height = reducedHeight;
                cbHighlight.GetTemporaryRT(mipOutlineBuffers[k], outlineDesc, FilterMode.Bilinear);
            }

            for (int k = 0; k < bufferCount - 1; k += 2) {
                if (k == 0) {
                    cbHighlight.Blit(sourceRT, mipOutlineBuffers[k + 1], fxMatBlurOutline, 0);
                } else {
                    cbHighlight.Blit(mipOutlineBuffers[k], mipOutlineBuffers[k + 1], fxMatBlurOutline, 0);
                }
                cbHighlight.Blit(mipOutlineBuffers[k + 1], mipOutlineBuffers[k], fxMatBlurOutline, 1);

                if (k < bufferCount - 2) {
                    cbHighlight.Blit(mipOutlineBuffers[k], mipOutlineBuffers[k + 2], fxMatBlurOutline, 2);
                }
            }
        }


        void ComposeSmoothBlend(Visibility smoothGlowVisibility, Visibility smoothOutlineVisibility) {

            // Render mask on target surface
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.IPhonePlayer) {
                cbHighlight.SetRenderTarget(colorAttachmentBuffer, depthAttachmentBuffer);
            } else {
                cbHighlight.SetRenderTarget(colorAttachmentBuffer);
            }

            bool renderSmoothGlow = glow > 0 && glowQuality == QualityLevel.Highest;
            if (renderSmoothGlow) {
                fxMatComposeGlow.SetVector(ShaderParams.Flip, (UnityEngine.XR.XRSettings.enabled && flipY) ? new Vector4(1, -1) : new Vector4(0, 1));
                fxMatComposeGlow.SetInt(ShaderParams.ZTest, GetZTestValue(smoothGlowVisibility));
                fxMatComposeGlow.SetColor(ShaderParams.Debug, glowBlitDebug ? debugColor : blackColor);
                cbHighlight.DrawMesh(quadMesh, quadGlowMatrix, fxMatComposeGlow, 0, 0);
            }
            bool renderSmoothOutline = outline > 0 && outlineQuality == QualityLevel.Highest;
            if (renderSmoothOutline) {
                fxMatComposeOutline.SetVector(ShaderParams.Flip, (UnityEngine.XR.XRSettings.enabled && flipY) ? new Vector4(1, -1) : new Vector4(0, 1));
                fxMatComposeOutline.SetInt(ShaderParams.ZTest, GetZTestValue(smoothOutlineVisibility));
                fxMatComposeOutline.SetColor(ShaderParams.Debug, outlineBlitDebug ? debugColor : blackColor);
                cbHighlight.DrawMesh(quadMesh, quadOutlineMatrix, fxMatComposeOutline, 0, 0);
            }
            // Release render textures
            if (renderSmoothGlow) {
                for (int k = 0; k < mipGlowBuffers.Length; k++) {
                    cbHighlight.ReleaseTemporaryRT(mipGlowBuffers[k]);
                }
            }
            if (renderSmoothOutline) {
                for (int k = 0; k < mipOutlineBuffers.Length; k++) {
                    cbHighlight.ReleaseTemporaryRT(mipOutlineBuffers[k]);
                }
            }

            cbHighlight.ReleaseTemporaryRT(sourceRT);
        }

        void InitMaterial(ref Material material, string shaderName) {
            if (material != null) return;
            Shader shaderFX = Shader.Find(shaderName);
            if (shaderFX == null) {
                Debug.LogError("Shader " + shaderName + " not found.");
                enabled = false;
                return;
            }
            material = new Material(shaderFX);
        }

        void Fork(Material mat, ref Material[] mats, Mesh mesh) {
            if (mesh == null)
                return;
            int count = mesh.subMeshCount;
            Fork(mat, ref mats, count);
        }

        void Fork(Material material, ref Material[] array, int count) {
            if (array == null || array.Length < count) {
                DestroyMaterialArray(array);
                array = new Material[count];
            }
            for (int k = 0; k < count; k++) {
                if (array[k] == null) {
                    array[k] = Instantiate(material);
                }
            }
        }

        /// <summary>
        /// Sets target for highlight effects
        /// </summary>
        public void SetTarget(Transform transform) {
            if (transform == target || transform == null)
                return;

            if (_highlighted) {
                SetHighlighted(false);
            }

            target = transform;
            SetupMaterial();
        }



        /// <summary>
        /// Sets target for highlight effects and also specify a list of renderers to be included as well
        /// </summary>
        public void SetTargets(Transform transform, Renderer[] renderers) {
            if (transform == null)
                return;

            if (_highlighted) {
                SetHighlighted(false);
            }

            effectGroup = TargetOptions.Scripting;
            target = transform;
            SetupMaterial(renderers);
        }


        /// <summary>
        /// Start or finish highlight on the object
        /// </summary>
        public void SetHighlighted(bool state) {

            if (!Application.isPlaying) {
                _highlighted = state;
                return;
            }

            if (fading == FadingState.NoFading) {
                fadeStartTime = Time.time;
            }

            if (state && !ignore) {
                if (_highlighted && fading == FadingState.NoFading) {
                    return;
                }
                if (OnObjectHighlightStart != null) {
                    if (!OnObjectHighlightStart(gameObject)) {
                        return;
                    }
                }
                SendMessage("HighlightStart", null, SendMessageOptions.DontRequireReceiver);
                highlightStartTime = targetFxStartTime = Time.time;
                if (fadeInDuration > 0) {
                    if (fading == FadingState.FadingOut) {
                        float remaining = fadeOutDuration - (Time.time - fadeStartTime);
                        fadeStartTime = Time.time - remaining;
                    }
                    fading = FadingState.FadingIn;
                } else {
                    fading = FadingState.NoFading;
                }
                _highlighted = true;
                requireUpdateMaterial = true;
            } else if (_highlighted) {
                if (fadeOutDuration > 0) {
                    if (fading == FadingState.FadingIn) {
                        float elapsed = Time.time - fadeStartTime;
                        fadeStartTime = Time.time + elapsed - fadeInDuration;
                    }
                    fading = FadingState.FadingOut; // when fade out ends, highlighted will be set to false in OnRenderObject
                } else {
                    fading = FadingState.NoFading;
                    _highlighted = false;
                    if (OnObjectHighlightEnd != null) {
                        OnObjectHighlightEnd(gameObject);
                    }
                    SendMessage("HighlightEnd", null, SendMessageOptions.DontRequireReceiver);
                    requireUpdateMaterial = true;
                }
            }
        }

        void SetupMaterial() {

#if UNITY_EDITOR
            staticChildren = false;
#endif

            if (target == null || fxMatMask == null)
                return;

            Fork(fxMatOutlineTemplate, ref fxMatOutline, 8);
            Fork(fxMatGlowTemplate, ref fxMatGlow, 8 * glowPasses.Length);

            Renderer[] rr = null;
            switch (effectGroup) {
                case TargetOptions.OnlyThisObject:
                    Renderer renderer = target.GetComponent<Renderer>();
                    if (renderer != null) {
                        rr = new Renderer[1];
                        rr[0] = renderer;
                    }
                    break;
                case TargetOptions.RootToChildren:
                    Transform root = target;
                    while (root.parent != null) {
                        root = root.parent;
                    }
                    rr = root.GetComponentsInChildren<Renderer>();
                    break;
                case TargetOptions.LayerInScene: {
                        HighlightEffect eg = this;
                        if (target != transform) {
                            HighlightEffect targetEffect = target.GetComponent<HighlightEffect>();
                            if (targetEffect != null) {
                                eg = targetEffect;
                            }
                        }
                        rr = FindRenderersWithLayerInScene(eg.effectGroupLayer);
                    }
                    break;
                case TargetOptions.LayerInChildren: {
                        HighlightEffect eg = this;
                        if (target != transform) {
                            HighlightEffect targetEffect = target.GetComponent<HighlightEffect>();
                            if (targetEffect != null) {
                                eg = targetEffect;
                            }
                        }
                        rr = FindRenderersWithLayerInChildren(eg.effectGroupLayer);
                    }
                    break;
                case TargetOptions.Children:
                    rr = target.GetComponentsInChildren<Renderer>();
                    break;
                case TargetOptions.Scripting:
                    if (rmsCount > 0) return;
                    return;
            }

            SetupMaterial(rr);
        }

        void SetupMaterial(Renderer[] rr) {

            if (rr == null) {
                rr = new Renderer[0];
            }
            if (rms == null || rms.Length < rr.Length) {
                rms = new ModelMaterials[rr.Length];
            }

            rmsCount = 0;
            for (int k = 0; k < rr.Length; k++) {
                rms[rmsCount].Init();
                Renderer renderer = rr[k];
                if (!string.IsNullOrEmpty(effectNameFilter)) {
                    if (!renderer.name.Contains(effectNameFilter)) continue;
                }
                rms[rmsCount].renderer = renderer;
                rms[rmsCount].renderWasVisibleDuringSetup = renderer.isVisible;


                if (renderer.transform != target) {
                    HighlightEffect otherEffect = renderer.GetComponent<HighlightEffect>();
                    if (otherEffect != null && otherEffect.enabled) {
                        otherEffect.highlighted = highlighted;
                        continue; // independent subobject
                    }
                }

                if (OnRendererHighlightStart != null) {
                    if (!OnRendererHighlightStart(renderer)) {
                        rmsCount++;
                        continue;
                    }
                }

                rms[rmsCount].isCombined = false;
                bool isSkinnedMesh = renderer is SkinnedMeshRenderer;
                rms[rmsCount].isSkinnedMesh = isSkinnedMesh;
                rms[rmsCount].normalsOption = isSkinnedMesh ? NormalsOption.PreserveOriginal : normalsOption;
                CheckCommandBuffers();
                if (isSkinnedMesh) {
                    // ignore cloth skinned renderers
                    rms[rmsCount].isSkinnedMesh = true;
                    rms[rmsCount].mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                } else if (Application.isPlaying && renderer.isPartOfStaticBatch) {
                    // static batched objects need to have a mesh collider in order to use its original mesh
                    MeshCollider mc = renderer.GetComponent<MeshCollider>();
                    if (mc != null) {
                        rms[rmsCount].mesh = mc.sharedMesh;
                    }
                } else {
                    MeshFilter mf = renderer.GetComponent<MeshFilter>();
                    if (mf != null) {
                        rms[rmsCount].mesh = mf.sharedMesh;

#if UNITY_EDITOR
                        if (renderer.gameObject.isStatic && renderer.GetComponent<MeshCollider>() == null) {
                            staticChildren = true;
                        }
#endif

                    }
                }

                if (rms[rmsCount].mesh == null) {
                    continue;
                }

                rms[rmsCount].transform = renderer.transform;
                Fork(fxMatMask, ref rms[rmsCount].fxMatMask, rms[rmsCount].mesh);
                Fork(fxMatSeeThrough, ref rms[rmsCount].fxMatSeeThroughInner, rms[rmsCount].mesh);
                Fork(fxMatSeeThroughBorder, ref rms[rmsCount].fxMatSeeThroughBorder, rms[rmsCount].mesh);
                Fork(fxMatOverlay, ref rms[rmsCount].fxMatOverlay, rms[rmsCount].mesh);
                Fork(fxMatInnerGlow, ref rms[rmsCount].fxMatInnerGlow, rms[rmsCount].mesh);
                Fork(fxMatSolidColor, ref rms[rmsCount].fxMatSolidColor, rms[rmsCount].mesh);
                rms[rmsCount].originalMesh = rms[rmsCount].mesh;
                if (!rms[rmsCount].preserveOriginalMesh) {
                    if (innerGlow > 0 || (glow > 0 && glowQuality != QualityLevel.Highest) || (outline > 0 && outlineQuality != QualityLevel.Highest)) {
                        if (normalsOption == NormalsOption.Reorient) {
                            ReorientNormals(rmsCount);
                        } else {
                            AverageNormals(rmsCount);
                        }
                    }
                }
                rmsCount++;
            }

#if UNITY_EDITOR
            // Avoids command buffer issue when refreshing asset inside the Editor
            if (!Application.isPlaying) {
                mipGlowBuffers = null;
                mipOutlineBuffers = null;
            }
#endif

            if (combineMeshes) {
                CombineMeshes();
            }

            UpdateMaterialProperties();
        }

        List<Renderer> tempRR;

        Renderer[] FindRenderersWithLayerInScene(LayerMask layer) {
            Renderer[] rr = FindObjectsOfType<Renderer>();
            if (tempRR == null) {
                tempRR = new List<Renderer>();
            } else {
                tempRR.Clear();
            }
            for (var i = 0; i < rr.Length; i++) {
                Renderer r = rr[i];
                if (((1 << r.gameObject.layer) & layer) != 0) {
                    tempRR.Add(r);
                }
            }
            return tempRR.ToArray();
        }

        Renderer[] FindRenderersWithLayerInChildren(LayerMask layer) {
            Renderer[] rr = target.GetComponentsInChildren<Renderer>();
            if (tempRR == null) {
                tempRR = new List<Renderer>();
            } else {
                tempRR.Clear();
            }
            for (var i = 0; i < rr.Length; i++) {
                Renderer r = rr[i];
                if (((1 << r.gameObject.layer) & layer) != 0) {
                    tempRR.Add(r);
                }
            }
            return tempRR.ToArray();
        }

        void CheckGeometrySupportDependencies() {
            InitMaterial(ref fxMatMask, "HighlightPlus/Geometry/Mask");
            InitMaterial(ref fxMatGlowTemplate, "HighlightPlus/Geometry/Glow");
            if (fxMatGlowTemplate != null && useGPUInstancing) fxMatGlowTemplate.enableInstancing = true;
            InitMaterial(ref fxMatInnerGlow, "HighlightPlus/Geometry/InnerGlow");
            InitMaterial(ref fxMatOutlineTemplate, "HighlightPlus/Geometry/Outline");
            if (fxMatOutlineTemplate != null && useGPUInstancing) fxMatOutlineTemplate.enableInstancing = true;
            InitMaterial(ref fxMatOverlay, "HighlightPlus/Geometry/Overlay");
            InitMaterial(ref fxMatSeeThrough, "HighlightPlus/Geometry/SeeThrough");
            InitMaterial(ref fxMatSeeThroughBorder, "HighlightPlus/Geometry/SeeThroughBorder");
            InitMaterial(ref fxMatTarget, "HighlightPlus/Geometry/Target");
            InitMaterial(ref fxMatComposeGlow, "HighlightPlus/Geometry/ComposeGlow");
            InitMaterial(ref fxMatComposeOutline, "HighlightPlus/Geometry/ComposeOutline");
            InitMaterial(ref fxMatSolidColor, "HighlightPlus/Geometry/SolidColor");
            InitMaterial(ref fxMatBlurGlow, "HighlightPlus/Geometry/BlurGlow");
            InitMaterial(ref fxMatBlurOutline, "HighlightPlus/Geometry/BlurOutline");
            InitMaterial(ref fxMatClearStencil, "HighlightPlus/ClearStencil");
        }

        void CheckCommandBuffers() {
            if (cbHighlight == null) {
                cbHighlight = new CommandBuffer();
                cbHighlight.name = "Highlight Plus for " + name;
            }
            if (cbSeeThrough == null) {
                cbSeeThrough = new CommandBuffer();
                cbSeeThrough.name = "See Through";
            }
            if (cbOverlay == null) {
                cbOverlay = new CommandBuffer();
                cbOverlay.name = "Overlay";
            }
            if (cbInnerGlow == null) {
                cbInnerGlow = new CommandBuffer();
                cbInnerGlow.name = "Inner Glow";
            }
        }


        public void UpdateMaterialProperties() {

            if (rms == null)
                return;

            if (ignore) {
                _highlighted = false;
            }

            Color seeThroughTintColor = this.seeThroughTintColor;
            seeThroughTintColor.a = this.seeThroughTintAlpha;

            if (lastOutlineVisibility != outlineVisibility) {
                // change by scripting?
                if (glowQuality == QualityLevel.Highest && outlineQuality == QualityLevel.Highest) {
                    glowVisibility = outlineVisibility;
                }
                lastOutlineVisibility = outlineVisibility;
            }
            if (outlineWidth < 0) {
                outlineWidth = 0;
            }
            if (outlineQuality == QualityLevel.Medium) {
                outlineOffsetsMin = 4; outlineOffsetsMax = 7;
            } else if (outlineQuality == QualityLevel.High) {
                outlineOffsetsMin = 0; outlineOffsetsMax = 7;
            } else {
                outlineOffsetsMin = outlineOffsetsMax = 0;
            }
            if (glowWidth < 0) {
                glowWidth = 0;
            }
            if (glowQuality == QualityLevel.Medium) {
                glowOffsetsMin = 4; glowOffsetsMax = 7;
            } else if (glowQuality == QualityLevel.High) {
                glowOffsetsMin = 0; glowOffsetsMax = 7;
            } else {
                glowOffsetsMin = glowOffsetsMax = 0;
            }
            if (targetFXTransitionDuration <= 0) {
                targetFXTransitionDuration = 0.0001f;
            }
            if (targetFXStayDuration <= 0) {
                targetFXStayDuration = 0.0001f;
            }

            useSmoothGlow = glow > 0 && glowQuality == QualityLevel.Highest;
            useSmoothOutline = outline > 0 && outlineQuality == QualityLevel.Highest;
            useSmoothBlend = useSmoothGlow || useSmoothOutline;
            if (useSmoothBlend) {
                if (useSmoothGlow && useSmoothOutline) {
                    outlineVisibility = glowVisibility;
                }
            }
            if (useSmoothGlow) {
                fxMatComposeGlow.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                fxMatBlurGlow.SetFloat(ShaderParams.BlurScale, glowWidth / glowDownsampling);
                fxMatBlurGlow.SetFloat(ShaderParams.Speed, glowAnimationSpeed);
            }

            if (useSmoothOutline) {
                fxMatComposeOutline.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                fxMatBlurOutline.SetFloat(ShaderParams.BlurScale, outlineWidth / outlineDownsampling);
            }

            // Setup materials
            for (int k = 0; k < rmsCount; k++) {
                if (rms[k].mesh != null) {

                    Renderer renderer = rms[k].renderer;
                    if (renderer == null)
                        continue;

                    Material mat = null;
                    if (renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0) {
                        mat = renderer.sharedMaterials[0];
                    }

                    bool hasTexture = mat != null && (mat.HasProperty(ShaderParams.MainTex) || mat.HasProperty(ShaderParams.BaseMap));
                    bool useAlphaTest = alphaCutOff > 0 && hasTexture;
                    Texture matTexture = hasTexture ? mat.mainTexture : null;
                    Vector2 matTextureOffset = hasTexture ? mat.mainTextureOffset : Vector2.zero;
                    Vector2 matTextureScale = hasTexture ? mat.mainTextureScale : Vector2.one;

                    // Outline
                    float scaledOutlineWidth = outlineQuality.UsesMultipleOffsets() ? 0f : outlineWidth / 100f;
                    for (int m = 0; m < fxMatOutline.Length; m++) {
                        Material fxMat = fxMatOutline[m];
                        fxMat.SetFloat(ShaderParams.OutlineWidth, scaledOutlineWidth);
                        fxMat.SetVector(ShaderParams.OutlineDirection, Vector4.zero);
                        fxMat.SetInt(ShaderParams.OutlineZTest, GetZTestValue(outlineVisibility));
                        fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                        fxMat.SetFloat(ShaderParams.ConstantWidth, constantWidth ? 1.0f : 0);
                        if (useAlphaTest) {
                            fxMat.mainTexture = matTexture;
                            fxMat.mainTextureOffset = matTextureOffset;
                            fxMat.mainTextureScale = matTextureScale;
                            fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                            fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                        } else {
                            fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                        }
                    }

                    // Glow
                    for (int m = 0; m < fxMatGlow.Length; m++) {
                        Material fxMat = fxMatGlow[m];
                        fxMat.SetVector(ShaderParams.Glow2, new Vector4(outline > 0 ? outlineWidth / 100f : 0, glowAnimationSpeed, glowDithering ? 0 : 1));
                        fxMat.SetInt(ShaderParams.GlowZTest, GetZTestValue(glowVisibility));
                        fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                        fxMat.SetFloat(ShaderParams.ConstantWidth, constantWidth ? 1.0f : 0);
                        fxMat.SetInt(ShaderParams.GlowStencilOp, glowBlendPasses ? (int)StencilOp.Keep : (int)StencilOp.Replace);
                        if (useAlphaTest) {
                            fxMat.mainTexture = matTexture;
                            fxMat.mainTextureOffset = matTextureOffset;
                            fxMat.mainTextureScale = matTextureScale;
                            fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                            fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                        } else {
                            fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                        }

                    }

                    // Target
                    if (targetFX) {
                        if (targetFXTexture == null) {
                            targetFXTexture = Resources.Load<Texture2D>("HighlightPlus/target");
                        }
                        fxMatTarget.mainTexture = targetFXTexture;
                    }

                    // Mask, See-through & Overlay per submesh
                    for (int l = 0; l < rms[k].mesh.subMeshCount; l++) {
                        if (((1 << l) & subMeshMask) == 0) continue;

                        mat = null;
                        renderer.GetSharedMaterials(rendererSharedMaterials);
                        if (l < rendererSharedMaterials.Count) {
                            mat = rendererSharedMaterials[l];
                        }
                        if (mat == null)
                            continue;

                        hasTexture = mat.HasProperty("_MainTex");
                        useAlphaTest = alphaCutOff > 0 && hasTexture;

                        // Mask
                        if (rms[k].fxMatMask != null && rms[k].fxMatMask.Length > l) {
                            Material fxMat = rms[k].fxMatMask[l];
                            if (fxMat != null) {
                                if (hasTexture) {
                                    fxMat.mainTexture = matTexture;
                                    fxMat.mainTextureOffset = matTextureOffset;
                                    fxMat.mainTextureScale = matTextureScale;
                                }
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                                fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                            }
                        }

                        // See-through
                        if (rms[k].fxMatSeeThroughInner != null && rms[k].fxMatSeeThroughInner.Length > l) {
                            Material fxMat = rms[k].fxMatSeeThroughInner[l];
                            if (fxMat != null) {
                                fxMat.SetFloat(ShaderParams.SeeThrough, seeThroughIntensity);
                                fxMat.SetFloat(ShaderParams.SeeThroughNoise, seeThroughNoise);
                                fxMat.SetColor(ShaderParams.SeeThroughTintColor, seeThroughTintColor);
                                fxMat.SetFloat(ShaderParams.SeeThroughBorderWidth, (seeThroughBorder * seeThroughBorderWidth) > 0 ? seeThroughBorderWidth / 100f : 0);
                                fxMat.SetFloat(ShaderParams.SeeThroughBorderConstantWidth, constantWidth ? 1.0f : 0);
                                if (hasTexture) {
                                    Texture texture = mat.mainTexture;
                                    fxMat.mainTexture = texture;
                                    fxMat.mainTextureOffset = mat.mainTextureOffset;
                                    fxMat.mainTextureScale = mat.mainTextureScale;
                                }
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                            }
                        }

                        // See-through border
                        if (rms[k].fxMatSeeThroughBorder != null && rms[k].fxMatSeeThroughBorder.Length > l) {
                            Material fxMat = rms[k].fxMatSeeThroughBorder[l];
                            if (fxMat != null) {
                                fxMat.SetColor(ShaderParams.SeeThroughBorderColor, new Color(seeThroughBorderColor.r, seeThroughBorderColor.g, seeThroughBorderColor.b, seeThroughBorder));
                                if (hasTexture) {
                                    Texture texture = mat.mainTexture;
                                    fxMat.mainTexture = texture;
                                    fxMat.mainTextureOffset = mat.mainTextureOffset;
                                    fxMat.mainTextureScale = mat.mainTextureScale;
                                }
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                            }
                        }

                        // Overlay
                        if (rms[k].fxMatOverlay != null && rms[k].fxMatOverlay.Length > l) {
                            Material fxMat = rms[k].fxMatOverlay[l];
                            if (fxMat != null) {
                                if (hasTexture) {
                                    fxMat.mainTexture = matTexture;
                                    fxMat.mainTextureOffset = matTextureOffset;
                                    fxMat.mainTextureScale = matTextureScale;
                                }
                                if (mat.HasProperty(ShaderParams.Color)) {
                                    fxMat.SetColor(ShaderParams.OverlayBackColor, mat.GetColor(ShaderParams.Color));
                                }
                                fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                            }
                        }

                        // Inner Glow
                        if (rms[k].fxMatInnerGlow != null && rms[k].fxMatInnerGlow.Length > l) {
                            Material fxMat = rms[k].fxMatInnerGlow[l];
                            if (fxMat != null) {
                                if (hasTexture) {
                                    fxMat.mainTexture = matTexture;
                                    fxMat.mainTextureOffset = matTextureOffset;
                                    fxMat.mainTextureScale = matTextureScale;
                                }
                                fxMat.SetFloat(ShaderParams.InnerGlowWidth, innerGlowWidth);
                                fxMat.SetInt(ShaderParams.InnerGlowZTest, GetZTestValue(innerGlowVisibility));
                                fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                            }
                        }

                        // Solid Color for smooth glow
                        if (rms[k].fxMatSolidColor != null && rms[k].fxMatSolidColor.Length > l) {
                            Material fxMat = rms[k].fxMatSolidColor[l];
                            if (fxMat != null) {
                                fxMat.color = glowHQColor;
                                fxMat.SetInt(ShaderParams.Cull, cullBackFaces ? (int)CullMode.Back : (int)CullMode.Off);
                                if (hasTexture) {
                                    fxMat.mainTexture = matTexture;
                                    fxMat.mainTextureOffset = matTextureOffset;
                                    fxMat.mainTextureScale = matTextureScale;
                                }
                                //                                if (!Application.isMobilePlatform) { // TODO: currently this does not work with URP on Android
                                if ((glow > 0 && glowQuality == QualityLevel.Highest && glowVisibility == Visibility.Normal) || (outline > 0 && outlineQuality == QualityLevel.Highest && outlineVisibility == Visibility.Normal)) {
                                    fxMat.EnableKeyword(ShaderParams.SKW_DEPTHCLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_DEPTHCLIP);
                                }
                                //}
                                if (useAlphaTest) {
                                    fxMat.SetFloat(ShaderParams.CutOff, alphaCutOff);
                                    fxMat.EnableKeyword(ShaderParams.SKW_ALPHACLIP);
                                } else {
                                    fxMat.DisableKeyword(ShaderParams.SKW_ALPHACLIP);
                                }
                            }
                        }
                    }
                }
            }
        }

        int GetZTestValue(Visibility param) {
            switch (param) {
                case Visibility.AlwaysOnTop: return (int)UnityEngine.Rendering.CompareFunction.Always;
                case Visibility.OnlyWhenOccluded: return (int)UnityEngine.Rendering.CompareFunction.Greater;
                default:
                    return (int)UnityEngine.Rendering.CompareFunction.LessEqual;
            }
        }

        void BuildQuad() {
            quadMesh = new Mesh();

            // Setup vertices
            Vector3[] newVertices = new Vector3[4];
            float halfHeight = 0.5f;
            float halfWidth = 0.5f;
            newVertices[0] = new Vector3(-halfWidth, -halfHeight, 0);
            newVertices[1] = new Vector3(-halfWidth, halfHeight, 0);
            newVertices[2] = new Vector3(halfWidth, -halfHeight, 0);
            newVertices[3] = new Vector3(halfWidth, halfHeight, 0);

            // Setup UVs
            Vector2[] newUVs = new Vector2[newVertices.Length];
            newUVs[0] = new Vector2(0, 0);
            newUVs[1] = new Vector2(0, 1);
            newUVs[2] = new Vector2(1, 0);
            newUVs[3] = new Vector2(1, 1);

            // Setup triangles
            int[] newTriangles = { 0, 1, 2, 3, 2, 1 };

            // Setup normals
            Vector3[] newNormals = new Vector3[newVertices.Length];
            for (int i = 0; i < newNormals.Length; i++) {
                newNormals[i] = Vector3.forward;
            }

            // Create quad
            quadMesh.vertices = newVertices;
            quadMesh.uv = newUVs;
            quadMesh.triangles = newTriangles;
            quadMesh.normals = newNormals;

            quadMesh.RecalculateBounds();
        }


        /// <summary>
        /// Returns true if a given transform is included in this effect
        /// </summary>
        public bool Includes(Transform transform) {
            for (int k = 0; k < rmsCount; k++) {
                if (rms[k].transform == transform) return true;
            }
            return false;
        }


        #region Normals handling

        static List<Vector3> vertices;
        static List<Vector3> normals;
        static Vector3[] newNormals;
        static int[] matches;
        static readonly Dictionary<Vector3, int> vv = new Dictionary<Vector3, int>();
        static readonly Dictionary<int, Mesh> smoothMeshes = new Dictionary<int, Mesh>();
        static readonly Dictionary<int, Mesh> reorientedMeshes = new Dictionary<int, Mesh>();
        static readonly Dictionary<int, Mesh> combinedMeshes = new Dictionary<int, Mesh>();
        static readonly List<Material> rendererSharedMaterials = new List<Material>();
        int combinedMeshesHashId;

        void AverageNormals(int objIndex) {
            if (rms == null || objIndex >= rms.Length) return;
            Mesh mesh = rms[objIndex].mesh;

            Mesh newMesh;
            int hashCode = mesh.GetHashCode();
            if (!smoothMeshes.TryGetValue(hashCode, out newMesh)) {
                if (!mesh.isReadable) return;
                if (normals == null) {
                    normals = new List<Vector3>();
                } else {
                    normals.Clear();
                }
                mesh.GetNormals(normals);
                int normalsCount = normals.Count;
                if (normalsCount == 0)
                    return;
                if (vertices == null) {
                    vertices = new List<Vector3>();
                } else {
                    vertices.Clear();
                }
                mesh.GetVertices(vertices);
                int vertexCount = vertices.Count;
                if (normalsCount < vertexCount) {
                    vertexCount = normalsCount;
                }
                if (newNormals == null || newNormals.Length < vertexCount) {
                    newNormals = new Vector3[vertexCount];
                } else {
                    Vector3 zero = Vector3.zero;
                    for (int k = 0; k < vertexCount; k++) {
                        newNormals[k] = zero;
                    }
                }
                if (matches == null || matches.Length < vertexCount) {
                    matches = new int[vertexCount];
                }
                // Locate overlapping vertices
                vv.Clear();
                for (int k = 0; k < vertexCount; k++) {
                    int i;
                    Vector3 v = vertices[k];
                    if (!vv.TryGetValue(v, out i)) {
                        vv[v] = i = k;
                    }
                    matches[k] = i;
                }
                // Average normals
                for (int k = 0; k < vertexCount; k++) {
                    int match = matches[k];
                    newNormals[match] += normals[k];
                }
                for (int k = 0; k < vertexCount; k++) {
                    int match = matches[k];
                    normals[k] = newNormals[match].normalized;
                }
                // Reassign normals
                newMesh = Instantiate(mesh);
                newMesh.hideFlags = HideFlags.DontSave;
                newMesh.SetNormals(normals);
                smoothMeshes[hashCode] = newMesh;
            }
            rms[objIndex].mesh = newMesh;
        }


        void ReorientNormals(int objIndex) {
            if (rms == null || objIndex >= rms.Length) return;
            Mesh mesh = rms[objIndex].mesh;

            Mesh newMesh;
            int hashCode = mesh.GetHashCode();
            if (!reorientedMeshes.TryGetValue(hashCode, out newMesh)) {
                if (!mesh.isReadable) return;
                if (normals == null) {
                    normals = new List<Vector3>();
                } else {
                    normals.Clear();
                }
                if (vertices == null) {
                    vertices = new List<Vector3>();
                } else {
                    vertices.Clear();
                }
                mesh.GetVertices(vertices);
                int vertexCount = vertices.Count;
                if (vertexCount == 0) return;

                Vector3 mid = Vector3.zero;
                for (int k = 0; k < vertexCount; k++) {
                    mid += vertices[k];
                }
                mid /= vertexCount;
                // Reorient normals
                for (int k = 0; k < vertexCount; k++) {
                    normals.Add((vertices[k] - mid).normalized);
                }
                // Reassign normals
                newMesh = Instantiate(mesh);
                newMesh.hideFlags = HideFlags.DontSave;
                newMesh.SetNormals(normals);
                reorientedMeshes[hashCode] = newMesh;
            }
            rms[objIndex].mesh = newMesh;
        }

        const int MAX_VERTEX_COUNT = 65535;
        void CombineMeshes() {
            // Combine meshes of group into the first mesh in rms
            if (combineInstances == null || combineInstances.Length != rmsCount) {
                combineInstances = new CombineInstance[rmsCount];
            }
            int first = -1;
            int count = 0;
            combinedMeshesHashId = 0;
            int vertexCount = 0;
            Matrix4x4 im = Matrix4x4Identity;
            for (int k = 0; k < rmsCount; k++) {
                combineInstances[k].mesh = null;
                if (!rms[k].isSkinnedMesh) {
                    Mesh mesh = rms[k].mesh;
                    if (mesh != null && mesh.isReadable) {
                        if (!string.IsNullOrEmpty(effectNameFilter)) {
                            if (!rms[k].transform.name.Contains(effectNameFilter)) continue;
                        }
                        if (vertexCount + mesh.vertexCount > MAX_VERTEX_COUNT) continue;

                        combineInstances[k].mesh = mesh;
                        int instanceId = rms[k].renderer.gameObject.GetInstanceID();
                        if (first < 0) {
                            first = k;
                            combinedMeshesHashId = instanceId;
                            im = rms[k].transform.worldToLocalMatrix;
                        } else {
                            combinedMeshesHashId ^= instanceId;
                            rms[k].mesh = null;
                        }
                        combineInstances[k].transform = im * rms[k].transform.localToWorldMatrix;
                        count++;
                    }
                }
            }
            if (count < 2) return;

            Mesh combinedMesh;
            if (!combinedMeshes.TryGetValue(combinedMeshesHashId, out combinedMesh) || combinedMesh == null) {
                combinedMesh = new Mesh();
                combinedMesh.CombineMeshes(combineInstances, true, true);
                combinedMeshes[combinedMeshesHashId] = combinedMesh;
            }
            rms[first].mesh = combinedMesh;
            rms[first].isCombined = true;
        }

        #endregion

    }
}


