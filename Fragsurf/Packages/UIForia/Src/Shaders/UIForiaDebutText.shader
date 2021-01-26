Shader "UIForia/DebugText"
{
    Properties
    {

        _MainTex ("Sprite Texture", 2D) = "white" {}
        
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
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
        Lighting Off
        ZWrite On
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            uniform float4 _Size;
            uniform float _BaseLine;
            uniform float _Descender;
            uniform fixed4 _BaseLineColor;
            uniform fixed4 _DescenderColor;
                        
            struct appdata_t {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata_t v)  {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }
            
           
            
            fixed4 frag(v2f IN) : SV_Target {
                float2 p = float2(IN.texcoord.x * _Size.x, ( 1 - IN.texcoord.y) * _Size.y);
                if(p.y >= _BaseLine && p.y <= _BaseLine + 1) {
                    return _BaseLineColor;
                }
                if(p.y >= _Descender && p.y <= _Descender + 1) {
                    return _DescenderColor;
                }
                return fixed4(0, 0, 0, 0);
            }
            
            
        ENDCG
        }
    }
}
