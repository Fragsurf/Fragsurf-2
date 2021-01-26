Shader "UIForia/SimpleLineSegments" {
    Properties {    }
    SubShader {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Cull Off // todo set this to Back
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma enable_d3d11_debug_symbols

           #include "UnityCG.cginc"
           
           #define antialias 1.5
           
           float ComputeU(float2 p0, float2 p1, float2 p) {
               float2 v = p1 - p0;
               return ((p.x - p0.x) * v.x + (p.y - p0.y) * v.y) / length(v); 
           }
           
           float LineDistance(float2 p0, float2 p1, float2 p) {
               float2 v = p1 - p0;
               float l2 = v.x * v.x + v.y * v.y;
               float u = ((p.x - p0.x) * v.x + (p.y - p0.y) * v.y) / l2;
               
               float2 h = p0 + u * v;
               return length(p - h);
           }
           
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
                float2 v_p0 : TEXCOORD1;
                float2 v_p1 : TEXCOORD2;
                float2 v_p : TEXCOORD3;
           };

           v2f vert (appdata input) {
               v2f o;
               float thickness = input.flags.x;
               float alpha = 0;
               float2 p0 = float2(input.prevNext.x, input.prevNext.y);
               float2 p1 = float2(input.prevNext.z, input.prevNext.w);
               float2 uv = input.uv;
               
               if( abs(thickness) < 1.0 ) {
                    thickness = 1.0;
                    alpha = abs(thickness);
               } else {
                    thickness = abs(thickness);
                    alpha = 1.0;
               } 
               
               float t = thickness / 2.0 + antialias;
               float l = length(p1 - p0);
               
               // map uv to [-1, 1]
               float u = 2.0 * uv.x - 1.0;
               float v = 2.0 * uv.y - 1.0;
               
               // Screen space
               float2 tangent = normalize(p1 - p0);
               float2 normal = float2(-tangent.y , tangent.x);
               float2 p = p0 + float2(0.5, 0.5) + uv.x * tangent * l + u * tangent * t + v * normal * t;
               float2 pos = p;
               
               // Local space
               tangent = float2(1.0, 0.0);
               normal = float2(0.0, 1.0);
               
               p = uv.x * tangent * l + u * tangent * t + v * normal * t;
               o.v_p0 = float2(0.0, 0.0);
               o.v_p1 = float2(  l, 0.0);
               o.v_p  = p;
               
               o.flags = float4(thickness, alpha, 0, 0);
               o.color = input.color;
               o.vertex = UnityObjectToClipPos(float3(pos.x, pos.y, input.vertex.z));
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
               float2 v_p = i.v_p;
               float2 v_p0 = i.v_p0;
               float2 v_p1 = i.v_p1;
               float v_thickness = i.flags.x;
               float v_alpha = i.flags.y;
               
               float d = 0;
               
               if( v_p.x < 0 ) {
                   d = length(v_p - v_p0) - v_thickness/2.0 + antialias/2.0;
               }    
               else if ( v_p.x > length(v_p1-v_p0) )
                   d = length(v_p - v_p1) - v_thickness/2.0 + antialias/2.0;
               else {
                   d = abs(v_p.y) - v_thickness/2.0 + antialias/2.0;
               }
                   
               if( d < 0) {
                   return fixed4(i.color.rgb, 1.0);
               }
               else if (d < antialias) { 
                   d = exp(-d*d);
                   return fixed4(i.color.rgb, d);
               }
               return fixed4(0, 0, 0, 0);
           }
           ENDCG
        }
    }
}
