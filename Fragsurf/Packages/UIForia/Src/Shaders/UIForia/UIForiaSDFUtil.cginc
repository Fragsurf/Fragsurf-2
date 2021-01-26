#ifndef UIFORIA_SDF_INCLUDE
#define UIFORIA_SDF_INCLUDE

#include "./UIForiaStructs.cginc"

const float SQRT_2 = 1.4142135623730951;
            
                           
#define PaintMode_Color (1 << 0)
#define PaintMode_Texture (1 << 1)
#define PaintMode_TextureTint (1 << 2)
#define PaintMode_LetterBoxTexture (1 << 3)
#define PaintMode_Shadow (1 << 4)
#define PaintMode_ShadowTint (1 << 5)

fixed4 UIForiaColorSpace(fixed4 color) {
    #ifdef UNITY_COLORSPACE_GAMMA
        return color;
    #else
        return fixed4(GammaToLinearSpace(color.rgb), color.a);
    #endif
}

// remap input from one range to an other            
inline float Map(float s, float a1, float a2, float b1, float b2) {
    return b1 + (s - a1) * (b2 - b1) / ( a2 - a1);
}           

// might divide by 0           
inline float PercentOfRange(float v, float minVal, float maxVal) {
    return (v - minVal) / (maxVal - minVal);
}

float subtractSDF(float base, float subtraction){
    return max(base, -subtraction);
}

float sdTriangle(float2 p, float2 p0, float2 p1, float2 p2 ) {
    float2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
    float2 v0 = p -p0, v1 = p -p1, v2 = p -p2;
    float2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
    float2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
    float2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
    float s = sign( e0.x*e2.y - e0.y*e2.x );
    float2 d = min(min(float2(dot(pq0,pq0), s*(v0.x*e0.y-v0.y*e0.x)),
                     float2(dot(pq1,pq1), s*(v1.x*e1.y-v1.y*e1.x))),
                     float2(dot(pq2,pq2), s*(v2.x*e2.y-v2.y*e2.x)));
    return -sqrt(d.x)*sign(d.y);
}

float2 UnpackSize(float packedSize);

fixed4 UIForiaAlphaClipColor(fixed4 color, sampler2D clipTexture, float2 clipPos, float4 clipData, float4 clipUvs) {    
    // todo -- if mask render texture is packed with padding we need to account for that padding
    // todo -- mask at half resolution
    // todo -- for clipping we don't want to blend do much, for masking we might
    
    half2 unpackedSizeXY = UnpackSize(clipData.x);
    half2 unpackedSizeZW = UnpackSize(clipData.y);
    

    // point in rect, does not handle rotation, need to be sure box & point are in the same same aligned coordinate space
    float2 s = step(float2(unpackedSizeXY.x, unpackedSizeZW.y), clipPos) - step(float2(unpackedSizeZW.x , unpackedSizeXY.y), clipPos);
    
    fixed4 retn = color;
    
    // float x = PercentOfRange(clipPos.x, unpackedSizeXY.x, unpackedSizeZW.x);
    // loat y = PercentOfRange(clipPos.y, unpackedSizeXY.y, unpackedSizeZW.y);
    
    // x = Map(x, 0, 1, clipUvs.x, clipUvs.z);
    // y = Map(y, 0, 1, clipUvs.y, clipUvs.w);

    // y comes in [0 - 1], need to sample with [1, 0]
    // todo -- this is the nested clipping feature using complex shapes. currently not working, probably 
    // the data input is wrong for non text things. text works but this might be a bug in that the wrong
    // data gets pushed into the clipData vector
   // fixed a = tex2Dlod(clipTexture, float4(x, 1 - y, 0, 0))[(int)clipData.z]; // z is the channel the target mask is on
   // retn = lerp(retn, fixed4(retn.rgb, lerp(color.a, a, 1 - a)), a < 1 && color.a > 0 && (clipUvs.z + clipUvs.w) != 0);
    retn = lerp(retn, fixed4(0, 0, 0, 0), (s.x * s.y) == 0);     
    return retn;
}
                                   
