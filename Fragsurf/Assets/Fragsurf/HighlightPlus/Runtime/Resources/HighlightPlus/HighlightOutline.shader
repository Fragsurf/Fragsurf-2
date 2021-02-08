Shader "HighlightPlus/Geometry/Outline" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _OutlineColor ("Outline Color", Color) = (0,0,0,1)
    _OutlineWidth ("Outline Offset", Float) = 0.01
    _OutlineDirection("Outline Direction", Vector) = (0,0,0)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _Cull ("Cull Mode", Int) = 2
    _ConstantWidth ("Constant Width", Float) = 1
	_OutlineZTest("ZTest", Int) = 4
    _CutOff("CutOff", Float ) = 0.5
}
    SubShader
    {
        Tags { "Queue"="Transparent+120" "RenderType"="Transparent" "DisableBatching"="True" }

        // Outline
        Pass
        {
            Stencil {
                Ref 2
                Comp NotEqual
                Pass replace 
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]
            ZTest [_OutlineZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ HP_ALPHACLIP
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
		        UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _ConstantWidth;
	        fixed _CutOff;
            sampler _MainTex;
      		float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _OutlineDirection)
            UNITY_INSTANCING_BUFFER_END(Props)
            	    
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = any(norm.xy)!=0 ? TransformViewToProjection(normalize(norm.xy)) : 0.0.xx;
				float z = lerp(UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z), 2.0, UNITY_MATRIX_P[3][3]);
                z = _ConstantWidth * (z - 2.0) + 2.0;
                float4 outlineDirection =  UNITY_ACCESS_INSTANCED_PROP(Props, _OutlineDirection); 
				o.pos.xy += offset * z * _OutlineWidth + outlineDirection.xy * z;
				o.uv = TRANSFORM_TEX (v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	#if HP_ALPHACLIP
            	    fixed4 col = tex2D(_MainTex, i.uv);
            	    clip(col.a - _CutOff);
            	#endif
  		      	return _OutlineColor;
            }
            ENDCG
        }

        // Outline Clear Stencil
        Pass
        {
            Stencil {
                Ref 2
                Comp Always
                Pass zero
            }

            ColorMask 0
            ZWrite Off
            Cull [_Cull]
            ZTest [_OutlineZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _OutlineColor;
            float _OutlineWidth;
            float2 _OutlineDirection;
            float _ConstantWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
				float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = any(norm.xy)!=0 ? TransformViewToProjection(normalize(norm.xy)) : 0.0.xx;
				float z = lerp(UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z), 2.0, UNITY_MATRIX_P[3][3]);
                z = _ConstantWidth * (z - 2.0) + 2.0;
				o.pos.xy += offset * z * _OutlineWidth + _OutlineDirection * z;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDCG
        }


    }
}