Shader "HighlightPlus/ClearStencil"
{
	Properties
	{
	}
	SubShader
	{
		Stencil {
			Ref 2
			Comp Always
			Pass zero
		}
		ZTest Always
		ZWrite Off
		Cull Off
		ColorMask 0

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};


			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = float4(v.vertex.xy, 0, 0.5);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG
		}
	}
}
