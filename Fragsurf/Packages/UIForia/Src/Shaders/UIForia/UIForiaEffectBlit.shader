Shader "UIForia/EffectBlit"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Back
		Lighting Off
		ZWrite Off
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "Default"

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			
			float4 _TargetArea;
			
            struct a2v {
                float4 vertex   : POSITION;
	            float4 color    : COLOR;
	            float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 vertex   : SV_POSITION;
	            fixed4 color    : COLOR;
	            half2 texcoord  : TEXCOORD0;
             	float4 worldPosition : TEXCOORD1;
            };
            
            v2f vert(a2v IN) {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
	            OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
	            OUT.texcoord = IN.texcoord;
	            return OUT;
            }
                        
           
            
			fixed4 frag(v2f IN) : SV_Target {
			    fixed4 color = fixed4(1, 0, 0, 1);
			    return color;
			}
			
		ENDCG
		}
	}
}