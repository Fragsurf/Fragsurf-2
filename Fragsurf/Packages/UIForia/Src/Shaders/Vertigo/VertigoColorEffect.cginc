#ifndef INCLUDE_VERTIGO_EFFECT_COLOR
#define INCLUDE_VERTIGO_EFFECT_COLOR

#define TONE_TYPE_GRAYSCALE 0
#define TONE_TYPE_SEPIA 1
#define TONE_TYPE_NEGATE 2

inline fixed2 VertigoPixelize(half2 texCoord, fixed effectFactor, half2 tileSettings, half2 textelSize) {
    half2 pixelSize = max(2, (1 - effectFactor * 0.95) * textelSize);
    return round(texCoord * pixelSize) / pixelSize;
}

inline fixed4 ApplyToneEffectGrayScale(fixed4 color, fixed factor) {
    fixed3 lum = Luminance(color.rgb);
    fixed3 greyscale = lerp(color.rgb, lum, factor);
    color.rgb = greyscale;
    return color;
}

inline fixed4 ApplyToneEffectSepia(fixed4 color, fixed factor) {
    fixed3 lum = Luminance(color.rgb);
    fixed3 sepia = lerp(color.rgb, lum * half3(1.07, 0.74, 0.43), factor);
    color.rgb = sepia;
    return color;
}

inline fixed4 ApplyToneEffectNegate(fixed4 color, fixed factor) {
    fixed3 negate = lerp(color.rgb, 1 - color.rgb, factor);
    color.rgb = negate;
    return color;
}

fixed4 ApplyToneEffect(int effectType, fixed4 color, fixed factor) {
    
    fixed3 lum = Luminance(color.rgb);
    
    fixed3 greyscale = lerp(color.rgb, lum, factor);
    fixed3 sepia = lerp(color.rgb, lum * half3(1.07, 0.74, 0.43), factor);
    fixed3 negate = lerp(color.rgb, 1 - color.rgb, factor);
    
    fixed3 retnRgb = lerp(greyscale, sepia, effectType == TONE_TYPE_SEPIA);
    retnRgb = lerp(retnRgb, negate, effectType == TONE_TYPE_NEGATE);
    
    color.rgb = retnRgb;

	return color;
}

fixed4 ApplyFillColorEffect(fixed4 color, fixed4 factor, int cutoff = 0) {
    color.rgb = lerp(color.rgb, factor.rgb, factor.a);
    color.a = lerp(color.a, factor.a, cutoff);
    return color;
}

fixed4 ApplyColorEffect(int effectType, half4 color, half4 factor, fixed cutoff = 0) {
    
    fixed3 fill = color.rgb = lerp(color.rgb, factor.rgb, factor.a);
    fixed3 add =  color.rgb + factor.rgb * factor.a;
    fixed3 sub = color.rgb - factor.rgb * factor.a;
    fixed3 none = lerp(color.rgb, color.rgb * factor.rgb, factor.a);
    
    fixed3 retn = lerp(none, fill, effectType == 1);
    retn = lerp(retn, add, effectType == 2);
    retn = lerp(retn, sub, effectType == 3);

    color.rgb = retn;
    color.a = lerp(color.a, factor.a, cutoff);
	return color;
}

#endif // VERTIGO_EFFECT_COLOR
