Shader "Vertigo/VertigoText"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderQueue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend One OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 GetColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
                half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
                half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));
            
                faceColor.rgb *= faceColor.a;
                outlineColor.rgb *= outlineColor.a;
            
                faceColor = lerp(faceColor, outlineColor, outlineAlpha);
            
                faceColor *= faceAlpha;
            
                return faceColor;
            }
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f input) : SV_Target {
             float4 c = tex2D(_MainTex, input.uv.xy).a;
             float4 param = float4(0.36486, 3.69998, 0.63514, 0);
             float	scale	= param.y;
             float	bias	= param.z;
             float	weight	= param.w;
             float	sd = (bias - c) * scale;
             float sca = 0.9;
             float outline = 0; //(_OutlineWidth * sca) * scale;
             float softness = 0; //(_OutlineSoftness * sca) * scale;
             half4 faceColor = half4(1, 1, 1, 1);//_FaceColor;
             half4 outlineColor = half4(1, 1, 1, 1); //_OutlineColor;
             faceColor.rgb *= input.color.rgb;
       
             faceColor = GetColor(sd, faceColor, outlineColor, outline, softness);
             return faceColor * input.color.a;
            }
            ENDCG
        }
    }
}
