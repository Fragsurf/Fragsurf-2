Shader "UIForia/Standard"
{
    Properties {
        //  _MainTex ("Texture", 2D) = "white" {}
        // _MaskTexture ("Mask", 2D) = "white" {}
        //// _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        // _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle(UIFORIA_TEXTURE_CLIP)] _TextureClip ("Texture Clip",  Int) = 1
    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend One OneMinusSrcAlpha
        ZTest Off
        ZClip Off
        // this stencil setting solves self-blending
        // does mean we have to issue the draw call twice probably
        // if we want to reset the stencil
        //        Stencil {
            //            Ref 0
            //            Comp Equal
            //            Pass IncrSat 
            //            Fail IncrSat
        //        }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature UIFORIA_TEXTURE_CLIP

            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE

            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
            #include "./UIForiaSDFUtil.cginc"
            
            sampler2D _MainTexture;
            sampler2D _MaskTexture;
            sampler2D _FontTexture;
            
            float4 _Color;
            
            float _FontScaleX;
            float _FontScaleY;
            float4 _FontScales;
            float4 _FontTextureSize;
            
            float4 _ColorData[BATCH_SIZE];
            float4 _MiscData[BATCH_SIZE];
            float4 _ObjectData[BATCH_SIZE];
            float4 _CornerData[BATCH_SIZE];
            float4 _ClipUVs[BATCH_SIZE];
            float4 _ClipRects[BATCH_SIZE];
            float4x4 _TransformData[BATCH_SIZE];
            float _DPIScale;
            
            #define _FontGradientScale _FontScales.x
            #define _FontScaleRatioA _FontScales.y
            #define _FontScaleRatioB _FontScales.z
            #define _FontScaleRatioC _FontScales.w
            
            #define _FontTextureWidth _FontTextureSize.x
            #define _FontTextureHeight _FontTextureSize.y
            
            #define Vert_BorderRadii objectInfo.z
            #define Vert_PackedSize objectInfo.y
            #define Vert_BorderColors v.texCoord1.xy
            
            #define Vert_CharacterScale v.texCoord1.x
            #define Vert_CharacterPackedOutline objectInfo.y
            // todo -- re-implement underlay
            #define Vert_CharacterPackedUnderlay 0
            #define Vert_CharacterWeight objectInfo.z
            
            #define Frag_SDFSize i.texCoord1.xy
            #define Frag_SDFBorderRadii i.texCoord1.z
            #define Frag_SDFStrokeWidth i.texCoord1.w
            #define Frag_SDFCoords i.texCoord0.zw
            #define Frag_ShapeType i.texCoord2.x
            #define Frag_BorderColors i.texCoord3
            #define Frag_BorderSize i.color.zw
            #define Frag_ColorMode i.texCoord2.y        
            
            v2f vert (appdata v) {
                v2f o;
                int objectIndex = (int)v.texCoord1.w; // can be a byte, consider packing this if needed

                float4 objectInfo = _ObjectData[objectIndex];
                float4x4 transform = _TransformData[objectIndex];
                
                uint shapeType = ((uint) objectInfo.x >> 16) & (1 << 16) - 1; // maps to ShapeType defines 
                uint colorMode = ((uint) objectInfo.x) & 0xffff;
                
                half2 size = UnpackSize(Vert_PackedSize);
                v.vertex = mul(transform, float4(v.vertex.xyz, 1));
                
                o.vertex = float4(UnityObjectToViewPos(v.vertex) / float3(0.5 * _ScreenParams.x, _ProjectionParams.x * 0.5 * _ScreenParams.y, 1.0), 1.0);
                float4 screenPos = ComputeScreenPos(o.vertex);
                o.texCoord0 = v.texCoord0;
                o.color = _ColorData[objectIndex];
                
                // this only works for 'flower' configuration meshes, not for quads. use a flag for the quad
                o.texCoord4 = float4(lerp(0, 1, v.texCoord0.x == 0.5 && v.texCoord0.y == 0.5), screenPos.xyw);
                
                if(shapeType != ShapeType_Text) {
                    o.vertex = UIForiaPixelSnap(o.vertex); // pixel snap is bad for text rendering
                    o.texCoord1 = float4(size.x, size.y, Vert_BorderRadii, objectIndex);
                    o.texCoord2 = float4(shapeType, colorMode, 0, 0);
                    o.texCoord3 = _MiscData[objectIndex];
                }
                else {             
                    _FontScaleX = 1;
                    _FontScaleY = 1;
                    
                    float weight = Vert_CharacterWeight; 

                    fixed4 unpackedOutline = UnpackColor(asuint(Vert_CharacterPackedOutline));
                    float outlineWidth = 0; // unpackedOutline.x;
                    float outlineSoftness = 0; // unpackedOutline.y;
                    
                    // todo -- glow
                    
                    fixed4 unpackedUnderlay = UnpackColor(asuint(Vert_CharacterPackedUnderlay));
                    
                    float4 underlayData = _MiscData[objectIndex];
                    fixed underlayX = underlayData.x;
                    fixed underlayY = underlayData.y;
                    fixed underlayDilate = underlayData.z;
                    fixed underlaySoftness = underlayData.w;
                    
                    // scale stuff can be moved to cpu, alpha clip & bias too
                    float2 pixelSize = o.vertex.w;
                    pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                    
                    float scale = rsqrt(dot(pixelSize, pixelSize));
                    scale *= abs(Vert_CharacterScale) * _FontGradientScale * 1.5;

                    float underlayScale = scale;
                    underlayScale /= 1 + ((underlaySoftness * _FontScaleRatioC) * underlayScale);
                    float underlayBias = (0.5 - weight) * underlayScale - 0.5 - ((underlayDilate * _FontScaleRatioC) * 0.5 * underlayScale);
                    
                    float2 underlayOffset = float2(
                        -(underlayX * _FontScaleRatioC) * _FontGradientScale / _FontTextureWidth,
                        -(underlayY * _FontScaleRatioC) * _FontGradientScale / _FontTextureHeight
                    );
                    
                    float bias = (0.5 - weight) + (0.5 / scale);
                    float alphaClip = (1.0 - outlineWidth * _FontScaleRatioA - outlineSoftness * _FontScaleRatioA);
                    
                    alphaClip = alphaClip / 2.0 - ( 0.5 / scale) - weight;
                   
                    o.texCoord1 = float4(alphaClip, scale, bias, objectIndex);
                    o.texCoord2 = float4(ShapeType_Text, outlineWidth, outlineSoftness, weight);
                    o.texCoord3 = float4(underlayOffset, underlayScale, underlayBias);
                }
                
                // todo -- more unpacking can be done in the vertex shader
                
                return o;
            }            
          
            float GetCornerBevel(float2 uv, float4 bevels) {
                
                float left = step(uv.x, 0.5); // 1 if left
                float bottom = step(uv.y, 0.5); // 1 if bottom
                
                #define top (1 - bottom)
                #define right (1 - left)  
                float r = 0;
                r += ((1 - bottom) * left) * bevels.x;
                r += ((1 - bottom) * (1 - left)) * bevels.y;
                r += (bottom * left) * bevels.z;
                r += (bottom * (1 - left)) * bevels.w;
                
                return r;
                  
            }
            
            fixed4 frag (v2f i) : SV_Target {           
                
                float2 clipPos = float2(i.vertex.x, _ProjectionParams.x > 0 ? i.vertex.y : _ScreenParams.y - i.vertex.y) * _DPIScale;
                float4 clipRect = _ClipRects[(uint)i.texCoord1.w];
                float4 clipUvs = _ClipUVs[(uint)i.texCoord1.w];           
                float opacity = _ObjectData[(uint)i.texCoord1.w].w;              
                float4 cornerBevels = _CornerData[(uint)i.texCoord1.w];
                //return fixed4(i.texCoord0.y, i.texCoord0.y, i.texCoord0.y, 1);
               // return fixed4(i.texCoord0.x, i.texCoord0.x, i.texCoord0.x, 1);
                // todo -- returns cause branching here
                // get rid of text and we can get rid of branching
                
                fixed4 mainColor = ComputeColor(i.color.r, i.color.g, Frag_ColorMode, i.texCoord0.xy, _MainTexture);
                
                if(Frag_ShapeType != ShapeType_Text) {
                    float bevel = GetCornerBevel(i.texCoord0.zw, cornerBevels);
                    BorderData borderData = GetBorderData(Frag_SDFCoords, Frag_SDFSize, Frag_BorderColors, Frag_BorderSize, Frag_SDFBorderRadii, mainColor);
                    SDFData sdfData;
                    sdfData.uv = Frag_SDFCoords;
                    sdfData.size = Frag_SDFSize;
                    sdfData.strokeWidth = borderData.size;
                    sdfData.radius = borderData.radius;
                    mainColor = SDFColor(sdfData, borderData.color, mainColor, bevel);
                    mainColor.a *= opacity;
                    
                    // todo -- this causes bad branching
                    if(Frag_ColorMode == PaintMode_Shadow || Frag_ColorMode == PaintMode_ShadowTint) {
                        float intensity = i.color.b;
                        sdfData.strokeWidth = 3;
                        float n = smoothstep(-intensity, 2, SDFShadow(sdfData, intensity, bevel));
                        fixed4 shadowColor = fixed4(UnpackColor(asuint(i.color.r)).rgb, 1);
                        fixed4 shadowTint = fixed4(UnpackColor(asuint(i.color.g)).rgb, 1);
                        fixed4 shadowRetn = lerp(fixed4(shadowColor.rgb, 1 - n), shadowColor, (1 - n));                
                        fixed4 tintedShadowColor = lerp(fixed4(shadowTint.rgb, 1 - n), shadowColor,  (1 - n));
                        tintedShadowColor = lerp(shadowRetn, tintedShadowColor, n);
                        
                        shadowRetn = lerp(shadowRetn, tintedShadowColor, Frag_ColorMode == PaintMode_ShadowTint);
                        shadowRetn.a *= i.color.a;
                        mainColor = shadowRetn;
                        mainColor.rgb *= mainColor.a;
                        return mainColor;
                    }
                    
                    mainColor = UIForiaAlphaClipColor(mainColor, _MaskTexture, clipPos, clipRect, clipUvs);
                    mainColor.rgb *= mainColor.a;
                    return mainColor;
                }

                float outlineWidth = 0; //i.texCoord2.y;
                float outlineSoftness = 0; //i.texCoord2.z;
                float c = tex2D(_FontTexture, float2(i.texCoord0.z, 1 - i.texCoord0.w)).a;

                float scaleRatio = _FontScaleRatioA;
                
                float scale	= i.texCoord1.y;
                float bias = i.texCoord1.z;
                float weight = i.texCoord2.w;
                float sd = (bias - c) * scale;

                float outline = 0; // (outlineWidth * scaleRatio) * scale;
                float softness = 0; //(outlineSoftness * scaleRatio) * scale;

                fixed4 faceColor = UIForiaColorSpace(UnpackColor(asuint(i.color.r))); // could just be mainColor?
                
                fixed4 outlineColor = Green;//UnpackColor(asuint(i.color.g));
                fixed4 underlayColor = UnpackColor(asuint(i.color.b));
                //fixed4 glowColor = UnpackColor(asuint(i.color.a));
                faceColor.a *= opacity;
                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);

                #define underlayOffset i.texCoord3.xy
                #define underlayScale i.texCoord3.z
                #define underlayBias i.texCoord3.w
                
                int hasUnderlay = 0;//underlayColor.a > 0;
                // todo -- pull underlay into a seperate shader
                float d = tex2D(_FontTexture, i.texCoord0.zw + i.texCoord3.xy).a * underlayScale;
                underlayColor = faceColor + fixed4(underlayColor.rgb * underlayColor.a, underlayColor.a)  * (saturate(d - underlayBias)) * (1 - faceColor.a);
                faceColor = lerp(faceColor, underlayColor, hasUnderlay);
                faceColor = UIForiaAlphaClipColor(faceColor, _MaskTexture, clipPos, clipRect, clipUvs);
                if(c < 0.005) return fixed4(0, 0, 0, 0);
                return faceColor;               

            }

            ENDCG
        }
    }
}
