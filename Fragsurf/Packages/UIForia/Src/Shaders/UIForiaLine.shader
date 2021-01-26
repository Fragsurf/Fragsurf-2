Shader "UIForia/UIForiaLine"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off // todo set this to Back
        
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
    
            #define prev v.prevNext.xy
            #define curr v.vertex.xy
            #define next v.prevNext.zw
            #define extrude v.flags.x
            #define leftRight v.flags.y
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 prevNext : TEXCOORD1;
                float4 flags : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                
                float strokeWidth = 5;
                
                if(v.flags.z == 5) {
                    float2 fromCurrent = normalize(next - curr);
                    float2 normal = float2(-fromCurrent.y, fromCurrent.x) * strokeWidth * extrude;
                    float3 vertWithOffset = float3(v.vertex.x, v.vertex.y, 0);
                    vertWithOffset.x += normal.x;
                    vertWithOffset.y += normal.y;
                    o.color = v.color;
                    o.vertex = UnityObjectToClipPos(vertWithOffset);
                }
                else if(v.flags.z == 6) {
                    float2 toCurrent = normalize(curr - prev);
                    float2 normal = float2(-toCurrent.y, toCurrent.x) * strokeWidth * extrude;
                    float3 vertWithOffset = float3(v.vertex.x, v.vertex.y, 0);
                    vertWithOffset.x += normal.x;
                    vertWithOffset.y += normal.y;
                    o.color = v.color;
                    o.vertex = UnityObjectToClipPos(vertWithOffset);
                }
                else {
                    float2 toCurrent = normalize(curr - prev);
                    float2 toNext = normalize(next - curr);
                    float2 tangent = normalize(toNext + toCurrent);
                    float2 miter = float2(-tangent.y, tangent.x);
                    float2 normal = float2(-toCurrent.y, toCurrent.x) * extrude;
                    
                    float miterLength = strokeWidth / dot(miter, normal);
                    
                    float3 vertWithOffset = float3(v.vertex.x, v.vertex.y, 0);
                    vertWithOffset.x += miter.x * miterLength;
                    vertWithOffset.y += miter.y * miterLength;
                    o.color = v.color;
                    o.vertex = UnityObjectToClipPos(vertWithOffset); 
                }

                return o;
            }
            
            fixed MapMinusOneOneToZeroOne(fixed val) {
                return val * 0.5 + 0.5;
            }

            fixed4 frag (v2f i) : SV_Target {
                return i.color;
            }
            
            ENDCG
        }
    }
}
