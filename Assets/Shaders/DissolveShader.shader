// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/DissolveEffectShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Noise", 2D) = "white" {}
		_NoiseDistortTex("Noise Mask", 2D) = "white" {}
		_NoiseDistortScroll("Distort Scroll XY, Add Strength Z, Mul Strength W", Vector) = (1.0, 1.0, 1.0, 1.0)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_EdgeColour1("Edge colour 1", Color) = (1.0, 1.0, 1.0, 1.0)
		_EdgeColour2("Edge colour 2", Color) = (1.0, 1.0, 1.0, 1.0)
		_Level("Dissolution level", Range(0.0, 1.0)) = 0.1
		_Edges("Edge width", Range(0.0, 1.0)) = 0.1
		_TintDestroyedOriginTex("Tint Destroyed Origin Tex", Color) = (0.0, 0.0, 0.0, 1.0)
		_GlowColour("Glow colour", Color) = (1.0, 1.0, 1.0, 1.0)
		_GlowRange("Glow Range", Float) = 0.1

	}
		SubShader
		{
			Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 100

			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Off
				Lighting Off
				ZWrite Off
				Fog { Mode Off }

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			// make fog work
			#pragma multi_compile DUMMY PIXELSNAP_ON

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 noise_uv : TEXCOORD1;
				float2 distort_uv : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _NoiseTex;
			sampler2D _NoiseDistortTex;
			float4 _NoiseDistortScroll;
			float4 _EdgeColour1;
			float4 _EdgeColour2;
			float4 _TintDestroyedOriginTex;
			float _Level;
			float _Edges;
			float4 _MainTex_ST;
			float4 _NoiseTex_ST;
			float4 _NoiseDistortTex_ST;
			float4 _GlowColour;
			float _GlowRange;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.noise_uv = TRANSFORM_TEX(v.uv, _NoiseTex);
				o.distort_uv = TRANSFORM_TEX(v.uv, _NoiseDistortTex);

				#ifdef PIXELSNAP_ON
				o.vertex = UnityPixelSnap(o.vertex);
				#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				float cutout = tex2D(_NoiseTex, i.noise_uv).r;
				float distort = tex2D(_NoiseDistortTex, i.distort_uv + _NoiseDistortScroll.xy * _Time).r - 0.5;
				cutout = cutout + distort * _NoiseDistortScroll.z;

				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 tintOriginCol = col * _TintDestroyedOriginTex;

				float glow = clamp(1.0 - abs(cutout - _Level) / _GlowRange, 0.0, 1.0) + distort * _NoiseDistortScroll.w;
				glow = glow * glow * glow * col.a;

				/*if (cutout < _Level)
					discard;
				if (cutout < col.a && cutout < _Level + _Edges)
					col = lerp(_EdgeColour1, _EdgeColour2, (cutout - _Level) / _Edges);*/

				if (cutout < _Level) {
					col = tintOriginCol;
				}
				else 
				{
					if (cutout < col.a && cutout < _Level + _Edges)
						col = lerp(_EdgeColour1, _EdgeColour2, (cutout - _Level) / _Edges);
					col = lerp(tintOriginCol, col, col.a);
				}

				return col + glow * _GlowColour;
			}
			ENDCG
		}
		}
}