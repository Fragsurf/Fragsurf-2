Shader "UIForia/StencilClipSet"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "DisableBatching"="True" }
        LOD 100

        Stencil {
            Ref 1
            Comp Always
            WriteMask 1
            Pass Replace
        }
        
        ColorMask 0

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
          
            #include "UnityCG.cginc"
            #include "UIForiaInc.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.flags = v.flags;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                #define ShapeType i.flags.x
                
                if(ShapeType > ShapeType_Path) {
                    float dist = length(i.uv.xy - 0.5);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
                    
                    if(alpha - 0.01 <= 0) discard;
                    
                }
                
                return fixed4(0, 0, 0, 1);
            }
            ENDCG
        }
    }
}
