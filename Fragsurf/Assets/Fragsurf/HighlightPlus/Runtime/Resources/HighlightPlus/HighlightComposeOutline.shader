Shader "HighlightPlus/Geometry/ComposeOutline" {
Properties {
    _MainTex ("Texture", Any) = "black" {}
	_Color("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
	_Cull("Cull Mode", Int) = 2
	_ZTest("ZTest Mode", Int) = 0
	_Flip("Flip", Vector) = (0, 1, 0)
	_Debug("Debug Color", Color) = (0,0,0,0)
}
SubShader
	{
		Tags { "Queue" = "Transparent+120" "RenderType" = "Transparent" "DisableBatching" = "True" }
		Blend SrcAlpha OneMinusSrcAlpha

		// Compose effect on camera target (optimal quad blit)
		Pass
		{
			ZWrite Off
			ZTest [_ZTest]
			Cull Off // [_Cull]
			Stencil {
				Ref 2
				Comp NotEqual
				Pass keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

            sampler2D _HPComposeOutlineFinal;
	
            fixed4 _Color;
			float3 _Flip;
			fixed4 _Debug;

            struct appdata
            {
                float4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos: SV_POSITION;
				float4 scrPos: TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.scrPos = ComputeScreenPos(o.pos);
				o.scrPos.y = o.scrPos.w * _Flip.x + o.scrPos.y * _Flip.y;
				return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
				fixed4 outline = tex2Dproj(_HPComposeOutlineFinal, i.scrPos);
            	fixed4 color = _Color;
            	color.a *= outline.r;
				color += _Debug;
            	color.a = saturate(color.a);
            	return color;
			}
				ENDCG
	}

		// Compose effect on camera target (full-screen blit)
		Pass
			{
				ZWrite Off
				ZTest Always //[_ZTest]
				Cull Off // [_Cull]
				Stencil {
					Ref 2
					Comp NotEqual
					Pass keep
				}


				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
				float4 _MainTex_ST;
				fixed4 _Color;
				float3 _Flip;

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv     : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f
				{
					float4 pos: SV_POSITION;
					float2 uv     : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
				};

				v2f vert(appdata v)
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_OUTPUT(v2f, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);
					o.uv.y = _Flip.x + o.uv.y * _Flip.y;
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					//UNITY_SETUP_INSTANCE_ID(i);
					//UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // Commandbuffers do not support Single Pass Instanced so we have to disable this
					fixed4 outline = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv);
					fixed4 color = _Color;
					color.a *= outline.r;
                    color = saturate(color);
					return color;
				}
				ENDCG
			}

    }
}