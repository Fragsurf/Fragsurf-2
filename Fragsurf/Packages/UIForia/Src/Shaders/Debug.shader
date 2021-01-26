Shader "UIForia/Debug"
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
            uniform float4 _MarginRect;
            uniform float4 _BorderRect;
            uniform float4 _PaddingRect;
            uniform float4 _ContentRect;
            uniform fixed4 _ContentColor;
            uniform fixed4 _PaddingColor;
            uniform fixed4 _BorderColor;
            uniform fixed4 _MarginColor;
            
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
            
            int PointInRect(float2 p, float4 rect) {
                if(p.x >= rect.x && p.x <= rect.x + rect.z && p.y >= rect.y && p.y <= rect.y + rect.w) {
                    return 1;
                }
                return 0;
            }
            
            fixed4 frag(v2f IN) : SV_Target {   
                float2 p = float2(IN.texcoord.x * _Size.x, ( 1 - IN.texcoord.y) * _Size.y);
                
                if(PointInRect(p, _ContentRect) == 1) {
                    return _ContentColor;  
                }
                
                if(PointInRect(p, _PaddingRect) == 1) {
                    return _PaddingColor;
                }
                
                if(PointInRect(p, _BorderRect) == 1) {
                    return _BorderColor;
                }
                
                if(PointInRect(p, _MarginRect) == 1) {
                    return _MarginColor;
                }
             
                return fixed4(0, 0, 0, 0);
            }
            
            
        ENDCG
        }
    }
}