// same as UnityPixelSnap except taht we add 0.5 to pixelPos after rounding
inline float4 UIForiaPixelSnap (float4 pos) {
     float2 hpc = _ScreenParams.xy * 0.5f;
     float2 adjustment = float2(0, 0);
     
     if((uint)_ScreenParams.y % 2 != 0) {
        adjustment.y = 0.5;
     }
     
     if((uint)_ScreenParams.x % 2 != 0) {
        adjustment.x = 0.5;
     }
     adjustment.x = (_ScreenParams.x % 2 != 0) * 0.5;
     adjustment.y = (_ScreenParams.y % 2 != 0) * 0.5;
     float2 pixelPos = round ((pos.xy / pos.w) * hpc) + adjustment;
     pos.xy = pixelPos / hpc * pos.w;
     return pos;
}

inline float pie(float2 p, float angle) {
    angle = radians(angle) * 0.5;
    float2 n = float2(cos(angle), sin(angle));
    return abs(p).x * n.x + p.y * n.y;
}
            
inline float sector(float2 p, float radius, float angle, float width) {
    width *= 0.5;
    radius -= width;
    return max(-pie(p, angle), abs(length(p) - radius) - width);
}
            
inline float RectSDF(float2 p, float2 size, float r) {
   float2 d = abs(p) - size + float2(r, r);
   return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
}

float EllipseSDF(float2 p, float2 r) {
    float k0 = length(p / r);
    float k1 = length(p/ (r * r));
    return k0 * (k0 - 1.0) / k1;
}

float RhombusSDF(float2 p, float2 size) {
    #define ndot(a, b) (a.x * b.x - a.y * b.y)
    
    float2 q = abs(p);
    float h = clamp( (-2.0*ndot(q,size) + ndot(size,size) )/dot(size,size), -1.0, 1.0 );
    float d = length( q - 0.5*size*float2(1.0-h,1.0+h) );
    return d * sign( q.x*size.y + q.y*size.x - size.x*size.y );
}

float DiamondSDF(float2 P, float size) {
    const float SQRT_2 = 1.4142135623730951;

    float x = SQRT_2/2.0 * (P.x - P.y);
    float y = SQRT_2/2.0 * (P.x + P.y);
    return max(abs(x), abs(y)) - size/(2.0*SQRT_2);
}

float TriangleSDF(float2 p, float2 p0, float2 p1, float2 p2) {
    float2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
    float2 v0 = p -p0, v1 = p -p1, v2 = p -p2;

    float2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
    float2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
    float2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
    
    float s = sign( e0.x*e2.y - e0.y*e2.x );
    float2 d = min(min(float2(dot(pq0,pq0), s*(v0.x*e0.y-v0.y*e0.x)),
                     float2(dot(pq1,pq1), s*(v1.x*e1.y-v1.y*e1.x))),
                     float2(dot(pq2,pq2), s*(v2.x*e2.y-v2.y*e2.x)));

    return -sqrt(d.x) * sign(d.y);
}

// https://www.shadertoy.com/view/MtScRG
// todo figure out parameters and coloring, this probably is returning alpha value that needs to be smoothsteped
float PolygonSDF(float2 p, int vertexCount, float radius) {
    // two pi
    float segmentAngle = 6.28318530718 / (float)vertexCount;
    float halfSegmentAngle = segmentAngle*0.5;

    float angleRadians = atan2(p.y, p.x);
    float repeat = (angleRadians % segmentAngle) - halfSegmentAngle;
    float inradius = radius*cos(halfSegmentAngle);
    float circle = length(p);
    float x = sin(repeat)*circle;
    float y = cos(repeat)*circle - inradius;

    float inside = min(y, 0.0);
    float corner = radius*sin(halfSegmentAngle);
    float outside = length(float2(max(abs(x) - corner, 0.0), y))*step(0.0, y);
    return inside + outside;
}

