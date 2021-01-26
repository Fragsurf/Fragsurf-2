
    // clip rect is in texture space with origin top left
    // texcoord origin is bottom left, invert to match
    float2 p = float2(IN.texcoord.x, 1 - IN.texcoord.y);
    float4 clipRect = m_ClipRect;

    clip((step(clipRect.xy, p) * step(p, clipRect.zw)) - 0.01);
    
    half blur = 1.414;
    half2 size = m_Size;
    half width = size.x;
    half height = size.y;
    half2 halfSize = size * 0.5;
    half2 texcoordMinusHalf = IN.texcoord - 0.5;

    half2 centerOffset = texcoordMinusHalf * size;
    
    FillSettings fillSettings;
    fillSettings.fillTexture = m_FillTexture;
    fillSettings.fillColor1 = m_FillColor;
    fillSettings.fillColor2 = m_FillColor2;
    fillSettings.fillRotation = m_FillRotation;
    fillSettings.fillOffset = m_FillOffset;
    fillSettings.fillScale = m_FillScale;
    fillSettings.gradientAxis = m_GradientAxis;
    fillSettings.gradientStart = m_GradientStart;
    fillSettings.gridSize = m_GridSize;
    fillSettings.lineSize = m_LineSize;
    
    fixed4 fillColor = fill(texcoordMinusHalf, size, m_ContentRect, fillSettings);
    
#if defined(UIFORIA_USE_BORDER)    
    int left = step(0.5, IN.texcoord.x);
    int top = step(0.5, IN.texcoord.y);
    
    int radiusIndex = 0;
    radiusIndex += and(top, left) * 1;
    radiusIndex += and(1 - top, 1 - left) * 2;
    radiusIndex += and(1 - top, left) * 3;
    
    half2 rectPos = p * size;
    half4 borderSize = m_BorderSize;

    int borderIndexer = radiusIndex; // todo -- make this work, dont use radius index;
    //borderIndexer += when_lt(rectPos.x, borderSize.w) * 1; 
    //borderIndexer += when_gt(rectPos.x, width - borderSize.y) * 2; 
    //borderIndexer += when_lt(rectPos.x, borderSize.w) * 3; 
    
    // subtract the AA blur from the outline so sizes stay mostly 
    // correct and outlines don't look too thick when scaled down.
    half outlineSize = max(0, m_BorderSize[borderIndexer] - blur);

    // some of these are constants, maybe move to constant buffer or vertex shader
    half halfMinDimension = min(halfSize.x, halfSize.y);
    half outerBlur = max(min(blur, halfMinDimension - outlineSize), 0);
    half innerBlur = max(min(outerBlur, halfMinDimension - outerBlur - outlineSize), 0);
                    
    half roundness = m_BorderRadius[radiusIndex];
    
    half radius = min(halfMinDimension, roundness);
    half2 extents = halfSize - radius;
    half2 delta = abs(centerOffset) - extents;
    
    // first component is distance to closest side when not in a corner circle,
    // second is distance to the rounded part when in a corner circle
    half dist = radius - (min(max(delta.x, delta.y), 0) + length(max(delta, 0)));
    half outline = outerBlur + outlineSize;

    // todo -- support roundness w/ padding 

    if(roundness == 0) {
        // borderSize == trbl
        half x = borderSize.w;              // right
        half y = borderSize.x;              // top
        half w = width - borderSize.y;      // left
        half h = height - borderSize.z;     // bottom
        
        if((rectPos.x < x || rectPos.x > w) || (rectPos.y < y || rectPos.y > h)) {
            fillColor = m_BorderColor;
        }
        else {
            outline = 0;
            innerBlur = 0;
            outerBlur = 0;
        }
    }
    
    innerBlur += outline;
    fixed4 color = outline_fill_blend(dist, fillColor, m_BorderColor, outerBlur, outline, innerBlur);
#else

    half2 delta = abs(centerOffset) - halfSize;
    half dist = -(min(max(delta.x, delta.y), 0) + length(max(delta, 0)));
    fixed4 color = fill_blend(dist, fillColor, 0 /*blur*/); //using blur here makes a 1px gap on the edges for some reason
    
#endif

    clip(color.a - 0.001);
    
    color.rgb *= color.a;
    return color;
