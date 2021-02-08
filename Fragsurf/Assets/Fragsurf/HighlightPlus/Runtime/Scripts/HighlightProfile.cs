using UnityEngine;

namespace HighlightPlus {

	[CreateAssetMenu (menuName = "Highlight Plus Profile", fileName = "Highlight Plus Profile", order = 100)]
	public class HighlightProfile : ScriptableObject {

		public TargetOptions effectGroup = TargetOptions.Children;
		public LayerMask effectGroupLayer = -1;
		public string effectNameFilter;
        public bool combineMeshes;
		[Range(0,1)]
		public float alphaCutOff = 0;
		public bool cullBackFaces = true;
        public NormalsOption normalsOption;

		public float fadeInDuration;
		public float fadeOutDuration;
		public bool constantWidth = true;

		[Range (0, 1)]
		public float overlay = 0.5f;
		[ColorUsage(true, true)] public Color overlayColor = Color.yellow;
		public float overlayAnimationSpeed = 1f;
		[Range (0, 1)]
		public float overlayMinIntensity = 0.5f;
		[Range (0, 1)]
		public float overlayBlending = 1.0f;

		[Range (0, 1)]
		public float outline = 1f;
		[ColorUsage(true, true)] public Color outlineColor = Color.black;
		public float outlineWidth = 0.45f;
		public QualityLevel outlineQuality = QualityLevel.High;
		[Range(1, 8)]
		public int outlineDownsampling = 2;

		public Visibility outlineVisibility = Visibility.Normal;
		public bool outlineIndependent;

		[Range (0, 5)]
		public float glow = 1f;
		public float glowWidth = 0.4f;
		public QualityLevel glowQuality = QualityLevel.High;
		[Range(1, 8)]
		public int glowDownsampling = 2;
		[ColorUsage(true, true)] public Color glowHQColor = new Color (0.64f, 1f, 0f, 1f);
		public bool glowDithering = true;
		public float glowMagicNumber1 = 0.75f;
		public float glowMagicNumber2 = 0.5f;
		public float glowAnimationSpeed = 1f;
		public Visibility glowVisibility = Visibility.Normal;
        public bool glowBlendPasses = true;
		public GlowPassData[] glowPasses;

		[Range (0, 5f)]
		public float innerGlow = 0f;
		[Range (0, 2)]
		public float innerGlowWidth = 1f;
		[ColorUsage(true, true)] public Color innerGlowColor = Color.white;
		public Visibility innerGlowVisibility = Visibility.Normal;

		public bool targetFX;
		public Texture2D targetFXTexture;
		[ColorUsage(true, true)] public Color targetFXColor = Color.white;
		public float targetFXRotationSpeed = 50f;
		public float targetFXInitialScale = 4f;
		public float targetFXEndScale = 1.5f;
		public float targetFXTransitionDuration = 0.5f;
		public float targetFXStayDuration = 1.5f;

		public SeeThroughMode seeThrough;
		[Range (0, 5f)]
		public float seeThroughIntensity = 0.8f;
		[Range (0, 1)]
		public float seeThroughTintAlpha = 0.5f;
		[ColorUsage(true, true)] public Color seeThroughTintColor = Color.red;
        [Range(0, 1)]
        public float seeThroughNoise = 1f;
        [Range(0, 1)] public float seeThroughBorder;
        public Color seeThroughBorderColor = Color.black;
        public float seeThroughBorderWidth = 0.45f;


		public void Load (HighlightEffect effect) {
			effect.effectGroup = effectGroup;
			effect.effectGroupLayer = effectGroupLayer;
			effect.effectNameFilter = effectNameFilter;
            effect.combineMeshes = combineMeshes;
			effect.alphaCutOff = alphaCutOff;
			effect.cullBackFaces = cullBackFaces;
            effect.normalsOption = normalsOption;
			effect.fadeInDuration = fadeInDuration;
			effect.fadeOutDuration = fadeOutDuration;
			effect.constantWidth = constantWidth;
			effect.overlay = overlay;
			effect.overlayColor = overlayColor;
			effect.overlayAnimationSpeed = overlayAnimationSpeed;
			effect.overlayMinIntensity = overlayMinIntensity;
			effect.overlayBlending = overlayBlending;
			effect.outline = outline;
			effect.outlineColor = outlineColor;
			effect.outlineWidth = outlineWidth;
			effect.outlineQuality = outlineQuality;
			effect.outlineDownsampling = outlineDownsampling;
			effect.outlineVisibility = outlineVisibility;
			effect.outlineIndependent = outlineIndependent;
			effect.glow = glow;
			effect.glowWidth = glowWidth;
			effect.glowQuality = glowQuality;
			effect.glowDownsampling = glowDownsampling;
			effect.glowHQColor = glowHQColor;
			effect.glowDithering = glowDithering;
			effect.glowMagicNumber1 = glowMagicNumber1;
			effect.glowMagicNumber2 = glowMagicNumber2;
			effect.glowAnimationSpeed = glowAnimationSpeed;
			effect.glowVisibility = glowVisibility;
            effect.glowBlendPasses = glowBlendPasses;
			effect.glowPasses = GetGlowPassesCopy (glowPasses);
			effect.innerGlow = innerGlow;
			effect.innerGlowWidth = innerGlowWidth;
			effect.innerGlowColor = innerGlowColor;
			effect.innerGlowVisibility = innerGlowVisibility;
			effect.targetFX = targetFX;
			effect.targetFXColor = targetFXColor;
			effect.targetFXEndScale = targetFXEndScale;
			effect.targetFXInitialScale = targetFXInitialScale;
			effect.targetFXRotationSpeed = targetFXRotationSpeed;
			effect.targetFXStayDuration = targetFXStayDuration;
			effect.targetFXTexture = targetFXTexture;
			effect.targetFXTransitionDuration = targetFXTransitionDuration;
			effect.seeThrough = seeThrough;
			effect.seeThroughIntensity = seeThroughIntensity;
			effect.seeThroughTintAlpha = seeThroughTintAlpha;
			effect.seeThroughTintColor = seeThroughTintColor;
            effect.seeThroughNoise = seeThroughNoise;
            effect.seeThroughBorder = seeThroughBorder;
            effect.seeThroughBorderColor = seeThroughBorderColor;
            effect.seeThroughBorderWidth = seeThroughBorderWidth;
		}