half2 UnpackToHalf2(float value) {
	const int PACKER_STEP = 4096;
	const int PRECISION = PACKER_STEP - 1;
	half2 unpacked;

	unpacked.x = (value % (PACKER_STEP)) / (PACKER_STEP - 1);
	value = floor(value / (PACKER_STEP));

	unpacked.y = (value % PACKER_STEP) / (PACKER_STEP - 1);
	return unpacked;
}

float4 UnpackColor(uint input) {
    return float4(
        uint((input >> 0) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 24) & 0xff) / float(0xff)
    );
}

inline int and(int a, int b) {
    return a * b;
}

float4 UnpackSDFParameters(float packed) {
    uint packedInt = asuint(packed);
    int shapeType = packedInt & 0xff;
    return float4(shapeType, 0, 0, 0);
}

inline float4 UnpackSDFRadii(float packed) {
    uint packedRadii = asuint(packed);
    return float4(
        uint((packedRadii >>  0) & 0xff),
        uint((packedRadii >>  8) & 0xff),
        uint((packedRadii >> 16) & 0xff),
        uint((packedRadii >> 24) & 0xff)
    );
}

inline float UnpackCornerRadius(float packed, float2 texCoord) {

    uint packedRadiiUInt = asuint(packed);
    float percentRadius = 0;
    
    float left = step(texCoord.x, 0.5); // 1 if left
    float bottom = step(texCoord.y, 0.5); // 1 if bottom
    
    #define top (1 - bottom)
    #define right (1 - left) 
    
    percentRadius += (top * left) * uint((packedRadiiUInt >> 0) & 0xff);
    percentRadius += (top * right) * uint((packedRadiiUInt >> 8) & 0xff);
    percentRadius += (bottom * left) * uint((packedRadiiUInt >> 16) & 0xff);
    percentRadius += (bottom * right) * uint((packedRadiiUInt >> 24) & 0xff);
    // radius comes in as a byte representing 0 to 50 of our width, remap 0 - 250 to 0 - 0.5
    percentRadius = (percentRadius * 2) / 1000; 
    return percentRadius;
    #undef top
    #undef right
}

inline float2 UnpackSize(float packedSize) {
    uint input = asuint(packedSize);
    uint high = (input >> 16) & (1 << 16) - 1;
    uint low =  input & 0xffff;
    return float2(high / 10, low / 10);
}

float3x3 TRS2D(float2 position, float2 scale, float rotation) {
    const float a = 1;
    const float b = 0;
    const float c = 0;
    const float d = 1;
    const float e = 0;
    const float f = 0;
    float ca = 0;
    float sa = 0;
    
    sincos(rotation, ca, sa);  
    
    return transpose( float3x3(
        (a * ca + c * sa) * scale.x,
        (b * ca + d * sa) * scale.x, 0, 
        (c * ca - a * sa) * scale.y,
        (d * ca - b * sa) * scale.y, 0,
        a * position.x + c * position.y + e,
        b * position.x + d * position.y + f, 
        1
    ));
}
            
#define ShapeType_Rect (1 << 0)
#define ShapeType_RoundedRect (1 << 1)
#define ShapeType_Circle (1 << 2)
#define ShapeType_Ellipse (1 << 3)
#define ShapeType_Rhombus (1 << 4)
#define ShapeType_Triangle (1 << 5)
#define ShapeType_RegularPolygon (1 << 6)
#define ShapeType_Text (1 << 7)
#define ShapeType_Sector (1 << 8)

#define ShapeType_RectLike (ShapeType_Rect | ShapeType_RoundedRect | ShapeType_Circle)

