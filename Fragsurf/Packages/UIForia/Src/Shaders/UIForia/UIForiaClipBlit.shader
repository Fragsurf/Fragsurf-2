Shader "UIForia/UIForiaClipBlit"
{
    Properties
    {
        
    }
    SubShader
    {
        Cull Back
        BlendOp Min
        Blend One One
        ZWrite Off
        ZTest Equal
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                        
            #include "UnityCG.cginc"
                        
            struct PathSDFAppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
            };

            struct UIForiaPathFragData {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            
            UIForiaPathFragData vert (PathSDFAppData v) {

                UIForiaPathFragData  o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texCoord0 = v.texCoord0;
                o.texCoord0.y = 1 - o.texCoord0.y;  
                return o;
            }
            
            bool inRange( float a, float b, float range) {
                return ( (a > b  - range) &&   (a < b + range));
            }
            
            fixed4 frag (UIForiaPathFragData i) : SV_Target {        
                fixed4 color = tex2Dlod(_MainTex, float4(i.texCoord0.xy, 0, 0));
                fixed e = 0.05;
                fixed val = color.r;
                int expected = i.texCoord0.z;
                float approx = (10 * val);
                
                if(inRange(approx, expected, e)) {
                    return fixed4(1, 1, 1, 1);
                }
                else {
                    return fixed4(0, 0, 0, 0);
                }
         
            }

            ENDCG
        }
    }
}
