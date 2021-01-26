Shader "UIForia/TempStrokes" {
    
    Properties {  
      _StencilRef ("Stencil ID", Float) = 1
    }
    
    SubShader {

        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        Cull Off 
        Lighting Off
	    Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGBA

        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma target 3.0
           #include "UnityCG.cginc"
           #include "UIForiaInc.cginc"
           
           #define antialias 0
           
           uniform sampler2D _MainTex;
                               
           struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
                float4 uv4 : TEXCOORD4;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD0;
                float4 uv : TEXCOORD1;

           };

         
           v2f vert (appdata input) {
               v2f o;
               o.vertex = UnityObjectToClipPos(float3(input.vertex.xy, input.vertex.z));
               o.color = input.color;
               o.flags = input.uv1;
               o.uv = input.uv;
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
           return fixed4(1, 0, 0, 1);
                if(i.flags.x == LineCap_Round && i.uv.y < 0.5) {
                     float dist = length(i.uv.xy - 0.5);
                     float pwidth = length(float2(ddx(dist), ddy(dist)));
                     float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
                     i.color = fixed4(i.color.rgb, i.color.a * alpha);
                }
                return fixed4(i.color.rgb, 0.5);
           }
           
            ENDCG
        }
    }
}
