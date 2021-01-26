Shader "UIForia/StencilFillCutout"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        ColorMask 0
        /*
        Why this works:
            
            1st pass (UIForiaClip) renders a clip value of 0 or 1 into stencil buffer
            2nd pass (UIForiaStencil) renders a fill value of 1 into the stencil buffer at bit 2
            3rd pass (This one) looks for all pixels with both the clip bit and the fill bit set (ie 3) and only renders there
        */
        Stencil {
        
            Ref 1 // use 3 for clip
            Comp Always
            Pass Invert
            WriteMask 1 // 2 for clip
            
        }
        
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }
            ENDCG
        }
    }
}
