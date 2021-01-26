Shader "UIForia/SimpleStrokeOpaque"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" } // revert to opaque
        LOD 100
        Cull Off // todo set this to Back
                Blend SrcAlpha OneMinusSrcAlpha

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
                float4 prevNext : TEXCOORD2;
                float4 flags : TEXCOORD1;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 fragData1 : TEXCOORD1;
                float4 fragData2 : TEXCOORD2;
           };

            // idx 0 == top left
            // idx 1 == top right
            // idx 2 == bottom left
            // idx 3 == bottom right
            // idx 4 == join point top left
            // idx 5 == join point top right
            // idx 6 == join point bottom center
            
            float cross2(float2 a, float2 b ){
                return a.x * b.y - a.y * b.x;
            }
            
            float2 LineIntersection(float2 p0, float2 p1, float2 p2, float2 p3) {
                float s1_x = p1.x - p0.x;
                float s1_y = p1.y - p0.y;
                float s2_x = p3.x - p2.x;
                float s2_y = p3.y - p2.y;
                
                float s = (-s1_y * (p0.x - p2.x) + s1_x * (p0.y - p2.y)) / (-s2_x * s1_y + s1_x * s2_y);
                float t = ( s2_x * (p0.y - p2.y) - s2_y * (p0.x - p2.x)) / (-s2_x * s1_y + s1_x * s2_y);
            
                return float2(p0.x + (t * s1_x), p0.y + (t* s1_y));
                
            }            
    
            float DistToLine(float2 p, float2 line_begin, float2 line_end) {
	            float2 c1 = line_end - line_begin;
	            float2 c2 = p - line_begin;
	
	            float area = cross2(c2, c1);
	
	            return area / length(c1);
            };
            
            float2 Reflect(float2 inDirection, float2 inNormal) {
                return -2 * dot(inNormal, inDirection) * inNormal + inDirection;
            }

            v2f vert (appdata v) {
                v2f o;
                float strokeWidth = 25;
                
                #define cap 1
                #define join 2
                
                int flag = (int)v.flags.z;
                int idx = (int)v.flags.y;
                
                float2 toCurrent = normalize(curr - prev);
                float2 toNext = normalize(next - curr);
                float2 tangent = normalize(toNext + toCurrent);
                float2 miter = float2(-tangent.y, tangent.x);
                float2 normal = float2(-toCurrent.y, toCurrent.x) * extrude;
                
                float miterLength = strokeWidth / dot(miter, normal);
                
                // todo -- encode this differently to avoid conditional
                
                int leftSide = idx == 0 || idx == 2;
                int rightSide = 1 - leftSide;
                int topSide = idx < 2;
                int bottomSide = idx == 2 || idx == 3;
                
                float2 toNextPerp = float2(-toNext.y, toNext.x);
                float2 toCurrentPerp = float2(-toCurrent.y, toCurrent.x);
                float segmentLength = distance(curr, prev);
                
                float dir = dot(toCurrent, toNextPerp) <= 0 ? -1 : 1;
                float2 miterVec = miter * miterLength;
                float2 vertWithOffset = v.vertex + miterVec;

                if(flag == join && idx < 4) {
                    // todo -- figure out how to remove this if
                    if(dir > 0) {
                        float2 leftRightVec = lerp(normal, toNextPerp, leftSide) * strokeWidth;
                        float2 topBottomVec = lerp(leftRightVec, miterVec, bottomSide);
                        vertWithOffset = v.vertex + topBottomVec;
                    }
                    else {
                        float2 topBottomVec = lerp(-toNextPerp, normal, rightSide) * strokeWidth;
                        float2 leftRightVec = lerp(topBottomVec, miterVec, topSide);
                        vertWithOffset = v.vertex + leftRightVec;
                    }
                    
                    // if distance from join point to offset point is greater than segment length, limit it to segment length

                    float2 topBottomInterp = lerp(toNextPerp, -toNextPerp, 1 - topSide);
                    topBottomInterp = lerp(topBottomInterp, normal, rightSide);
                    float2 originalPosition = v.vertex + (topBottomInterp * strokeWidth);
                    
                    if(distance(originalPosition, vertWithOffset) > segmentLength) {
                        vertWithOffset = originalPosition;
                    }
                }
                
                if(idx > 3) {
                    float2 joinPoint = lerp(toCurrentPerp, toNextPerp, idx == 4);
                    vertWithOffset = v.vertex + joinPoint * strokeWidth * dir;
                }
                
                if(idx == 6) {
                    vertWithOffset = v.vertex + miterVec * dir;
                    if(distance(v.vertex, vertWithOffset) > segmentLength) {
                        vertWithOffset = v.vertex;
                    }
                }
                
                //#if USE_ROUND_OR_MITER_CLIP_JOIN
                    // round join / clipped miter
                    if(idx > 6) {
                        vertWithOffset = -miterVec;
                        if(idx == 7) {
                            vertWithOffset = normal * strokeWidth;
                        }
                        else if(idx == 8) {
                            vertWithOffset = toNextPerp * strokeWidth;
                        }                      

                        vertWithOffset = v.vertex + vertWithOffset * dir;
                     
                    }
              //  #endif
             
                if(flag == cap) {
                    vertWithOffset = v.vertex + normal * strokeWidth;   
                }
                
                o.fragData1 = float4(segmentLength, idx, bottomSide, miter.y);
                o.fragData2 = float4(v.vertex.xy, vertWithOffset.xy);
//                o.fragData2 = float4(prev, curr);
                o.color = v.color;
                if(dir < 0) o.color = fixed4(0, 0, 0, 1);
                o.vertex = UnityObjectToClipPos(float3(vertWithOffset.xy, v.vertex.z)); 
                return o;
                
           }

           // returns between 0 and 1
//           float DistToLine(float2 pt1, float2 pt2, float2 testPt) {
//              float2 lineDir = pt2 - pt1;
//              float2 perpDir = float2(lineDir.y, -lineDir.x);
//              float2 dirToPt1 = pt1 - testPt;
//              return abs(dot(normalize(perpDir), dirToPt1));
//           }

           fixed4 frag (v2f i) : SV_Target {
               int idx = (int)i.fragData1.y;
              //  return fixed4(1, 0, 0, 1);
//                    Round Join                        
//                    if(distance(i.fragData2.xy, i.fragData2.zw) > 25) {
//                        //discard;
//                    }    
//                    else {
//                        return fixed4(1, 1, 1, 1);
//                    }           
            //   }
               
               return i.color;
           }
           ENDCG
        }
    }
}