		public void Save (HighlightEffect effect) {
			effectGroup = effect.effectGroup;
			effectGroupLayer = effect.effectGroupLayer;
			effectNameFilter = effect.effectNameFilter;
            combineMeshes = effect.combineMeshes;
			alphaCutOff = effect.alphaCutOff;
			cullBackFaces = effect.cullBackFaces;
            normalsOption = effect.normalsOption;
			fadeInDuration = effect.fadeInDuration;
			fadeOutDuration = effect.fadeOutDuration;
			constantWidth = effect.constantWidth;
			overlay = effect.overlay;
			overlayColor = effect.overlayColor;
			overlayAnimationSpeed = effect.overlayAnimationSpeed;
			overlayMinIntensity = effect.overlayMinIntensity;
			overlayBlending = effect.overlayBlending;
			outline = effect.outline;
			outlineColor = effect.outlineColor;
			outlineWidth = effect.outlineWidth;
			outlineQuality = effect.outlineQuality;
			outlineDownsampling = effect.outlineDownsampling;
            outlineVisibility = effect.outlineVisibility;
			outlineIndependent = effect.outlineIndependent;
			glow = effect.glow;
			glowWidth = effect.glowWidth;
			glowQuality = effect.glowQuality;
			glowDownsampling = effect.glowDownsampling;
			glowHQColor = effect.glowHQColor;
			glowDithering = effect.glowDithering;
			glowMagicNumber1 = effect.glowMagicNumber1;
			glowMagicNumber2 = effect.glowMagicNumber2;
			glowAnimationSpeed = effect.glowAnimationSpeed;
            glowVisibility = effect.glowVisibility;
            glowBlendPasses = effect.glowBlendPasses;
			glowPasses = GetGlowPassesCopy (effect.glowPasses);
			innerGlow = effect.innerGlow;
			innerGlowWidth = effect.innerGlowWidth;
			innerGlowColor = effect.innerGlowColor;
            innerGlowVisibility = effect.innerGlowVisibility;
			targetFX = effect.targetFX;
			targetFXColor = effect.targetFXColor;
			targetFXEndScale = effect.targetFXEndScale;
			targetFXInitialScale = effect.targetFXInitialScale;
			targetFXRotationSpeed = effect.targetFXRotationSpeed;
			targetFXStayDuration = effect.targetFXStayDuration;
			targetFXTexture = effect.targetFXTexture;
			targetFXTransitionDuration = effect.targetFXTransitionDuration;
			seeThrough = effect.seeThrough;
			seeThroughIntensity = effect.seeThroughIntensity;
			seeThroughTintAlpha = effect.seeThroughTintAlpha;
			seeThroughTintColor = effect.seeThroughTintColor;
            seeThroughNoise = effect.seeThroughNoise;
            seeThroughBorder = effect.seeThroughBorder;
            seeThroughBorderColor = effect.seeThroughBorderColor;
            seeThroughBorderWidth = effect.seeThroughBorderWidth;
		}

		GlowPassData[] GetGlowPassesCopy (GlowPassData[] glowPasses) {
			if (glowPasses == null) {
				return new GlowPassData[0];
			}
			GlowPassData[] pd = new GlowPassData[glowPasses.Length];
			for (int k = 0; k < glowPasses.Length; k++) {
				pd [k].alpha = glowPasses [k].alpha;
				pd [k].color = glowPasses [k].color;
				pd [k].offset = glowPasses [k].offset;
			}
			return pd;
		}
	}
}

