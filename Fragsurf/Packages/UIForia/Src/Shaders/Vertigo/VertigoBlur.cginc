#ifndef INCLUDE_VERTIGO_BLUR
#define INCLUDE_VERTIGO_BLUR

fixed4 Texture2DAdvancedBlur3x3(sampler2D tex, half2 texCoord, half2 blur, half4 mask) {
    const int KernalSize = 5;
    const float Kernal[5] = { 0.2486, 0.7046, 1.0, 0.7046, 0.2486};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            fixed masked = min(mask.x <= uv.x, uv.x <= mask.z) * min(mask.y <= uv.y, uv.y <= mask.w);
            o += lerp(fixed4(0.5, 0.5, 0.5, 0), tex2D(tex, uv), masked) * weight;
		}
	}
	return o / sum;
}
/*
fixed4 Texture2DAdvancedBlur5x5(sampler2D texture, half2 texCoord, half2 blur, half4 mask) {

    const int KernalSize = 9;
    const float Kernal[9] = { 0.0438, 0.1719, 0.4566, 0.8204, 1.0, 0.8204, 0.4566, 0.1719, 0.0438};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            fixed masked = min(mask.x <= uv.x, uv.x <= mask.z) * min(mask.y <= uv.y, uv.y <= mask.w);
            o += lerp(fixed4(0.5, 0.5, 0.5, 0), tex2D(tex, uv), masked) * weight;
		}
	}
	
	return o / sum;
}

fixed4 Texture2DAdvancedBlur7x7(sampler2D texture, half2 texCoord, half2 blur, half4 mask) {

    const int KernalSize = 13;
    const float Kernal[13] = { 0.0438, 0.1138, 0.2486, 0.4566, 0.7046, 0.9141, 1.0, 0.9141, 0.7046, 0.4566, 0.2486, 0.1138, 0.0438};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            fixed masked = min(mask.x <= uv.x, uv.x <= mask.z) * min(mask.y <= uv.y, uv.y <= mask.w);
            o += lerp(fixed4(0.5, 0.5, 0.5, 0), tex2D(tex, uv), masked) * weight;
		}
	}
	
	return o / sum;
}

fixed4 Texture2DBlur3x3(sampler2D texture, half2 texCoord, half2 blur) {

    const int KernalSize = 3;
    const float Kernal[3] = { 0.4566, 1.0, 0.4566};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            o += tex2D(tex, uv) * weight;
		}
	}
	
	return o / sum;
}
*/
fixed4 Texture2DBlur5x5(sampler2D tex, half2 texCoord, half2 blur) {

    const int KernalSize = 5;
    const float Kernal[5] = { 0.2486, 0.7046, 1.0, 0.7046, 0.2486};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            o += tex2D(tex, uv) * weight;
		}
	}
	
	return o / sum;
}


fixed4 Texture2DBlur7x7(sampler2D tex, half2 texCoord, half2 blur) {

    const int KernalSize = 7;
    const float Kernal[7] = { 0.1719, 0.4566, 0.8204, 1.0, 0.8204, 0.4566, 0.1719};
    
    float4 o = 0;
	float sum = 0;
	float2 shift = 0;
	
	for(int x = 0; x < KernalSize; x++) {
		shift.x = blur.x * (float(x) - KernalSize/2);
		for(int y = 0; y < KernalSize; y++) {
			shift.y = blur.y * (float(y) - KernalSize/2);
			float2 uv = texCoord + shift;
			float weight = Kernal[x] * Kernal[y];
			sum += weight;
            o += tex2D(tex, uv) * weight;
		}
	}
	
	return o / sum;
}

/*

/*fixed4 Tex2DBlurring1D (sampler2D tex, half2 uv, half2 blur) {
    const int KernalSize = 3;
    
    float4 o = 0;
	float sum = 0;
	float weight;
	half2 texcood;
	
	for(int i = -KernalSize/2; i <= KernalSize/2; i++) { 
		texcood = uv;
		texcood.x += blur.x * i;
		texcood.y += blur.y * i;
		weight = 1.0/(abs(i)+2);
		o += tex2D(tex, texcood) * weight;
		sum += weight;
	}
	return o / sum;
}

// Sample texture with blurring.
// * Fast: Sample texture with 3x3 kernel.
// * Medium: Sample texture with 5x5 kernel.
// * Detail: Sample texture with 7x7 kernel.
fixed4 Tex2DBlurring (sampler2D tex, half2 texcood, half2 blur) {
	return Tex2DBlurring(tex, texcood, blur, half4(0,0,1,1));
}

// Sample texture with blurring.
// * Fast: Sample texture with 3x1 kernel.
// * Medium: Sample texture with 5x1 kernel.
// * Detail: Sample texture with 7x1 kernel.
fixed4 Tex2DBlurring1D (sampler2D tex, half2 uv, half2 blur) {
	#if FASTBLUR
	    const int KernalSize = 3;
	#elif MEDIUMBLUR
	    const int KernalSize = 5;
	#elif DETAILBLUR
	    const int KernalSize = 7;
	#else
	    const int KernalSize = 1;
	#endif
	
	float4 o = 0;
	float sum = 0;
	float weight;
	half2 texcood;
	for(int i = -KernalSize/2; i <= KernalSize/2; i++) { 
		texcood = uv;
		texcood.x += blur.x * i;
		texcood.y += blur.y * i;
		weight = 1.0/(abs(i)+2);
		o += tex2D(tex, texcood)*weight;
		sum += weight;
	}
	return o / sum;
}
*/

#endif //INCLUDE_VERTIGO_BLUR