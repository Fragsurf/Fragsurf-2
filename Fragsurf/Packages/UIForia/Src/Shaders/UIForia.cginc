#define PI 3.141592653
#define PI2 6.283185307

#define red fixed4(1, 0, 0, 1)
#define green fixed4(0, 1, 0, 1)
#define blue fixed4(0, 0, 1, 1)
#define yellow fixed4(1, 1, 0, 1)

#define OrthoCameraWidth unity_OrthoParams.x
#define OrthoCameraHeight unity_OrthoParams.y

struct FillSettings {
    fixed4 fillColor1;
    fixed4 fillColor2;
    float fillRotation;
    float2 fillOffset;
    float2 fillScale;
    int gradientAxis;
    float gradientStart;
    float gridSize;
    float lineSize;
    sampler2D fillTexture;
};

// ***** 
// begin http://theorangeduck.com/page/avoiding-shader-conditionals
// *****

float4 when_eq(float4 x, float4 y) {
  return 1.0 - abs(sign(x - y));
}

float4 when_neq(float4 x, float4 y) {
  return abs(sign(x - y));
}

float4 when_gt(float4 x, float4 y) {
  return max(sign(x - y), 0.0);
}

float4 when_lt(float4 x, float4 y) {
  return max(sign(y - x), 0.0);
}

float4 when_ge(float4 x, float4 y) {
  return 1.0 - when_lt(x, y);
}

float4 when_le(float4 x, float4 y) {
  return 1.0 - when_gt(x, y);
}

float4 and(float4 a, float4 b) {
  return a * b;
}

float4 or(float4 a, float4 b) {
  return min(a + b, 1.0);
}

float4 xor(float4 a, float4 b) {
  return (a + b) % 2.0;
}

float4 not(float4 a) {
  return 1.0 - a;
}

// *****
// end http://theorangeduck.com/page/avoiding-shader-conditionals
// *****

// this seems a little nicer than smoothstep for antialiasing, though
// not necessarily for large amounts of blurring...there are tradeoffs,
// might want to separate blur from antialias
float blur(float edge1, float edge2, float amount) {
    return clamp(lerp(0, 1, (amount - edge1) / (edge2 - edge1)), 0, 1);
    // return clamp(pow(max(0, amount - edge1), 2) / pow(edge2 - edge1, 2), 0, 1);
    // return clamp((amount - edge1) / (edge2 - edge1), 0, 1);
    // amount = clamp(amount, edge1, edge2);
    // return pow((amount - edge1) / max(edge2 - edge1, 0.0001), 0.9);
    // amount = clamp((amount - edge1) / (edge2 - edge1), 0.0, 1.0);
    // return amount*amount*amount*(amount*(amount*6 - 15) + 10);
}

fixed4 fill_blend(float dist, fixed4 fillColor, float outerBlur) {
    float alpha = blur(0, outerBlur, dist);
    fixed4 color = fillColor;
    color.a *= alpha;
    return color;
}

fixed4 outline_fill_blend(float dist, fixed4 fillColor, fixed4 outlineColor, float outerBlur, float outline, float innerBlur) {
    float mix = blur(outline, innerBlur, dist);
    fixed4 color = lerp(outlineColor, fillColor, mix);
    float alpha = blur(0, 1, dist / outerBlur);
    color.a *= alpha;
    return color;
}

float2 rotate_fill(float2 fpos, float rotation) {
    float2 old_fpos = fpos;
    fpos.x = old_fpos.x * cos(rotation) - old_fpos.y * sin(rotation);
    fpos.y = old_fpos.x * sin(rotation) + old_fpos.y * cos(rotation);
    return fpos;
}

