#ifndef INCLUDE_VERTIGO_EFFECT_HSV
#define INCLUDE_VERTIGO_EFFECT_HSV

struct HSVEffecData {
    fixed targetRange;
    fixed3 targetHsv;
    fixed3 shift;
    fixed4 color;
}

half3 RgbToHsv(half3 c) {
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	half d = q.x - min(q.w, q.y);
	half e = 1.0e-10;
	return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 HsvToRgb(half3 c) {
	c = half3(c.x, clamp(c.yz, 0.0, 1.0));
	half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

half4 ApplyHsvEffect(in HSVEffecData effectData) {
    fixed3 targetHsv = param1.rgb;

    fixed3 hsvShift = effectData.color - 0.5;
	half3 hsv = RgbToHsv(effectData.color);
	half3 range = abs(hsv - effectData.targetHsv);
	half diff = max(max(min(1-range.x, range.x), min(1-range.y, range.y)/10), min(1-range.z, range.z)/10);

	fixed masked = step(diff, effectData.targetRange);
	
	fixed4 retn = effectData.color;
	
	retn.rgb = HsvToRgb(hsv + hsvShift * masked);

	return retn;
}

#endif // INCLUDE_VERTIGO_EFFECT_HSV