struct BorderData {
    fixed4 color;
    float size;
    float radius;
};
/*
BorderData GetBorderDataOld(float2 coords, float2 size, float4 packedBorderColors, float2 packedBorderSizes, float packedRadii) {
    float left = step(coords.x, 0.5); // 1 if left
    float bottom = step(coords.y, 0.5); // 1 if bottom
    
    #define top (1 - bottom)
    #define right (1 - left)  
    
    fixed4 borderColorTop = UnpackColor(asuint(packedBorderColors.x));
    fixed4 borderColorRight = UnpackColor(asuint(packedBorderColors.y));
    fixed4 borderColorBottom = UnpackColor(asuint(packedBorderColors.w));
    fixed4 borderColorLeft = UnpackColor(asuint(packedBorderColors.z));

    BorderData retn;

    uint packedRadiiUInt = asuint(packedRadii);
    
    float4 radii = float4(
        uint((packedRadiiUInt >>  0) & 0xff),
        uint((packedRadiiUInt >>  8) & 0xff),
        uint((packedRadiiUInt >> 16) & 0xff),
        uint((packedRadiiUInt >> 24) & 0xff)
    );
        
    float r = 0;
    r += (top * left) * radii.x;
    r += (top * right) * radii.y;
    r += (bottom * left) * radii.z;
    r += (bottom * right) * radii.w;
    retn.radius = (r * 2) / 1000;
    
    half2 topLeftBorderSize = UnpackSize(packedBorderSizes.x);
    half2 bottomRightBorderSize = UnpackSize(packedBorderSizes.y);
    
//    #define borderTop topLeftBorderSize.y
//    #define borderLeft topLeftBorderSize.x
//    #define borderBottom bottomRightBorderSize.y
//    #define borderRight bottomRightBorderSize.x
    
    if((top * left) != 0) {
        float x = coords.x * size.x;
        float y = (1 - coords.y) * size.y;
        retn.color = lerp(borderColorLeft, borderColorTop, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderTop, borderLeft, x < y);
        return retn;  
    }
    
    if((top * right) != 0) {
        float x = (1 - coords.x) * size.x;
        float y = (1 - coords.y) * size.y;
        
        if(borderTop == 0) {
            
        }
        else if(borderRight == 0) {
            retn.color = lerp(borderColorRight, borderColorTop, smoothstep(-0.01, 0.01, x - y));
            retn.size = lerp(borderTop, borderRight, x < y);
            return retn;  
        }
        
        retn.color = lerp(borderColorRight, borderColorTop, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderTop, borderRight, x < y);
        return retn;  
    }
    
    if((bottom * left) != 0) {
        float x = (coords.x) * size.x;
        float y = (coords.y) * size.y;
        retn.color = lerp(borderColorLeft, borderColorBottom, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderBottom, borderLeft, x < y);
        return retn;  
    }
    
    // bottom right case
    float x = (1 - coords.x) * size.x;
    float y = (coords.y) * size.y;
    retn.color = lerp(borderColorRight, borderColorBottom, smoothstep(-0.01, 0.01, x - y));
    retn.size = lerp(borderBottom, borderRight, x < y);
    return retn;      
}*/

// This code works sort of for mixed border sizes, sucks for rounded corners and the blend line is bad but it does work
 
inline half DistToLine(half2 pt1, half2 pt2, half2 testPt) {
  half2 lineDir = pt2 - pt1;
  half2 perpDir = half2(lineDir.y, -lineDir.x);
  half2 dirToPt1 = pt1 - testPt;
  return abs(dot(normalize(perpDir), dirToPt1));
}

