Shader "Custom/ButtonShader" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard

		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		fixed4 _ActivatedColor;
		fixed4 _FocusColor;

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			o.Albedo = _Color;
			o.Emission = _ActivatedColor + _FocusColor;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
}
