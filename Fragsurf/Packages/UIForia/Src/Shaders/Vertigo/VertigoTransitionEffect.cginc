#ifndef INCLUDE_VERTIGO_EFFECT_TRANSITION
#define INCLUDE_VERTIGO_EFFECT_TRANSITION

#include "../VertigoColorEffect.cginc"

struct TransitionData {
    half4 color;
    half2 position;
    fixed noiseAlpha;
    int reversed;
}

//try to accept colors instead of samplers for all functions

fixed4 ApplyFadeTransition(TransitionData data) {
	float alpha = noiseAlpha;
	fixed factor = lerp(data.effectFactor, 1 - data.effectFactor, data.reversed);
    fixed4 color = data.color;
    color.a *= saturate(alpha + (1 - factor * 2));
    return color;
}

fixed4 ApplyCutOffTransition(TransitionData data) {
	float alpha = noiseAlpha;
	fixed factor = lerp(data.effectFactor, 1 - data.effectFactor, data.reversed);
    fixed4 color = data.color;
	color.a *= step(0.001, color.a * alpha - effectFactor);
    return color;
}

struct DissolveData {
    half4 color;
    half2 position;
    fixed noiseAlpha;
    int reversed;
    fixed width;
    fixed softness;
    fixed3 dissolveColor;
}

fixed4 ApplyDissolveTransition(DissolveData data) {
	float alpha = data.noiseAlpha;
    fixed factor = lerp(data.effectFactor, 1 - data.effectFactor, data.reversed);
    width = width * 0.25;
    float factor = alpha - effectFactor * ( 1 + width ) + width;
    fixed edgeLerp = step(factor, color.a) * saturate((width - factor)*16/ softness);
    color = ApplyColorEffect(color, fixed4(dissolveColor, edgeLerp));
    color.a *= saturate((factor)*32/ softness);
}

/*
fixed4 ApplyTransitionEffect(half4 color, half3 transParam) {
	fixed4 param = tex2D(_ParamTex, float2(0.25, transParam.z));
	float alpha = tex2D(_NoiseTex, transParam.xy).a;
	
	#if REVERSE
        fixed effectFactor = 1 - param.x;
	#else
        fixed effectFactor = param.x;
	#endif
	
	#if FADE
	    color.a *= saturate(alpha + (1 - effectFactor * 2));
	#elif CUTOFF
	    color.a *= step(0.001, color.a * alpha - effectFactor);
	#elif DISSOLVE
        fixed width = param.y/4;
        fixed softness = param.z;
        fixed3 dissolveColor = tex2D(_ParamTex, float2(0.75, transParam.z)).rgb;
        float factor = alpha - effectFactor * ( 1 + width ) + width;
        fixed edgeLerp = step(factor, color.a) * saturate((width - factor)*16/ softness);
        color = ApplyColorEffect(color, fixed4(dissolveColor, edgeLerp));
        color.a *= saturate((factor)*32/ softness);
	#endif

	return color;
}
*/
#endif // INCLUDE_VERTIGO_EFFECT_TRANSITION