#ifndef VERTIGO_SDF_INCLUDE
#define VERTIGO_SDF_INCLUDE

#include "./VertigoStructs.cginc"

const float SQRT_2 = 1.4142135623730951;

float RectSDF(float2 p, float2 size, float r) {
   float2 d = abs(p) - size + float2(r, r);
   return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
}

float EllipseSDF(float2 p, float2 r) {
    float k0 = length(p/r);
    float k1 = length(p/(r*r));
    return k0*(k0-1.0)/k1;
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

#define ShapeType_Rect (1 << 0)
#define ShapeType_RoundedRect (1 << 1)
#define ShapeType_Circle (1 << 2)
#define ShapeType_Ellipse (1 << 3)
#define ShapeType_Rhombus (1 << 4)
#define ShapeType_Triangle (1 << 5)
#define ShapeType_RegularPolygon (1 << 6)
#define ShapeType_Text (1 << 7)

#define ShapeType_RectLike (ShapeType_Rect | ShapeType_RoundedRect | ShapeType_Circle)

SDFData UnpackSDFData(float4 packedData, float2 coords) {
    
    float left = step(coords.x, 0.5); // 1 if left
    float bottom = step(coords.y, 0.5); // 1 if bottom
    float top = 1 - bottom;
    float right = 1 - left;
    
    float4 radii = UnpackSDFRadii(packedData.x);
    
    float r = 0;
    r += (top * left)* radii.x;
    r += (top * right) * radii.y;
    r += (bottom * left) * radii.z;
    r += (bottom * right) * radii.w;
    
    float radius = (r * 2) / 1000;
    float borderSize = 0;
    uint intSize = asuint(packedData.z);

    float width = ((intSize >> 16) & (1 << 16) - 1) / 10;
    float height = ( intSize & 0xffff) / 10;
    
    SDFData retn;
    retn.uv = coords.xy;
    retn.size = float2(width, height);
    retn.radius = radius;
    retn.strokeWidth = 0;
    retn.shapeType = asuint(packedData.y) & 0xff;
    
    return retn;
}
          
fixed4 SDFRectColor(SDFData sdfData, fixed4 color) {
    float halfStrokeWidth = sdfData.strokeWidth * 0.5;
    float minSize = min(sdfData.size.x, sdfData.size.y);
    float2 halfShapeSize = (sdfData.size * 0.5) - halfStrokeWidth;
    float radius = clamp(sdfData.size * sdfData.radius, 0, minSize);
    float2 center = (sdfData.uv.xy - 0.5) * sdfData.size;   
   
    float fDist = RectSDF(center, halfShapeSize, radius - halfStrokeWidth);
    float s = lerp(0, -1, radius / minSize);
    float e = lerp(1, 0, radius / minSize);
    float fBlendAmount = smoothstep(s, e, fDist);
    
    return lerp(color, fixed4(color.rgb, 0), fBlendAmount);
}
            
fixed4 SDFColor(SDFData sdfData, fixed4 color) {
    float fDist = 0;
    float halfStrokeWidth = 0;// sdfData.strokeWidth * 0.5;
    fixed4 strokeColor = fixed4(color.rgb, 0);
    fixed4 fillColor = fixed4(color.rgb, 1);
    float2 size = sdfData.size;
    float minSize = min(size.x, size.y);
    float2 halfShapeSize = (size * 0.5);
    float radius = clamp(minSize * sdfData.radius, 0, minSize);
    float2 center = (sdfData.uv.xy - 0.5) * size;   
    float fBlendAmount = 0;

        
    float2 sizeForStroke = halfShapeSize - halfStrokeWidth;
    minSize = min(sizeForStroke.x, sizeForStroke.y) * 2;
    radius = clamp(minSize * sdfData.radius, 0, minSize);
    float shape1 = RectSDF(center, halfShapeSize, radius);
    float shape2 = RectSDF(center, sizeForStroke, radius);
    float retn = lerp(shape1, max(shape1, -shape2), halfStrokeWidth > 0);

    fBlendAmount = smoothstep(-1, 1, retn);

        
    //}
    /*
    if(sdfData.shapeType == ShapeType_Ellipse) {
        fDist = EllipseSDF(center - float2(0, 0.5), halfShapeSize);
        fBlendAmount = smoothstep(-1, 1, fDist);
    }
    
    if(sdfData.shapeType == ShapeType_Triangle) {
        fDist = abs(TriangleSDF(sdfData.uv * size, float2(0, 0) * size, float2(0.6, 1) * size, float2(1, 0.8) * size)) - 1;
    }
    
    if(sdfData.shapeType == ShapeType_Rhombus) {
        fDist = RhombusSDF(center, halfShapeSize - halfStrokeWidth);
        // for stroke try this:
        //float distanceChange = fwidth(fDist);
        //fBlendAmount = smoothstep(-distanceChange, distanceChange, fDist);
        fBlendAmount = smoothstep(-1, 1, fDist);
    }
    */
   /* halfStrokeWidth = 10;
    fDist = RhombusSDF(center * (size + halfStrokeWidth * 0.5), halfShapeSize - halfStrokeWidth);
    float innerDist = abs(RhombusSDF(center * (size + halfStrokeWidth * 0.5), halfShapeSize - halfStrokeWidth)) - halfStrokeWidth;
    
    halfStrokeWidth = 0;
    float shape1 = RhombusSDF(center * (size + halfStrokeWidth * 0.5), halfShapeSize - halfStrokeWidth);;
    halfStrokeWidth = 0;
    
    float shape2 = RhombusSDF(center * (size + halfStrokeWidth * 0.5), halfShapeSize - halfStrokeWidth);;
    float retn = max(shape1, -shape2); // gives a hard outline
    */
    //float distanceChange = fwidth(fDist);
    //float fBlendAmount = smoothstep(distanceChange, -distanceChange, fDist);
    
    
    //float fBlendAmount = smoothstep(-1, 1, 1 - retn);
    
    //using -1 to 1 gives slightly better aa but all edges have alpha which is bad when two shapes share an edge
    // use larger negative to get nice blur effect
    // smoothstep(-1, 0, fDist) gives the best aa but there is a gap between shapes that should touch
    // smoothstep(0, 1, fDist) fixes the gap perfectly but causes rounded shapes to be slightly cut off at the bottom and right edges
    return lerp(strokeColor, fillColor, 1 - fBlendAmount); // do not pre-multiply alpha here!
}
         
#endif // VERTIGO_SDF_INCLUDE