Shader "HighlightPlus/Geometry/SeeThroughBorder" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _SeeThroughBorderColor ("Outline Color", Color) = (0,0,0,1)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
}
    SubShader
    {
        Tags { "Queue"="Transparent+201" "RenderType"="Transparent" "DisableBatching"="True" }
   
        // See through effect
        Pass
        {
            Stencil {
                ReadMask 3
                Ref 2
                Comp Greater
                Pass keep
            }

            // In order to preserve rendering order when two objects overlaps we adjust the o.pos.z to near clip and 
            //ZTest Greater // Always
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ HP_ALPHACLIP

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _SeeThroughBorderColor;
            fixed _CutOff;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos    = UnityObjectToClipPos(v.vertex);
                #if UNITY_REVERSED_Z
                    o.pos.z = o.pos.w - (o.pos.w - o.pos.z) * 0.001;
                    o.pos.z -= 0.0001;
                #else
                    o.pos.z = -o.pos.w + ( o.pos.z + o.pos.w ) * 0.001;
                    o.pos.z += 0.0001;
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                #if HP_ALPHACLIP
                    fixed4 col = tex2D(_MainTex, i.uv);
                    clip(col.a - _CutOff);
                #endif
                return _SeeThroughBorderColor;
            }
            ENDCG
        }

    }
}