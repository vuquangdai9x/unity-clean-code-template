Shader "Toon/LitWithRim" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
		_LightRatio ("Light Ratio", float) = 1
		
		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimAmount("Rim Amount", Range(0, 1)) = 0.716
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf ToonRamp

		sampler2D _Ramp;

		// custom lighting function that uses a texture ramp based
		// on angle between light direction and normal
		#pragma lighting ToonRamp exclude_path:prepass
		inline half4 LightingToonRamp (SurfaceOutput s, half3 lightDir, half atten)
		{
			#ifndef USING_DIRECTIONAL_LIGHT
			lightDir = normalize(lightDir);
			#endif
	
			half d = dot (s.Normal, lightDir)*0.5 + 0.5;
			half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
	
			half4 c;
			c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
			c.a = 0;
			return c;
		}


		sampler2D _MainTex;
		float4 _Color;
		half _LightRatio;

		float4 _RimColor;
		float _RimAmount;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float3 viewDir;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color * _LightRatio;

			//// Calculate rim lighting.
			//float NdotL = dot(_WorldSpaceLightPos0, o.Normal);
			//float rimDot = 1 - dot(IN.viewDir, o.Normal);
			//// We only want rim to appear on the lit side of the surface,
			//// so multiply it by NdotL, raised to a power to smoothly blend it.
			//float rimIntensity = rimDot * pow(NdotL, _RimThreshold);
			//rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimIntensity);
			//float4 rim = rimIntensity * _RimColor;

			half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			rim = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rim);
			o.Emission = _RimColor.rgb * rim;

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	Fallback "Diffuse"
}