// returns the fill color for the given uv point in the quad.
// @param uv the uv coords from -0.5 to 0.5
fixed4 fill(float2 uv, float2 size, float4 contentRect, FillSettings settings) {
    float2 fpos = uv * size;

    #if UIFORIA_FILLTYPE_COLOR
        return settings.fillColor1;
   
    #elif UIFORIA_FILLTYPE_TEXTURE
        float2 p = (uv + 0.5) * size;
        
        if(p.x <= contentRect.x || p.x >= contentRect.x + contentRect.z || p.y <= contentRect.y || p.y >= contentRect.y + contentRect.w) {
            return settings.fillColor1;
        }
        float2 uv2 = float2(
            (p.x - contentRect.x) / contentRect.z, 
            (p.y - contentRect.y) / contentRect.w
        );
        uv2 = rotate_fill(uv2, settings.fillRotation);
        uv2 += settings.fillOffset;
        uv2 /= settings.fillScale;
        return tex2D(settings.fillTexture, uv2) * settings.fillColor1;
        
    #elif defined(UIFORIA_FILLTYPE_LINEAR_GRADIENT) | defined(UIFORIA_FILLTYPE_RADIAL_GRADIENT) | defined(UIFORIA_FILLTYPE_CYLINDRICAL_GRADIENT)
        
        fpos = rotate_fill(fpos, settings.fillRotation);
        fpos += settings.fillOffset;
        float gmin = 0;
        float gmax = 0;
        float current = 0;

        #if UIFORIA_FILLTYPE_LINEAR_GRADIENT
        
            if (settings.gradientAxis == 0) {
                gmin = -size.x * 0.5 + settings.gradientStart * size.x;
                gmax = size.x * 0.5;
                current = fpos.x;
            } else {
                gmin = -size.y * 0.5 + settings.gradientStart * size.y;
                gmax = size.y * 0.5;
                current = fpos.y;
            }
            
        #elif UIFORIA_FILLTYPE_CYLINDRICAL_GRADIENT
            
            if (settings.gradientAxis == 0) {
                gmin = settings.gradientStart * 0.5 * size.x;
                gmax = size.x * 0.5;
                current = abs(fpos.x);
            } else {
                gmin = settings.gradientStart * 0.5 * size.y;
                gmax = size.y * 0.5;
                current = abs(fpos.y);
            }
            
        #elif UIFORIA_FILLTYPE_RADIAL_GRADIENT
            
            gmax = length(float2(0.5, 0.5));
            gmin = gmax * settings.gradientStart;
            current = length(fpos);
            
        #endif
        
        if (current < gmin) {
            return settings.fillColor1;
        }
        
        if (gmax == gmin) {
            return settings.fillColor2;
        }
        
        return lerp(settings.fillColor1, settings.fillColor2, (current - gmin) / (gmax - gmin));
    
    #elif UIFORIA_FILLTYPE_GRID
        
        fpos = rotate_fill(fpos, settings.fillRotation);
        fpos += settings.fillOffset;
        float gridSize = settings.gridSize;
        float lineSize = settings.lineSize;
        float edge = min(2, gridSize);
        
        float px = abs(frac(fpos.x / gridSize) * gridSize * 2 - gridSize;
        float py = abs(frac(fpos.y / gridSize) * gridSize * 2 - gridSize;
        
        float mixx = smoothstep(gridSize - lineSize - edge, gridSize - lineSize, px);
        float mixy = smoothstep(gridSize - lineSize - edge, gridSize - lineSize, py);
    
        return lerp(settings.fillColor1, settings.fillColor2, max(mixx, mixy));
    
    #elif UIFORIA_FILLTYPE_CHECKER
        
        fpos = rotate_fill(fpos, settings.fillRotation);
        fpos += fillOffset;
        float edge = min(1, settings.gridSize);
        float2 p = frac(fpos / settings.gridSize);
        float2 mix = smoothstep(0, edge / settings.gridSize,p);
        float tile = abs(floor(fpos.y / settings.gridSize) + floor(fpos.x / settings.gridSize)) % 2;
        fixed4 color1 = tile * settings.fillColor1 + (1 - tile) * settings.fillColor2;
        fixed4 color2 = tile * settings.fillColor2 + (1 - tile) * settings.fillColor1;
        return lerp(color1, color2, min(mix.x, mix.y));
    
    #elif UIFORIA_FILLTYPE_STRIPES
    
        fpos = rotate_fill(fpos, settings.fillRotation);
        fpos += settings.fillOffset;
        float edge = min(2, settings.gridSize);
        float p = abs(frac(fpos.x / settings.gridSize) * settings.gridSize * 2 - settings.gridSize);
        float mix = smoothstep(settings.gridSize - settings.lineSize - edge, settings.gridSize - settings.lineSize, p);
        return lerp(settings.fillColor1, settings.fillColor2, mix); 
            
    #else
        return fixed4(0, 0, 0, 1);
           
    #endif
            
    
}
