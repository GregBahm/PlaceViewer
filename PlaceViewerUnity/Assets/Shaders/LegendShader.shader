Shader "Custom/LegendShader" {
	Properties {
		_MainTex("Pixel Color", 2D) = "white" {}
		_Heat("Heat", 2D) = "white" {}
		_Longevity("Longevity", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _Heat;
		sampler2D _Longevity;

		float _HeatAlpha;
		float _LongevityAlpha;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			IN.uv_MainTex.x = 1 - IN.uv_MainTex.x * .99;
			fixed3 c = tex2D(_MainTex, IN.uv_MainTex).xyz;
			fixed3 heat = tex2D(_Heat, IN.uv_MainTex).xyz;
			fixed3 longevity = tex2D(_Longevity, IN.uv_MainTex).xyz;
			fixed3 ret = lerp(c, heat, _HeatAlpha);
			ret = lerp(ret, longevity, _LongevityAlpha);
			o.Albedo = ret; 
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Emission = ret;
		}
		ENDCG
	}
}
