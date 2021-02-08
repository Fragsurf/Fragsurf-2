Shader "HighlightPlus/Geometry/SolidColor" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
	_ZTest("ZTest", Int) = 4
}
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "DisableBatching" = "True" }

        // Compose effect on camera target
        Pass
        {
            ZWrite Off
            Cull [_Cull]
			ZTest Always
            Stencil {
                Ref 2
                Comp NotEqual
                Pass replace 
            }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ HP_ALPHACLIP
            #pragma multi_compile _ HP_DEPTHCLIP

            #include "UnityCG.cginc"

            sampler _MainTex;
            #if HP_DEPTHCLIP
                UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            #endif
      		float4 _MainTex_ST;
            fixed _CutOff;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                #if HP_DEPTHCLIP
                    float depth   : TEXCOORD1;
                    float4 scrPos : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX (v.uv, _MainTex);
                #if HP_DEPTHCLIP
                    o.depth = COMPUTE_DEPTH_01;
                    o.scrPos = ComputeScreenPos(o.pos);
                #endif
				return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                #if HP_DEPTHCLIP
                    float vz = Linear01Depth(UNITY_SAMPLE_DEPTH(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, i.scrPos.xy / i.scrPos.w )));
                    clip( vz - i.depth * 0.999);
                #endif

            	#if HP_ALPHACLIP
            	    fixed4 col = tex2D(_MainTex, i.uv);
            	    clip(col.a - _CutOff);
            	#endif

            	return fixed4(1.0, 1.0, 1.0, 1.0);
            }
            ENDCG
        }

    }
}