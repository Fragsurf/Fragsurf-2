Shader "UIForia/SimpleFillOpaque"
{
    Properties
    {
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilRef ("Stencil ID", Float) = 1
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "DisableBatching"="True" }

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA
        
        Stencil {
            Ref [_StencilRef]
            Comp Equal
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UIForiaInc.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _globalGradientAtlas;
            uniform float _globalGradientAtlasSize;
         
            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0; // xy = origin, zw = size
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1; // x = shape type, y = fill mode, z = gradientId, w = gradientDirection
                float4 fillSettings : TEXCOORD2;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
                float4 colorFlags : TEXCOORD2;
            };
            
            v2f vert (appdata v) {
            
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // todo -- support scaling and repeating uvs
                if(v.flags.x == ShapeType_Circle) {
                    o.uv = v.uv.xy;
                }
                else {
                    o.uv = float2((v.vertex.x - v.uv.x) / (v.uv.z + 1), (v.vertex.y + v.uv.y) / (v.uv.w + 1));
                }
                
                o.color = v.color;
                o.flags = v.flags;
                
                uint fillFlags = (uint)v.flags.y;
                
                uint texFlag = (fillFlags & FillMode_Texture) != 0;
                uint gradientFlag = (fillFlags & FillMode_Gradient) != 0;
                uint tintFlag = (fillFlags & FillMode_Tint) != 0;
                
                o.colorFlags = float4(texFlag, gradientFlag, tintFlag, texFlag + gradientFlag + tintFlag);
                return o;
            }        

            fixed4 frag (v2f i) : SV_Target {
                
                // todo -- use feature flags and make renderer determine if they can be used 
                
                #define ShapeType i.flags.x
                #define GradientId i.flags.z
                #define GradientDirection i.flags.w
                                
                float t = lerp(i.uv.x, i.uv.y, GradientDirection);
                float y = GetPixelInRowUV(GradientId, _globalGradientAtlasSize);

                fixed4 color = i.color;
                fixed4 textureColor = tex2D(_MainTex, i.uv);
                fixed4 gradientColor = tex2Dlod(_globalGradientAtlas, float4(t, y, 0, 0));
                fixed4 tintColor = lerp(fixed4(1, 1, 1, 1), color, i.colorFlags.x);
                                
                textureColor = lerp(color, textureColor, i.colorFlags.x);
                textureColor = lerp(textureColor, textureColor * tintColor, tintColor.a);
                gradientColor = lerp(color, gradientColor, i.colorFlags.y);
                
                // todo -- support mixing gradient with texture
                color = lerp(textureColor, gradientColor, 0);
                
                color = lerp(color, i.color, 0);
                                
                if(ShapeType > ShapeType_Path) {
                    float dist = length(i.uv - 0.5);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
                                        
                    color = fixed4(color.rgb, color.a * alpha);
                }
                             
                if(color.a - 0.001 <= 0) {
                    discard;
                }
                   
                return color;
            }

                 
            ENDCG
        }
        
    }
}
