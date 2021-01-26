#ifndef INCLUDE_VERTIGO_EFFECT_SHINE
#define INCLUDE_VERTIGO_EFFECT_SHINE

struct ShineEffectData {
    fixed nomalizedPosition;
    fixed location;
    fixed width;
    fixed softness;
    fixed brightness;
    fixed gloss;
    fixed4 color;
}

half4 ApplyShinyEffect(in ShineEffectData  shineData) {
	half normalized = 1 - saturate(abs((shineData.nomalizedPos - shineData.location) / shineData.width));
	half shinePower = smoothstep(0, shineData.softness * 2, normalized);
	half3 reflectColor = lerp(1, shineData.color.rgb * 10, shineData.gloss);

	color.rgb += shineData.color.a * (shinePower / 2) * shineData.brightness * reflectColor;

	return color;
}


#endif // INCLUDE_VERTIGO_EFFECT_SHINE