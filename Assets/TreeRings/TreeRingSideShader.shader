Shader "TreeRingSideShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Lut("Lut", 2D) = "white" {}
		_Flip("Flip", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			}; 

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float distShade : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _Lut;
			float _Flip;
			float _SideStart;
			float _SideEnd;
			float _Top;
			float _Bottom;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				
				o.distShade = v.uv.x - _Bottom;
				v.uv.y = lerp(_SideStart, _SideEnd, v.uv.y);
				v.uv.x = lerp(_Bottom, _Top, v.uv.x);
				float flippedY = (v.uv.y - .5) * -1 + .5;
				v.uv.y = lerp(v.uv.y, flippedY, _Flip);
				v.uv.y *= (float)1000 / 1024;
				o.uv = v.uv;
				return o;
			}

			fixed GetLutX(fixed4 sourceData, float param) 
			{
				int index = ((param * 1024) % 1) * 4;

				//int index = param * 4;
				bool xMultiplier = saturate(1 - index);
				bool yMultiplier = saturate(1 - abs(1 - index));
				bool zMultiplier = saturate(1 - abs(2 - index));
				bool wMultiplier = saturate(1 - abs(3 - index));
				return sourceData.x * xMultiplier
					+ sourceData.y * yMultiplier
					+ sourceData.z * zMultiplier
					+ sourceData.w * wMultiplier;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 sourceData = tex2D(_MainTex, i.uv);
				fixed lutX = GetLutX(sourceData, i.uv.x);
				fixed4 ret = tex2D(_Lut, float2(lutX, 0));
				float finalDistShade = saturate(i.distShade * 2 + 1) * .8;
				return ret * finalDistShade;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
