Shader "Mobile/MobileTint" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Contrast("Contrast", float) = 1.0
	}
		SubShader{
			Tags { "RenderType" = "Opaque" }
			LOD 150

			CGPROGRAM
			#pragma surface surf Lambert noforwardadd

			sampler2D _MainTex;
			fixed4 _Color;
			float _Contrast;

			struct Input {
				float2 uv_MainTex;
			};

			void surf(Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
				o.Albedo = saturate(lerp(half3(0.5, 0.5, 0.5), c.rgb * _Color.rgb, _Contrast));
				o.Alpha = c.a * _Color.a;
			}
			ENDCG
		}

			Fallback "Mobile/VertexLit"
}