BorderData GetBorderData(float2 coords, float2 size, float4 packedBorderColors, float2 packedBorderSizes, float packedRadii, fixed4 contentColor) {
    float left = step(coords.x, 0.5); // 1 if left
    float bottom = step(coords.y, 0.5); // 1 if bottom
    
    #define top (1 - bottom)
    #define right (1 - left)  
    
    fixed4 borderColorTop = UIForiaColorSpace(UnpackColor(asuint(packedBorderColors.x)));
    fixed4 borderColorRight = UIForiaColorSpace(UnpackColor(asuint(packedBorderColors.y)));
    fixed4 borderColorBottom = UIForiaColorSpace(UnpackColor(asuint(packedBorderColors.z)));
    fixed4 borderColorLeft = UIForiaColorSpace(UnpackColor(asuint(packedBorderColors.w)));
    BorderData retn;

    uint packedRadiiUInt = asuint(packedRadii);
    
    fixed4 radii = fixed4(
        uint((packedRadiiUInt >>  0) & 0xff),
        uint((packedRadiiUInt >>  8) & 0xff),
        uint((packedRadiiUInt >> 16) & 0xff),
        uint((packedRadiiUInt >> 24) & 0xff)
    );
        
    fixed r = 0;
    r += (top * left) * radii.x;
    r += (top * right) * radii.y;
    r += (bottom * left) * radii.z;
    r += (bottom * right) * radii.w;
    retn.radius = (r * 2) / 1000; // radius comes in as a byte representing 0 to 50 of our width, remap 0 - 250 to 0 - 0.5
    
    half2 topLeftBorderSize = UnpackSize(packedBorderSizes.x);
    half2 bottomRightBorderSize = UnpackSize(packedBorderSizes.y);
    
    #define borderTop topLeftBorderSize.y
    #define borderLeft topLeftBorderSize.x
    #define borderBottom bottomRightBorderSize.y
    #define borderRight bottomRightBorderSize.x
    
    half x = coords.x * size.x;
    half y = (1 - coords.y) * size.y;
    half2 p = half2(x, y);
    
    // find the corner nearest this pixel and compute an inset point from that corner 
    half2 corner = half2(lerp(0, size.x, right), lerp(0, size.y, bottom));
    half2 inset = half2(lerp(borderLeft, size.x - borderRight, right), lerp(borderTop, size.y - borderBottom, bottom));
    half dir = 1;

    if(left != 0) {
        dir = -1;
    }
    
    // equasion of a line, take the sign to determine if a point is above or below the line
    half v = (inset.x - corner.x) * (y - corner.y) - (inset.y - corner.y) * (x - corner.x);
    half d  = DistToLine(corner, inset, p);
    
    fixed sideOfLine = dir * sign(v);
    
    half verticalSize = lerp(borderTop, borderBottom, bottom);
    half horizontalSize = lerp(borderRight, borderLeft, left);
    
    half sizeAbove = borderTop;
    half sizeBelow = horizontalSize; 
    
    fixed4 horizontal = lerp(borderColorRight, borderColorLeft, left);
    fixed4 vertical = lerp(borderColorTop, borderColorBottom, bottom);
    
    fixed4 colorAbove = borderColorTop;
    fixed4 colorBelow = horizontal;
      
    if(bottom) {
        sizeAbove = sizeBelow;
        sizeBelow = borderBottom;
        colorAbove = colorBelow;
        colorBelow = borderColorBottom;
    }
              
    if(sideOfLine == 1) {
        retn.color = colorAbove; 
        retn.size = sizeAbove;
    }
    else {
        retn.color = colorBelow;
        retn.size = sizeBelow;
    }
              
    if(d < 1 && d > -1) {
        d =  Map(d, -1, 1, 0, 1);
        if(sideOfLine == -1) {
            d = 1 - d;
        }
        retn.color =  lerp(colorBelow, colorAbove, d);
    }
    
    float2 s = step(float2(borderLeft, size.y - borderBottom), p) - step(float2(size.x - borderRight, borderTop), p);
    
    float radialX = lerp(coords.x * size.x, (1 - coords.x) * size.x, right);
    float radialY = lerp(coords.y * size.y, (1 - coords.y) * size.y, top);
    
    if(retn.radius > 0) {
        retn.color = lerp(horizontal, vertical, smoothstep(-0.01, 0.01, radialX - radialY));
        retn.size = lerp(verticalSize, horizontalSize, radialX < radialY); // - 0.5;
    }
    else if(s.x * s.y) {
        retn.size = 0;
    }
    
    retn.color.rgb * retn.color.a;
    return retn;     
}

