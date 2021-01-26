Shader "UIForia/JoinedPolyline" {
    Properties {    }
    SubShader {

        Tags { "RenderType"="Transparent" "DisableBatching"="True" }
        Cull Back 
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma enable_d3d11_debug_symbols

           #include "UnityCG.cginc"
           
           #define antialias 2
           
           struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 flags : TEXCOORD1;
                float4 prevNext : TEXCOORD2;
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

               float2 prev = input.prevNext.xy;
               float2 next = input.prevNext.zw;
               float2 curr = input.vertex.xy;
               
               float strokeWidth = input.flags.w;
               
               float aa = strokeWidth < 2 ? 1 : antialias;
               
               int idx = input.flags.y;
               
               float w = (strokeWidth * 0.5) + aa;
               float dir = input.flags.x;
               
               float2 v0 = normalize(curr - prev);
               float2 v1 = normalize(next - curr);
               
               float2 n0 = float2(-v0.y, v0.x);
               float2 n1 = float2(-v1.y, v1.x);
               
               float2 miter = normalize(n0 + n1);
                        
               float miterLength = w / dot(miter, n1);
               float2 pos = curr + (miter * miterLength * dir);
               o.color = input.color;
               o.uv = float4(w, w * dir, 0, 0);
               
               o.flags = float4(strokeWidth, w * dir, aa, curr.x == next.x || curr.y == next.y);
               
               // todo -- support flags for pushing stroke to inside or outside of shape
               // for pushing stroke outwards: pos.xy - (n1 * strokeWidth * 0.5)
               // for pushing stroke inwards: pos.xy + (n1 * strokeWidth * 0.5)
               
               o.vertex = UnityObjectToClipPos(float3(pos.xy, input.vertex.z));
               
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
               
               float thickness = i.flags.x;
               float aa = i.flags.z;
               float w = (thickness * 0.5) - aa;

               float d = abs(i.uv.y) - w;
               
               if(d <= 0) {
                   return i.color;
               }

               d /= aa;
               float threshold = 1;
               float afwidth = length(float2(ddx(d), ddy(d)));
               float alpha = smoothstep(threshold - afwidth, threshold + afwidth, d);
               return fixed4(i.color.rgb, i.color.a * (1 - alpha));
               
           }
           ENDCG
        }
    }
}
