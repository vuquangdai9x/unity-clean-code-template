Shader "Toon/LitWithEmmisiveOutline" {
	Properties {
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Base (RGB)", 2D) = "white" { }
		_EmissiveTex("Emmisive", 2D) = "white" { }
		_Ramp("Toon Ramp (RGB)", 2D) = "gray" { }
		_LightRatio("Light Ratio", float) = 1
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.1)) = .005
	}

	SubShader {
		Pass 
		{
			Name "TOON"
			Tags {
				"LightMode" = "ForwardBase"
				"PassFlags" = "OnlyDirectional"
			}

			ZWrite On
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
			};

			struct v2f 
			{
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
				float3 worldNormal : NORMAL;
				float3 viewDir : TEXCOORD1;
			};
	
			sampler2D _MainTex;
			sampler2D _EmissiveTex;
			sampler2D _Ramp;
			float4 _Color;
			half _LightRatio;
			uniform fixed4 _GAmbientColor;

			v2f vert(appdata IN) 
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.worldNormal = UnityObjectToWorldNormal(IN.normal);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
				OUT.viewDir = WorldSpaceViewDir(IN.vertex);
				return OUT;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				float3 normal = normalize(IN.worldNormal);
				fixed4 color = tex2D(_MainTex, IN.texcoord);
				fixed4 emitColor = tex2D(_EmissiveTex, IN.texcoord);

				float NdotL = dot(_WorldSpaceLightPos0, normal);
				half d = NdotL * 0.5 + 0.5;
				fixed3 lightIntensity = tex2D(_Ramp, float2(d, d)).rgb * _LightRatio;

				//float lightIntensity = smoothstep(0, 0.01, NdotL);

				//return color * smoothstep(0, 0.01, NdotL);
				return float4(color.rgb * (_GAmbientColor.rgb + lightIntensity * _LightColor0.rgb) + emitColor.rgb * emitColor.a, color.a);
			}
			ENDCG
		}

		Pass {
			Name "OUTLINE"
			//Tags { "RenderType" = "Opaque" }
			Cull Front
			//ZWrite On
			//ColorMask RGB
			//Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata 
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};
	
			float _Outline;
			float4 _OutlineColor;
	
			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);

				float3 norm   = normalize(mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal));

				float2 offset = TransformViewToProjection(norm.xy);
				//#ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE //to handle recent standard asset package on older version of unity (before 5.5)
				//	o.pos.xy += offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z) * _Outline;
				//#else
				//	o.pos.xy += offset * o.pos.z * _Outline;
				//#endif

				o.pos.xy += offset * _Outline;

				o.color = _OutlineColor;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}

	} 

	Fallback "Diffuse"
}