Shader "Toon/LitWithEmmisiveThreshold" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[Header(Emission)]
		_GrayscaleWeight ("Grayscale Weight", Vector) = (0.21,0.71,0.07,0)
		_EmissiveThresholdMin ("Emmisive Min", float) = 0
		_EmissiveThresholdMax ("Emmisive Max", float) = 1
		[Header(Ramp)]
		_Ramp ("Toon Ramp (RGB)", 2D) = "gray" {} 
		_LightRatio ("Light Ratio", float) = 1
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
		float4 _GrayscaleWeight;
		half _LightRatio;
		half _EmissiveThresholdMin;
		half _EmissiveThresholdMax;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D(_MainTex, IN.uv_MainTex) * _LightRatio;
			float grayLevel = _GrayscaleWeight.r * c.r + _GrayscaleWeight.g * c.g + _GrayscaleWeight.b * c.b + _GrayscaleWeight.a;
			o.Albedo = c.rgb;
			o.Emission = c.rgb * c.a * smoothstep(_EmissiveThresholdMin, _EmissiveThresholdMax, grayLevel);
			o.Alpha = c.a;
		}
		ENDCG

	} 

	Fallback "Diffuse"
}