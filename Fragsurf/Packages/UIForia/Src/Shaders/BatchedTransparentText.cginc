      
#define gWeightNormal _globalFontData1.x
#define gWeightBold _globalFontData1.y
#define gFontTextureWidth _globalFontData1.z
#define gFontTextureHeight _globalFontData1.w
#define gGradientScale _globalFontData2.x
#define gScaleRatioA _globalFontData2.y
#define gScaleRatioB _globalFontData2.z
#define gScaleRatioC _globalFontData2.w
#define _ScaleX 1
#define _ScaleY 1
#define _FaceDilate 0
#define _GlowOuter 0
#define _GlowOffset 0
#define _UnderlayColor 0
#define _UnderlaySoftness 0
#define _UnderlayDilate 0
#define _UnderlayOffsetX 0
#define _UnderlayOffsetY 0
           
fixed4 GetColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
    half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
    half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));
    // enable for pre mul alpha
    //faceColor.rgb *= faceColor.a;
    //outlineColor.rgb *= outlineColor.a;

    faceColor = lerp(faceColor, outlineColor, outlineAlpha);

    // enable for pre mul alpha
    faceColor *= faceAlpha;

    return faceColor;
}

          
v2f TextVertex(appdata input) {
           
    float outlineWidth = input.uv2.x;
    float outlineSoftness = input.uv2.y;

    float4 vPosition = UnityObjectToClipPos(input.vertex);
    float2 pixelSize = vPosition.w;

    pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
    float scale = rsqrt(dot(pixelSize, pixelSize));
    scale *= abs(/* character scale */input.uv.z) * gGradientScale * 1.5; 

    int bold = 0;

    float weight = lerp(gWeightNormal, gWeightBold, 0) / 4.0;
    weight = (weight + _FaceDilate) * gScaleRatioA * 0.5;

    float bias =(0.5 - weight) + (0.5 / scale);
    float alphaClip = (1.0 - outlineWidth * gScaleRatioA - outlineSoftness * gScaleRatioA);
    
    alphaClip = alphaClip / 2.0 - ( 0.5 / scale) - weight;
    
    v2f o;
    o.vertex = UnityObjectToClipPos(input.vertex);
    o.color = input.color;
    o.uv = input.uv;
    o.flags = float4(RenderType_Text, 0, 0, 0);
    o.fragData1 = float4(input.vertex.xy, input.uv2.x, 0);
    o.fragData2 =  float4(alphaClip, scale, bias, weight);
    o.fragData3 = input.uv2;
    
    return o;
}


fixed4 TextFragment(v2f input) {
   float c = tex2D(_globalFontTexture, input.uv.xy).a;
   float outlineWidth = 0;//input.fragData3.x;
   float outlineSoftness = 0;//input.fragData3.y;

   float scale	= input.fragData2.y;
   float bias	= input.fragData2.z; 
   float weight	= input.fragData2.w;
   float sd = (bias - c) * scale;
   float outline = (outlineWidth * gScaleRatioA) * scale;
   float softness = (outlineSoftness * gScaleRatioA) * scale;
   half4 faceColor = input.color;
   half4 outlineColor = half4(0, 0, 0, 0); //_OutlineColor;
   
   // enable this line for premul alpha
   faceColor.rgb *= input.color.rgb;
   
   faceColor = GetColor(sd, faceColor, outlineColor, outline, softness);
   return faceColor * input.color.a;

}

#undef gWeightNormal
#undef gWeightBold
#undef gFontTextureWidth
#undef gFontTextureHeight
#undef gGradientScale
#undef gScaleRatioA
#undef gScaleRatioB
#undef gScaleRatioC
#undef _ScaleX 
#undef _ScaleY 
#undef _FaceDilate 
#undef _OutlineWidth 
#undef _OutlineSoftness 
#undef _GlowOuter 
#undef _GlowOffset 
#undef _UnderlayColor
#undef _UnderlaySoftness 
#undef _UnderlayDilate
#undef _UnderlayOffsetX 
#undef _UnderlayOffsetY 