// this will give great AA on rotated edges but for cases where the sides are vertical or horizontal 
// it will cause a ~1 pixel blend that looks terrible when placed side by side with another object
// size is size of the quad, distFromCenter is 0 - 1 where 0 is an edge          
inline fixed4 MeshBorderAA(fixed4 mainColor, float2 size, float distFromCenter) {
     // this tries to find a 1 or 2 pixel border from edges
     float borderSize = 1 / (min(size.x, size.y)) * 2;// could also be 1.41 as sqrt2 for pixel size
     
     if(mainColor.a > 0 && distFromCenter < borderSize && distFromCenter == 1) {
        fixed4 retn = fixed4(mainColor.rgb, 0);
        retn = lerp(retn, mainColor, distFromCenter / borderSize);
        retn.rgb *= distFromCenter / (borderSize);
        return retn;
     }
     
     return mainColor;
}


            
inline fixed4 ComputeColor(float packedBg, float packedTint, int colorMode, float2 texCoord, sampler2D _MainTexture) {

    int useColor = (colorMode & PaintMode_Color) != 0;
    int useTexture = (colorMode & PaintMode_Texture) != 0;
    int tintTexture = (colorMode & PaintMode_TextureTint) != 0;
    int letterBoxTexture = (colorMode & PaintMode_LetterBoxTexture) != 0;

    fixed4 bgColor = UIForiaColorSpace(UnpackColor(asuint(packedBg)));
    fixed4 tintColor = UIForiaColorSpace(UnpackColor(asuint(packedTint)));
    fixed4 textureColor = tex2D(_MainTexture, texCoord);

    // bgColor.rgb *= bgColor.a;
    // tintColor.rgb *= tintColor.a;
    // textureColor.rgb *= textureColor.a;
    
    textureColor = lerp(textureColor, textureColor * tintColor, tintTexture);
    if (useTexture && letterBoxTexture && (texCoord.x < 0 || texCoord.x > 1) || (texCoord.y < 0 || texCoord.y > 1)) {
        return bgColor; // could return a letterbox color instead
    }
    
    if(useTexture && useColor) {
        return lerp(textureColor, bgColor, 1 - textureColor.a);
    }
                    
    return lerp(bgColor, textureColor, useTexture);
}

inline fixed4 GetTextColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
    half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
    half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));

    faceColor.rgb *= faceColor.a;
    outlineColor.rgb *= outlineColor.a;

    faceColor = lerp(faceColor, outlineColor, outlineAlpha);

    faceColor *= faceAlpha;

    return faceColor;
}

    
// sca is the sin/cos of the orientation
// scb is the sin/cos of the aperture
// ra = radius
// rb = width
float SDFArc(float2 p, float ta, float tb, float radius, float width) {
    float2 sca = float2(sin(ta), cos(ta));
    float2 scb = float2(sin(tb), cos(tb));
    p = mul(float2x2(sca.x, sca.y, -sca.y, sca.x), p);
    p.x = abs(p.x);
    float k = (scb.y * p.x > scb.x * p.y) ? dot(p.xy, scb) : length(p.xy);
    return sqrt(dot(p, p) + radius * radius - 2.0 * radius * k) - width;
}
            
float SDFCornerBevel(float2 uv, float2 size, float cutX, float cutY) {
    fixed hDir = lerp(-1, 1, uv.x > 0.5);
    fixed vDir = lerp(-1, 1, uv.y > 0.5);
    float halfX = size.x * 0.5;
    float halfY = size.y * 0.5;
    float2 center = ((uv.xy - 0.5) * size);
    float2 p0 = float2(hDir * (halfX - cutX), vDir * halfY);
    float2 p1 = float2(hDir * size.x, vDir * size.y); // big on purpose so we don't get bad bleeding of non clipped edge
    float2 p2 = float2(hDir * halfX, vDir * (halfY - cutY));
    return sdTriangle(center, p0, p1, p2);
}

