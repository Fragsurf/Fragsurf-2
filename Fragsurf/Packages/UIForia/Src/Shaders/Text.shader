Shader "UIForia/Text"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _FaceColor ("Color", Color) = (1, 1, 1, 1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        Lighting Off
        Fog { Mode Off }
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma target 3.0
		    #pragma vertex vert
		    #pragma fragment frag
            #pragma multi_compile __ UIFORIA_USE_UNDERLAY UIFORIA_USE_UNDERLAY_INNER UIFORIA_USE_OUTLINE UIFORIA_USE_GLOW
            #pragma multi_compile __ UIFORIA_USE_UNDERLAY UIFORIA_USE_UNDERLAY_INNER UIFORIA_USE_OUTLINE UIFORIA_USE_GLOW
            #include "UnityCG.cginc"

            struct vertex_t {
                float4	vertex		: POSITION;
                float2	texcoord	: TEXCOORD0;
                float2	clipCoord	: TEXCOORD1;
		    };

            struct pixel_t {
                float4	vertex		   : SV_POSITION;
                float2	atlas		   : TEXCOORD0;
                float4	params		   : TEXCOORD1;  // alphaClip, scale, bias, weight
                float4  underlayParams : TEXCOORD2; // u, v, scale, bias         
                float2	clipCoord	   : TEXCOORD3;

                fixed4  underlayColor  : COLOR1;
             };

            // defined by render system
            uniform float4 _ClipRect;
            
            // defined by font       
            uniform sampler2D _MainTex;
            uniform float _WeightNormal;
            uniform float _WeightBold;
            uniform float _GradientScale;
            uniform float _ScaleRatioA;
            uniform float _ScaleRatioB;
            uniform float _ScaleRatioC;
            uniform float2 _TextureSize;
            uniform float _Rotation;
            uniform float2 _Pivot;
            
            // defined by user
            uniform int _Bold;
            uniform float _FontScale;
            uniform fixed4 _FaceColor;
            uniform fixed4 _OutlineColor;
            uniform float2 _OutlineSettings;
            #define _OutlineWidth _OutlineSettings.x
            #define _OutlineSoftness _OutlineSettings.y 
            
            // defined by user
            uniform float4 _UnderlaySettings; // offsetxy, dilate, softness;
            #define _UnderlayOffset _UnderlaySettings.xy
            #define _UnderlayDilate _UnderlaySettings.z
            #define _UnderlaySoftness _UnderlaySettings.w
            
            uniform fixed4 _GlowColor;
            uniform float4 _GlowSettings;
            uniform float2 _GlowOffset;
            
            #define _GlowInner _GlowSettings.x
            #define _GlowOuter _GlowSettings.y
            #define _GlowPower _GlowSettings.z
            
            pixel_t vert (vertex_t v) {
                pixel_t o;
        		
        		float pixelSize = 1; // maybe change this to reflect view scale
        		pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
        		float scale = rsqrt(dot(1, 1));
        		
        		
			    scale *= _FontScale * _GradientScale * 1.5;
                float weight = lerp(_WeightNormal, _WeightBold, _Bold) / 4.0;
			    weight = (weight) * _ScaleRatioA * 0.5;
       			float bias = (0.5 - weight) + (0.5 / scale);
			    float alphaClip = (1.0 - _OutlineWidth * _ScaleRatioA - _OutlineSoftness * _ScaleRatioA);
                
                float4 params = float4(alphaClip, scale, bias, weight);
            
            #ifdef UIFORIA_USE_GLOW
            
                alphaClip = min(alphaClip, 1.0 - _GlowOffset * _ScaleRatioB - _GlowOuter * _ScaleRatioB);
  
            #endif
            
			    alphaClip = alphaClip / 2.0 - ( .5 / scale) - weight;
            
            #if defined(UIFORIA_USE_UNDERLAY) | defined(UIFORIA_USE_UNDERLAY_INNER)
                // todo this can all be a uniform input
            	float4 underlayColor = _UnderlayColor;
			    underlayColor.rgb *= underlayColor.a;

			    float underlayScale = scale;
			    underlayScale /= 1 + ((_UnderlaySoftness * _ScaleRatioC) * underlayScale);
			    float underlayBias = (0.5 - weight) * underlayScale - 0.5 - ((_UnderlayDilate * _ScaleRatioC) * 0.5 * underlayScale);
    
			    float x = -(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureSize.x;
			    float y = -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureSize.y;
			    float2 bOffset = float2(x, y);
			    o.underlayParams = float4(v.texcoord + float2(x,y), underlayScale, underlayBias);
			    o.underlayColor = underlayColor;
			    
			#endif 
			
			    if(_Rotation != 0) {
                
                    float4 rotated = v.vertex;
                    float s = sin(_Rotation);
                    float c = cos(_Rotation);
                    
                    rotated.x -= _Pivot.x;
                    rotated.y -= _Pivot.y;
                    
                    float newX = (c * rotated.x) - (s * rotated.y);
                    float newY = (s * rotated.x) + (c * rotated.y);
                    
                    rotated.x = newX + _Pivot.x;
                    rotated.y = newY + _Pivot.y;
                    o.vertex = UnityObjectToClipPos(rotated);
                    
                }
                else {
                    o.vertex = UnityObjectToClipPos(v.vertex);
                }
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.atlas = v.texcoord;
                o.params = params;
                o.clipCoord = v.clipCoord;
                return o;
                
            }
            
            fixed4 frag (pixel_t IN) : SV_Target {

                float2 p = float2(IN.clipCoord.x, 1 - IN.clipCoord.y);
                float4 clipRect = _ClipRect;
    
                clip(step(clipRect.xy, p) * step(p, clipRect.zw));
                
                float c = tex2D(_MainTex, IN.atlas).a;
                float scale	= IN.params.y;
                float bias	= IN.params.z;
                float weight = IN.params.w;
                float sd = (bias - c) * scale;
                
			    float softness = (_OutlineSoftness * _ScaleRatioA) * scale;
                
                fixed4 faceColor = _FaceColor;
                
            #ifdef UIFORIA_USE_OUTLINE
            
                 fixed4 outlineColor = _OutlineColor;
                 float outline = _OutlineWidth * _ScaleRatioA) * scale;
                 fixed faceAlpha = 1 - saturate((sd - outline * 0.5 + softness * 0.5) / (1.0 + softness));
                 fixed outlineAlpha = saturate((sd + outline * 0.5)) * sqrt(min(1.0, outline));
                 faceColor.rgb *= faceColor.a;
                 outlineColor.rgb *= outlineColor.a;
                 faceColor = lerp(faceColor, outlineColor, outlineAlpha) * faceAlpha;
                 
            #else
            
                fixed faceAlpha = 1 - saturate((sd + (softness * 0.5)) / (1.0 + softness));
                faceColor.rgb *= faceColor.a;
                faceColor *= faceAlpha;
                
            #endif
            
            #ifdef UIFORIA_USE_UNDERLAY
            	
            	float d = tex2D(_MainTex, IN.underlayParams.xy).a * IN.underlayParams.z;
			    faceColor += IN.underlayColor * saturate(d - IN.underlayParams.w) * (1 - faceColor.a);

            #endif
            
            #ifdef UIFORIA_USE_UNDERLAY_INNER
            
                float d = tex2D(_MainTex, IN.underlayParams.xy).a * IN.underlayParams.z;
			    faceColor += IN.underlayColor * (1 - saturate(d - IN.underlayParams.w)) * saturate(1 - sd) * (1 - faceColor.a);
            
            #endif
            
            #ifdef UIFORIA_USE_GLOW
            
                float glow = sd - (_GlowOffset * _ScaleRatioB) * 0.5 * scale;
	            float t = lerp(_GlowInner, (_GlowOuter * _ScaleRatioB), step(0.0, glow)) * 0.5 * scale;
	            glow = saturate(abs(glow/(1.0 + t)));
	            glow = 1.0 - pow(glow, _GlowPower);
	            glow *= sqrt(min(1.0, t)); // Fade off glow thinner than 1 screen pixel
	            fixed4 glowColor = (_GlowColor.rgb, saturate(_GlowColor.a * glow * 2));
			    faceColor.rgb += glowColor.rgb * glowColor.a;
			    
            #endif
                clip(faceColor.a - 0.001);
                return faceColor;
            }
            
            ENDCG
        }
    }
}