Shader "HighlightPlus/Geometry/Overlay" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _OverlayBackColor ("Overlay Back Color", Color) = (1,1,1,1)
    _OverlayData("Overlay Data", Vector) = (1,0.5,1)
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
}
    SubShader
    {
        Tags { "Queue"="Transparent+121" "RenderType"="Transparent" "DisableBatching"="True" }
    
        // Overlay
        Pass
        {
           	Stencil {
                Ref 2
                Comp Equal
                Pass keep 
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Offset -1, -1
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ HP_ALPHACLIP

            #include "UnityCG.cginc"

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
				UNITY_VERTEX_OUTPUT_STEREO
            };

      		fixed4 _Color;
      		sampler2D _MainTex;
      		float4 _MainTex_ST;
      		fixed4 _OverlayBackColor;
      		fixed3 _OverlayData; // x = speed, y = MinIntensity, z = blend;
      		fixed _CutOff;

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
            	fixed4 color = tex2D(_MainTex, i.uv);
            	#if HP_ALPHACLIP
            	clip(color.a - _CutOff);
            	#endif
				fixed t = _OverlayData.y + (1.0 - _OverlayData.y) * 2.0 * abs(0.5 - frac(_Time.y * _OverlayData.x));
                fixed4 col = lerp(_Color, color * _OverlayBackColor * _Color, _OverlayData.z);
                col.a *= t;
				return col;
            }
            ENDCG
        }

    }
}