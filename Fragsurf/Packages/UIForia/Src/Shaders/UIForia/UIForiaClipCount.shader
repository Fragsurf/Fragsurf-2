Shader "UIForia/UIForiaClipCount"
{
    Properties
    {
    }
    SubShader
    {
        Cull Back
        BlendOp Add
        Blend One One
        ZWrite Off
        ZTest Equal
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE
            
            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
                        
            struct PathSDFAppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoord1 : TEXCOORD1;
            };

            struct UIForiaPathFragData {
                float4 vertex : SV_POSITION;
            };
            
            #define Vert_ObjectIndex v.texCoord1.w
            float4x4 _TransformData[BATCH_SIZE];
            
            UIForiaPathFragData vert (PathSDFAppData v) {

                float4x4 transform = _TransformData[(int)Vert_ObjectIndex];
                UIForiaPathFragData  o;
                v.vertex = mul(transform, float4(v.vertex.xyz, 1));
                o.vertex = UnityObjectToClipPos(v.vertex);
                // if pixel snapping is on some shapes get cut off. find a way to account for this
                //  o.vertex = UIForiaPixelSnap(o.vertex);
                return o;
            }
         
            fixed4 frag (UIForiaPathFragData i) : SV_Target {          
                // todo -- mask by channel
               return fixed4(0.1, 0.1, 0.1, 0.1);
                //return fixed4(1 / 256, 1 / 256, 1 / 256, 1 / 256);               
            }

            ENDCG
        }
    }
}
