Shader "UIForia/UIForiaPathSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}       
        _SrcBlend ("__srcBlend", Float) = 1
        _DstBlend ("__dstBlend", Float) = 1
        _ZWrite ("__zWrite", Float) = 1.0
        _ZTest ("__zTest", Float) = 1.0
        _BlendOp ("__blndop", Float) = 1.0
    }
    SubShader
    {
        LOD 100
        //Blend One OneMinusSrcAlpha
        Cull Off // lines often come in reversed
        
        BlendOp [_BlendOp]
        Blend [_SrcBlend][_DstBlend]
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE SHADOW PRE_MULTIPLY_ALPHA
            
            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
            #include "UIForiaSDFUtil.cginc"
            
            struct PathSDFAppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoord1 : TEXCOORD1;
            };

            struct UIForiaPathFragData {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
                nointerpolation float4 texCoord1 : TEXCOORD1;
                nointerpolation float4 texCoord2 : TEXCOORD2;
            };
            
            #define Vert_ObjectIndex v.texCoord1.w
            #define Frag_ObjectIndex i.texCoord1.w
            #define Frag_StrokeWidth objectInfo.w
            #define Frag_ShapeType i.texCoord2.z
            #define Frag_PaintMode i.texCoord2.w
            #define ObjectInfo_CornerRadii objectInfo.y

            float4 _ObjectData[BATCH_SIZE];
            float4 _GradientData[BATCH_SIZE];
            float4 _ColorData[BATCH_SIZE];

            // todo -- better not to use a 4x4, paths are always 2d
            float4x4 _TransformData[BATCH_SIZE];

            sampler2D _MainTexture;

            UIForiaPathFragData vert (appdata v) {
            
                float4 objectInfo = _ObjectData[(int)Vert_ObjectIndex];
                float4 gradientInfo = _GradientData[(int)Vert_ObjectIndex];
                half2 size = UnpackSize(objectInfo.z);
                uint packedFlags = objectInfo.x;
                uint shapeType = (packedFlags >> 16) & (1 << 16) - 1;
                uint colorMode = packedFlags & 0xffff;
                float4x4 transform = _TransformData[(int)Vert_ObjectIndex];

                UIForiaPathFragData  o;
                v.vertex = mul(transform, float4(v.vertex.xyz, 1));
                o.vertex = UnityObjectToClipPos(v.vertex);
                // if pixel snapping is on some shapes get cut off. find a way to account for this
               //  o.vertex = UIForiaPixelSnap(o.vertex);
                o.texCoord0 = v.texCoord0;
                o.texCoord1 = v.texCoord1;
                o.texCoord2 = float4(size.x, size.y, shapeType, colorMode);
                return o;
            }
            
            float SmoothRect(float2 uv, float2 pos, float2 size, float sX, float sY) {
               float2 end = pos + size;
               return smoothstep(pos.x - sX, pos.x + sX, uv.x) 
                   * (1.0 - smoothstep(end.x - sX, end.x + sX, uv.x))
                   * smoothstep(pos.y - sY, pos.y + sY, uv.y)
                   * (1.0 - smoothstep(end.y - sY, end.y + sY, uv.y));
            }
            
            fixed4 ShadowFragment(float2 pos) {

               float shadowSoftnessX = 0.75;
               float shadowSoftnessY = 0.25;
               
               float shadowAlpha = 1; 
               fixed4 shadowTint = fixed4(0, 1, 0, 0);
               float2 shadowSize = float2(1, 1);
               float2 shadowPosition =  float2((1 - shadowSize.x) * 0.5, (1 - shadowSize.y) * 0.5);
               
               float shadowRect = shadowAlpha * SmoothRect(pos, shadowPosition, shadowSize, shadowSoftnessX, shadowSoftnessY);
               
               float a = smoothstep(0.3, 0.9, shadowRect);
               fixed4 shadowColor = fixed4(1, 1,1, 1);
               fixed4 color = lerp(fixed4(shadowTint.rgb, 0), fixed4(shadowColor.rgb, shadowColor.a * a), a);
               color = lerp(fixed4(1, 1, 1, 0), color, a);
               color.rgb *= color.a;
               return color;
               
            }
            
            fixed4 frag (UIForiaPathFragData i) : SV_Target {              
                float2 size = i.texCoord2.xy;
                float minSize = min(size.x, size.y);

                float4 objectInfo = _ObjectData[(int)Frag_ObjectIndex];
                float4 colorInfo = _ColorData[(int)Frag_ObjectIndex];
                uint packedFlags = (objectInfo.x);
                uint paintMode = (packedFlags & 0xffff);
                
                fixed4 mainColor = ComputeColor(colorInfo.r, colorInfo.g, paintMode, i.texCoord0.xy, _MainTexture);
                mainColor.a *= colorInfo.b;
                
                int isStroke = Frag_StrokeWidth > 0;
                float halfStrokeWidth = max(Frag_StrokeWidth, 0) * 0.5;
                int isShadow = (paintMode & PaintMode_Shadow) != 0;
                int shapeType = Frag_ShapeType;
                float2 center = (i.texCoord0.xy - 0.5) * size;

                float shadowIntensity = colorInfo.a;
                
                fixed4 inner = mainColor;
                fixed4 outer = fixed4(mainColor.rgb, 0);

                float sdf = 0;
                float percentRadius = UnpackCornerRadius(ObjectInfo_CornerRadii, i.texCoord0.zw);
                float radius = clamp(minSize * percentRadius, 0, minSize);
                float cut = radius;
                float halfX = size.x * 0.5;
                float halfY = size.y * 0.5;
                
                fixed hDir = lerp(-1, 1, i.texCoord0.x > 0.5);
                fixed vDir = lerp(-1, 1, i.texCoord0.y > 0.5);
                
                float2 p0 = float2(hDir * (halfX - cut), vDir * halfY);
                float2 p1 = float2(hDir * size.x, vDir * size.y); // big on purpose so we don't get bad bleeding of non clipped edge
                float2 p2 = float2(hDir * halfX, vDir * (halfY - cut));
                int mainColorOnly = 0;
                
                if(shapeType == ShapeType_Ellipse) {
                    halfStrokeWidth = halfStrokeWidth / max(size.x, size.y);
                    sdf = EllipseSDF(i.texCoord0.xy - 0.5, float2(0.49, 0.49));
                }
                else if((shapeType & ShapeType_RectLike) != 0) {      
                    cut = 0; // todo -- actually need to unpack this like radii
                    sdf = RectSDF(center,  (size * 0.5) - halfStrokeWidth, radius - halfStrokeWidth);
                }
                else if(shapeType == ShapeType_Sector) {
                    cut = 0;
                    float angle = i.texCoord1.x;
                    float width = i.texCoord1.y;
                    float rotation = i.texCoord1.z;
                    // if stroking the geometry is twice as large to account for weirdness 
                    // with stroke length going off the geometry and clipping horribly
                    float ta = 3.14 * 0.33;
                    float tb = 3.14 * 0.66;
                    sdf = SDFArc(center, ta, tb, (minSize * 0.5) - width - halfStrokeWidth, width);
                    if(isStroke) sdf = abs(sdf) - halfStrokeWidth;
                    
                }        
                else if(shapeType == ShapeType_Triangle) {
                    // for triangle case already call sdTriangle for cut, but triangle never uses cut so we hijack the call since its expensive
                     // store last point in objectInfo since we don't have radii for triangles and we can't use texCoord1.w because thats where object index is stored
                     center = i.texCoord0.xy * size;
                     p0 = i.texCoord0.zw * size;
                     p1 = i.texCoord1.xy * size;//;
                     p2 = float2(i.texCoord1.z, objectInfo.y) * size;
                }
                else {
                    mainColorOnly = 1; // just returning mainColor here causes NINE extra branches. Don't do it!
                }
                
                float tri = sdTriangle(center, p0, p1, p2);

               // sdf = lerp(lerp(sdf, subtractSDF(sdf, tri), cut != 0), tri, shapeType == ShapeType_Triangle);
                // todo -- use alpha blend somehow to mix these colors
               // #if SHADOW
                    float n = smoothstep(-shadowIntensity, 2, sdf);
                    fixed4 shadowColor = fixed4(UnpackColor(asuint(colorInfo.r)).rgb, 1);
                    fixed4 shadowTint = fixed4(UnpackColor(asuint(colorInfo.g)).rgb, 1);
            
                    fixed4 shadowRetn = lerp(fixed4(shadowColor.rgb, 1 - n), shadowColor, (1 - n));                
                    fixed4 tintedShadowColor = lerp(fixed4(shadowTint.rgb, 1 - n), shadowColor,  (1 - n));
                    tintedShadowColor = lerp(shadowRetn, tintedShadowColor, n);
                    
                    shadowRetn = lerp(shadowRetn, tintedShadowColor, (paintMode & PaintMode_ShadowTint) != 0);
                    shadowRetn.a *= colorInfo.b;
               // #else
                  //  fixed4 shadowRetn = fixed4(0, 0, 0, 0);
               // #endif
                
                float distanceChange = fwidth(sdf) * 0.5;
                float aa = smoothstep(distanceChange, -distanceChange, sdf);
               // #if PRE_MULTIPLY_ALPHA
                    inner.rgb *= inner.a;
                    outer.rgb *= outer.a;
                    shadowRetn.rgb *= shadowRetn.a;
               // #endif
                
                fixed4 sdfRetn = lerp(inner, outer, 1 - aa);
                fixed4 retn = lerp(sdfRetn, shadowRetn, isShadow && shadowIntensity > 1);
              //  return lerp(retn, mainColor, mainColorOnly);
              //  return sdfRetn; // todo - stop cheating
                
                #if MASK_OUTPUT
                    fixed a = lerp(retn.a, mainColor.a, mainColorOnly);
                    // todo -- mask my target channel
                    return fixed4(a, a, a, a);
                #else
                    return lerp(retn, mainColor, mainColorOnly);
                #endif
            }

            ENDCG
        }
    }
}
