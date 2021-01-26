Shader "Vertigo/VertigoSDF"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTexture ("Mask", 2D) = "white" {}
        _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle] _InvertMask ("Invert Mask",  Int) = 0
    }
    SubShader {
        Tags {
         "RenderType"="Transparent"
         "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend One OneMinusSrcAlpha
         
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
            
            #pragma multi_compile __ SDF_RECTLIKE SDF_TRIANGLE SDF_TEXT SDF_RHOMBUS
            
            #include "UnityCG.cginc"
            #include "./VertigoSDFUtil.cginc"
            
            sampler2D _MainTex;
            sampler2D _MaskTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _MaskSoftness;
            float _InvertMask;
             
            // todo -- set as vector4s instead of floats
            sampler2D _FontTexture;
            float _FontScaleRatioA;
            float _FontScaleRatioB;
            float _FontScaleRatioC;
            float _FontTextureWidth;
            float _FontTextureHeight;
            float _FontGradientScale;
            float _FontScaleX;
            float _FontScaleY;
            float _FontWeightNormal;
            float _FontWeightBold;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = UnityPixelSnap(o.vertex);
                o.color = v.color;
                o.texCoord0 = v.texCoord0;
                o.texCoord1 = v.texCoord1;
                o.sdfCoord = UnpackToHalf2(v.texCoord1.w);
                
//                if(v.texCoord2.x == 1) {
//                    float outlineWidth = input.uv2.x;
//                    float outlineSoftness = 0;//input.uv2.y;
//                    
//                    float2 pixelSize = vPosition.w;
//                    
//                    pixelSize /= float2(_FontScaleX, _FontScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
//                    float scale = rsqrt(dot(pixelSize, pixelSize));
//                    scale *= abs(input.uv.z) * _FontGradientScale * 1.5; 
//                    
//                    int bold = 0;
//                    
//                    float weight = lerp(_FontWeightNormal, _FontWeightBold, 0) / 4.0;
//                    weight = (weight + _FaceDilate) * _FontScaleRatioA * 0.5;
//                    
//                    float bias =(.5 - weight) + (.5 / scale);
//                    float alphaClip = (1.0 - outlineWidth * _FontScaleRatioA - outlineSoftness * _FontScaleRatioA);
//                    
//                    alphaClip = min(alphaClip, 1.0 - _GlowOffset * _FontScaleRatioB - _GlowOuter * _FontScaleRatioB);
//                    alphaClip = alphaClip / 2.0 - ( .5 / scale) - weight;
//                    
//                    float4 underlayColor = _UnderlayColor;
//                    underlayColor.rgb *= underlayColor.a;
//                    9
//                    float bScale = scale;
//                    bScale /= 1 + ((_UnderlaySoftness * _FontScaleRatioC) * bScale);
//                    float bBias = (0.5 - weight) * bScale - 0.5 - ((_UnderlayDilate *  _FontScaleRatioC) * 0.5 * bScale);
//                    
//                    float x = -(_UnderlayOffsetX *  _FontScaleRatioC) * _FontGradientScale / _FontTextureWidth;
//                    float y = -(_UnderlayOffsetY *  _FontScaleRatioC) * _FontGradientScale / _FontTextureHeight;
//                    float2 bOffset = float2(x, y);
//                     
//                    float4 outlineColor = input.uv3;
//                }
//                
                return o;
            }
            
            float4 UnpackColor(uint input) {
                return fixed4(
                    uint((input >> 0) & 0xff) / float(0xff),
                    uint((input >> 8) & 0xff) / float(0xff),
                    uint((input >> 16) & 0xff) / float(0xff),
                    uint((input >> 24) & 0xff) / float(0xff)
                );
            }

            #define PaintMode_Color 1 << 0
            #define PaintMode_Texture 1 << 1
            #define PaintMode_TextureTint 1 << 2
            
            inline fixed4 ComputeColor(float4 packedColor, float2 texCoord) {
                uint colorMode = packedColor.b;

                int useColor = (colorMode & PaintMode_Color) != 0;
                int useTexture = (colorMode & PaintMode_Texture) != 0;
                int tintTexture = (colorMode & PaintMode_TextureTint) != 0;
                
                fixed4 bgColor = UnpackColor(asuint(packedColor.r));
                fixed4 tintColor = UnpackColor(asuint(packedColor.g));
                fixed4 textureColor = tex2D(_MainTex, texCoord);

                bgColor.rgb *= bgColor.a;
                tintColor.rgb *= tintColor.a;
                textureColor.rgb *= textureColor.a;
                
                textureColor = lerp(textureColor, textureColor * tintColor, tintTexture);
                
                if(useTexture && useColor) {
                    return lerp(textureColor, bgColor, 1 - textureColor.a);
                }
                
                return lerp(bgColor, textureColor, useTexture);
            }
             
            fixed4 frag (v2f i) : SV_Target {
                           
                SDFData sdfData = UnpackSDFData(i.texCoord1, i.sdfCoord);
                sdfData.strokeWidth = 30;
                sdfData.shapeType = ShapeType_RectLike;
                // todo -- to combat cut off of edges, constraint texcoords to 0.1 - 0.99 or something similiar
                fixed4 alpha = SDFColor(sdfData, fixed4(1, 0, 0, 1));
                alpha.rgb *= alpha.a;
                return alpha;
                
                fixed4 mainColor = ComputeColor(i.color, i.texCoord0.xy);
                mainColor.a = alpha;
                mainColor.rgb *=  mainColor.a;
                return mainColor;
                
                // clip(textureColor.a - 0.01);
                // fixed maskAlpha = saturate(tex2D(_MaskTexture, i.texCoord0.xy).a / _MaskSoftness);
                // maskAlpha = lerp(1 - maskAlpha, maskAlpha, _InvertMask);
                // mainColor.a *= maskAlpha;
                // return textureColor;// * fixed4(bgColor.rgb, 1) * 2;

            }

            ENDCG
        }
    }
}
