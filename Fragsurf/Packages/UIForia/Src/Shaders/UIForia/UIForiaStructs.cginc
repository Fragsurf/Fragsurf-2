#ifndef UIFORIA_STRUCT_INCLUDE
#define UIFORIA_STRUCT_INCLUDE

struct appdata {
    float4 vertex : POSITION;
    float4 texCoord0 : TEXCOORD0;
    float4 texCoord1 : TEXCOORD1;
};

#define Red fixed4(1, 0, 0, 1)
#define Green fixed4(0, 1, 0, 1)
#define Blue fixed4(0, 0, 1, 1)
#define White fixed4(1, 1, 1, 1)
#define Black fixed4(0, 0, 0, 1)
#define Yellow fixed4(1, 1, 0, 1)

struct v2f {
    
    float4 vertex : SV_POSITION;
    float4 texCoord0 : TEXCOORD0;    
    float4 texCoord4 : TEXCOORD4;
    nointerpolation float4 texCoord1 : TEXCOORD1;
    nointerpolation float4 texCoord2 : TEXCOORD2;
    nointerpolation float4 texCoord3 : TEXCOORD3;
    nointerpolation float4 color : COLOR0;      // could probably also be array look up
    
};

struct SDFData {
    float2 uv;
    float2 size;
    float radius;
    float strokeWidth;
};


#endif 