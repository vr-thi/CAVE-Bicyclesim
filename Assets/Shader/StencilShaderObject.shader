Shader "Custom/StencilShaderObject" {
	Properties {
		_Color ("Color", Color) = (0,0,0,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionColor("Emission Color", Color) = (0,0,0,1)
		_EmissionTex("Emission (RGB)", 2D) = "white" {}
		_EmissionStrength("Emission Strength", Range(1,4)) = 2
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		Ztest Always
		LOD 200

		Stencil{
			Ref 1
			Comp equal
		}
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex, _EmissionTex;

		struct Input {
			float2 uv_MainTex;
			float2 uv_EmissionTex;
		};

		fixed4 _Color, _EmissionColor;
		half _EmissionStrength;
		

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) *_Color;
			//o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables

			o.Alpha = c.a;

			//Emission
			fixed4 e = _EmissionColor;// *_EmissionStrength;
			//fixed4 lerpEmission = lerp(fixed4(0,0,0,0), e)
			o.Emission = e.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}