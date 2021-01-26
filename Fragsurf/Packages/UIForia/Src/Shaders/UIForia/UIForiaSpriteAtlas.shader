Shader "UIForia/UIForiaSpriteAtlas"
{
    Properties
    {
    }
    SubShader
    {
        Cull Back
        Blend Off
        ZTest Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                        
            #include "UnityCG.cginc"
                        
            struct AppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
            };

            struct FragData {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            
            FragData vert (AppData v) {

                FragData  o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texCoord0 = v.texCoord0;
                return o;
            }
         
            fixed4 frag (FragData i) : SV_Target {       
                return tex2Dlod(_MainTex, float4(i.texCoord0.xy, 0, 0));
            }

            ENDCG
        }
    }
}
