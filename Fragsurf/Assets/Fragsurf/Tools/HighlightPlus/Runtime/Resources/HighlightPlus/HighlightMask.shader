Shader "HighlightPlus/Geometry/Mask" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
	_ZTest("ZTest", Int) = 4
}
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" "DisableBatching"="True" }

        // Create mask
        Pass
        {
			Stencil {
                Ref 2
                Comp always
                Pass replace
            }
            ColorMask 0
            ZWrite Off
            Offset -1, -1
            Cull [_Cull]  // default Cull Back improves glow in high quality)
			ZTest [_ZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ HP_ALPHACLIP
            #include "UnityCG.cginc"

            sampler _MainTex;
      		float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
            fixed _CutOff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos: SV_POSITION;
                float2 uv     : TEXCOORD0;
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
				return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	#if HP_ALPHACLIP
            	    fixed4 col = tex2D(_MainTex, i.uv);
            	    clip(col.a - _CutOff);
            	#endif
            	return 0;
            }
            ENDCG
        }

    }
}