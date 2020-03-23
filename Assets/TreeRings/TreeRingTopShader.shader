Shader "Unlit/TreeRingTopShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Lut("Lut", 2D) = "white" {}
		_Spin("Spin", Float) = 0
		_ShadowRadius("ShadowRadius", Float) = 10
		_ShadowCurve("ShadowCurve", Float) = 2
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float2 baseUv : TEXCOORD1;
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _Lut;
			float2 _UvOffset;
			float2 _UvScale;
			float _Spin;
			float _ShadowX1;
			float _ShadowY1;
			float _ShadowX2;
			float _ShadowY2;
			float _ShadowRadius;
			float _ShadowCurve;
			float _ShadowAlpha;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.baseUv = float2(1 - v.uv.x, v.uv.y);
				o.vertex = UnityObjectToClipPos(v.vertex);
				v.uv.y = 1 - v.uv.y;
				o.uv = v.uv *_UvScale + _UvOffset;
				o.uv = lerp(o.uv, 1 - o.uv, _Spin);

				o.uv *= (float)1000 / 1024;
				return o;
			}

			float GetCorner(float2 baseUv, float cornerX, float cornerY)
			{
				float ret = length(baseUv - float2(cornerX, cornerY));
				ret = saturate(ret * _ShadowRadius);
				return 1 - ret;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float xDistA = _ShadowX1 - i.baseUv.x ;
				float yDistA = _ShadowY1 - i.baseUv.y;
				float xDistB = i.baseUv.x - _ShadowX2;
				float yDistB = i.baseUv.y - _ShadowY2;
				float cornerA = GetCorner(i.baseUv, _ShadowX1, _ShadowY1);
				float cornerB = GetCorner(i.baseUv, _ShadowX2, _ShadowY2);
				float cornerC = GetCorner(i.baseUv, _ShadowX1, _ShadowY2);
				float cornerD = GetCorner(i.baseUv, _ShadowX2, _ShadowY1);
				float corners = 1 - (cornerA + cornerB + cornerC + cornerD);
				//return corners;
				float xStripe = max(xDistA, xDistB);
				float yStripe = max(yDistA, yDistB);
				float xStripe2 = max(xStripe * _ShadowRadius, yStripe * 10000);
				float yStripe2 = max(yStripe * _ShadowRadius, xStripe * 10000);

				float shadow = min(min(yStripe2, xStripe2), corners);
				shadow = pow(shadow, _ShadowCurve);
				shadow = max(shadow, 1 - _ShadowAlpha);

				fixed lutVal = tex2D(_MainTex, i.uv).x;
				fixed4 col = tex2D(_Lut, float2(lutVal, 0));
				return col * shadow;
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
