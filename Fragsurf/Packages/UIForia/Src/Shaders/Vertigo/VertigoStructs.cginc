#ifndef VERTIGO_STRUCT_INCLUDE
#define VERTIGO_STRUCT_INCLUDE

struct appdata {
    float4 vertex : POSITION;
    float4 texCoord0 : TEXCOORD0;
    float4 texCoord1 : TEXCOORD1;
    // float4 texCoord2 : TEXCOORD2;
    // float4 texCoord3 : TEXCOORD3;
    float4 color : COLOR;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float4 texCoord0 : TEXCOORD0;
    nointerpolation // is this needed?
    float4 texCoord1 : TEXCOORD1;
    // float4 texCoord2 : TEXCOORD2;
    // float4 texCoord3 : TEXCOORD3;
    float2 sdfCoord  : TEXCOORD4;
    float4 color : COLOR0;
};

struct SDFData {
    float2 uv;
    float2 size;
    float radius;
    float strokeWidth;
    int shapeType;
};


#endif 