float SDFShadow(SDFData sdfData, float intensity, float cut) {
    float halfStrokeWidth = sdfData.strokeWidth * 0.5;
    float2 size = sdfData.size;
    float minSize = min(size.x, size.y);
    float radius = clamp(minSize * sdfData.radius, 0, minSize);
    
    if(cut != 0) {
        radius = 0.2 * minSize;
    }
    
    float2 center = ((sdfData.uv.xy - 0.5) * size);
    
    float sdf = RectSDF(center, (size * 0.5) - halfStrokeWidth, radius - halfStrokeWidth);
    float tri = SDFCornerBevel(sdfData.uv, size, cut, cut);
    sdf = lerp(sdf, subtractSDF(sdf, tri), cut > 0);
    return sdf;
    // return abs(sdf) - halfStrokeWidth;
}

fixed4 SDFColor(SDFData sdfData, fixed4 borderColor, fixed4 contentColor, float cornerBevel) {
    float halfStrokeWidth = sdfData.strokeWidth * 0.5;
    
    float2 size = sdfData.size;
    float minSize = min(size.x, size.y);
    float radius = clamp(minSize * sdfData.radius, 0, minSize);
    
    float2 center = ((sdfData.uv.xy - 0.5) * size);
                       
    float cutX = cornerBevel;
    float cutY = cornerBevel;
    
    if(contentColor.a <= 0) {
        contentColor = fixed4(borderColor.rgb, 0);
    }
     
    if(halfStrokeWidth == 0 || borderColor.a <= 0) { // if has border but border alpha is 0 might need to handle that 
        halfStrokeWidth = lerp(3, 0, cutX + cutY > 0);
        borderColor = contentColor;
    }
      
    float sdf = RectSDF(center, (size * 0.5) - halfStrokeWidth, radius - halfStrokeWidth);
    float tri = SDFCornerBevel(sdfData.uv, size, cutX, cutY);
    sdf = lerp(sdf, subtractSDF(sdf, tri), cutX + cutY > 0);
    float retn = abs(sdf) - halfStrokeWidth;
    
    fixed4 innerColor = borderColor; 
    fixed4 outerColor = contentColor;
    
    if(sdf >= 0) {
        innerColor = borderColor;
        outerColor = fixed4(borderColor.rgb, 0);
    }
    
    float ddxRetn = ddx(retn);
    float ddyRetn = ddy(retn);

    float distanceChange = sqrt(ddxRetn * ddxRetn + ddyRetn * ddyRetn);
    float aa = 0;
    
    if(cutX + cutY > 0) {
        aa = 1 - smoothstep(-1, 1, sdf);
    }
    else {
        if(step(abs(ddxRetn) * abs(ddyRetn), 0))  {
            distanceChange *= 0.5;
        }
        else {
            distanceChange *= 0.71;
        }
        aa = smoothstep(distanceChange, -distanceChange, retn);
    }
    
    return lerp(innerColor, outerColor, 1 - aa); // do not pre-multiply alpha here!
}

    // using -1 to 1 gives slightly better aa but all edges have alpha which is bad when two shapes share an edge
    // use larger negative to get nice blur effect
    // smoothstep(-1, 0, fDist) gives the best aa but there is a gap between shapes that should touch
    // smoothstep(0, 1, fDist) fixes the gap perfectly but causes rounded shapes to be slightly cut off at the bottom and right edges
         
             // this is 1 pixel of border size
    //float borderSize = (1 / minSize) * 1.4;

    // with a border -1, 1 looks the best, without use 0, 1
          
    // below seems no longer needed but keeping just in case          
   // if(distFromCenter < halfStrokeWidth * 2 * borderSize) {  
    
        // this is for the clipped corner case, not currently in use 
      //  if(sdfData.radius == 0) {
      //      return borderColor;
      //  }
        
        //if(sdfData.radius > 0 && distFromCenter < borderSize) {
            // todo -- ask micha how to optimize this
          //  if((sdfData.uv.x > sdfData.radius * sdfData.uv.x < 1 - sdfData.radius) || (sdfData.uv.y > sdfData.radius * sdfData.uv.y < 1 - sdfData.radius)) {
                // return lerp(contentColor, borderColor, halfStrokeWidth != 0);
           // }
      //  }
      
#endif // UIFORIA_SDF_INCLUDE