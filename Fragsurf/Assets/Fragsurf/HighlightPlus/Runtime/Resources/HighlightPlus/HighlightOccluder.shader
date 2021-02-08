Shader "HighlightPlus/Geometry/SeeThroughOccluder" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
}
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }

        // Create mask
        Pass
        {
			Stencil {
                Ref 2
                Comp always
                Pass DecrWrap
}
            ColorMask 0
            ZWrite Off
            Offset -1, -1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
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