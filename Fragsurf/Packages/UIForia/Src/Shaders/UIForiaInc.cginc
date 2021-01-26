          
#ifndef __UIForiaInc__
#define __UIForiaInc__
               
#define FillMode_Color 0
#define FillMode_Texture (1 << 0)
#define FillMode_Gradient (1 << 1)
#define FillMode_Tint (1 << 2)
#define FillMode_GradientTint (FillMode_Tint | FillMode_Gradient)

#define ShapeType_Rect 1
#define ShapeType_RoundedRect 2
#define ShapeType_Path 3
#define ShapeType_Circle 4
#define ShapeType_Ellipse 5

#define StrokePlacement_Center 0
#define StrokePlacement_Inside 1
#define StrokePlacement_Outside 2

#define LineCap_Butt 0
#define LineCap_Square 1
#define LineCap_Round 2

#define RenderType_Fill 0
#define RenderType_Text 1
#define RenderType_Stroke 2
#define RenderType_StrokeShape 3
#define RenderType_Shadow 4

#define Red fixed4(1, 0, 0, 1)
#define Green fixed4(0, 1, 0, 1)
#define Blue fixed4(0, 0, 1, 1)
#define White fixed4(1, 1, 1, 1)
#define Black fixed4(0, 0, 0, 1)
#define Clear fixed4(1, 1, 1, 0)

// doesn't handle alpha, might need the input to use uint not int
inline float4 FloatToFixed4(float value) {
    uint input = asuint(value);
    return float4(
        uint((input >> 0) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        1       
    );
    
}


 // 0.5 is to target center of texel, otherwise we get bad neighbor blending
inline float GetPixelInRowUV(int targetY, float textureHeight) {
    return (targetY + 0.5) / textureHeight;
}

inline uint GetByte0(uint value) {
    return value & 0xff;
}

inline uint GetByte1(uint value) {
    return (value >> 8) & 0xff;
}

inline uint GetByte2(uint value) {
    return (value >> 16) & 0xff;
}

inline uint GetByte3(uint value) {
    return (value >> 24) & 0xff;
}

inline uint GetHighBits(uint value) {
    return (value >> 16) & (1 << 16) - 1;
}

inline uint GetLowBits(uint value) {
    return value & 0xffff;
}
           
inline float PercentageOfRange(float v, float bottom, float top) {
    return (v - bottom) / (top - bottom);
}

inline int and(int a, int b) {
    return a * b;
}
   
inline float when_ge(float x, float y) {
    return 1 - max(sign(y - x), 0.0);
}

inline float InsideBox(float2 v, float2 bottomLeft, float2 topRight) {
    float2 s = step(bottomLeft, v) - step(topRight, v);
    return s.x * s.y;   
}

// Rounded rect distance function
inline float udRoundRect(float2 p, float2 b, float r) {
   return length(max(abs(p) - b, 0)) - r;
}

float RectSDF(float2 p, float2 b, float r) {
   float2 d = abs(p) - b + float2(r, r);
   return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
}

float SmoothRect(float2 uv, float2 pos, float2 size, float sX, float sY) {
   float2 end = pos + size;
   return smoothstep(pos.x - sX, pos.x + sX, uv.x) 
       * (1.0 - smoothstep(end.x - sX, end.x + sX, uv.x))
       * smoothstep(pos.y - sY, pos.y + sY, uv.y)
       * (1.0 - smoothstep(end.y - sY, end.y + sY, uv.y));
}

// doesn't use a position value
float SmoothRect2(float2 uv, float2 size, float s) {
   float2 end = size;
   return smoothstep(-s, s, uv.x) 
       * (1.0 - smoothstep(end.x - s, end.x + s, uv.x))
       * smoothstep(-s, s, uv.y)
       * (1.0 - smoothstep(end.y - s, end.y + s, uv.y));
}

// https://thndl.com/going-round-in-squircles.html
float SquircleSDF(float2 uv, float2 size, float power) {

   float right = 1 - step(uv.x, 0.5); // 1 if left
   float bottom = step(uv.y, 0.5);
   
   // need to re-map into a different uv range or we only get 1 quadrant of squricle
   uv.x = lerp(1 - PercentageOfRange(uv.x, 0, 0.5), PercentageOfRange(uv.x, 0.5, 1),  right); 
   uv.y = lerp(1 - PercentageOfRange(uv.y, 0, 0.5), PercentageOfRange(uv.y, 0.5, 1), bottom);

   float2 powRar = pow(uv, float2(power, power));
   float s = (1 - length(powRar)) * size;
   // need to scale this into a value that makes sense, other sdf functions use pixel size and return values outside the [0, 1] range
   // s = smoothstep(0, 0.1, s); use this to get a solid shape, otherwise we get a blur
   return s;
}

float EllipseSDF( float2 p, float2 r ) {
    float k0 = length(p/r);
    float k1 = length(p/(r*r));
    return k0*(k0-1.0)/k1;
}

float GetStrokeWidth(float2 pixelCoord, float2 drawSurfaceSize, float4 widths) {
    float strokeWidth = 0;
    // left
    if(pixelCoord.x <= widths.w) {
        strokeWidth = widths.w;
    }
    // right
    else if(pixelCoord.x >= (drawSurfaceSize.x - widths.y)) {
        strokeWidth = widths.y;
    }
    // top
    else if(pixelCoord.y <= widths.x) {
        strokeWidth = widths.x;
    } 
    // bottom
    else if(pixelCoord.y >= (drawSurfaceSize.y - widths.z)) {
        strokeWidth = widths.z;
    }
    return strokeWidth;
}

#endif // __UIForiaInc__
// old circle and ellipse AA
//     float dist = length(i.uv.xy - 0.5);
//     float pwidth = length(float2(ddx(dist), ddy(dist)));
//     float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
//                         
//     color = fixed4(color.rgb, color.a * alpha);
                    