Shader "UIForia/UIForiaClearClipRegions"
{
    Properties
    {
    }
    SubShader
    {
        Cull Back
        ZWrite On
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
                        
            #include "UnityCG.cginc"
                        
            struct AppData {
                float4 vertex : POSITION;
            };

            struct FragData {
                float4 vertex : SV_POSITION;
            };
            
            fixed4 _Color;
            
            FragData vert (AppData v) {          
                FragData  o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (FragData i) : SV_Target { 
                return _Color;
            }

            ENDCG
        }
    }
}
