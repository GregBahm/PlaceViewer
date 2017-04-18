Shader "Unlit/DisplayShader"
{
	Properties
	{
		_Credits("Credits", 2D) = "white" {}
		_PixelIndexLut("Pixel Index Lut", 2D) = "white" {}
		_HeatLut("Heat Lut", 2D) = "white" {}
		_LongevityLut("Longevity Lyt", 2D) = "white" {}
		_HeatHeightMax("Heat Height", Float) = 200
		_LongevityHeightMax("Heat Height", Float) = 50
		_HeatHeightRamp("Heat Height Ramp", Range(0, 5)) = 1
		_LongevityHeightRamp("Longevity Height Ramp", Range(0, 5)) = 1
		_LongevityKey("Longevity Key", Range(0, 1)) = .5
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


			struct MeshData
			{
				float3 Position;
				float3 Normal;
			};

			struct v2f 
			{
				float4 vertex : SV_POSITION;
				float3 color : COLOR;
				float3 normal : NORMAL;
				float3 viewDir : TEXCOORD0;
			};

			float4x4 _Transformer;
			float3 _SceneScale;
			float3 _SceneOffset;

			StructuredBuffer<MeshData> _MeshBuffer;
			Buffer<float2> _GridBuffer;
			sampler2D _MainTex;
			sampler2D _Credits;
			sampler2D _PixelIndexLut;
			sampler2D _HeatLut;
			sampler2D _LongevityLut;

			float _ColorAlpha;
			float _HeatAlpha;
			float _LongevityAlpha;

			float _HeatHeightRamp;
			float _HeatHeightAlpha;
			float _HeatHeightMax;
			float _LongevityHeightRamp;
			float _LongevityHeightAlpha;
			float _LongevityHeightMax;

			float _LongevityKey;

			float3 GetColor(fixed4 sourceData, float normalY)
			{
				float4 colorLutUVs = float4(sourceData.x, 0, 0, 0);
				float3 baseColor = tex2Dlod(_PixelIndexLut, colorLutUVs).xyz;

				float heatLut = pow(sourceData.y * 10, .5);
				float4 heatLutUVs = float4(heatLut, 0, 0, 0);
				float3 heatColor = tex2Dlod(_HeatLut, heatLutUVs).xyz;

				float longevityLut = pow(1 - sourceData.z, 1);
				float4 longevityLutUVs = float4(longevityLut, 0, 0, 0);
				float3 longevityColor = tex2Dlod(_LongevityLut, longevityLutUVs).xyz;
				longevityColor *= normalY;

				return baseColor;// *_ColorAlpha + heatColor * _HeatAlpha + longevityColor * _LongevityAlpha;
			}

			float GetHeight(fixed4 sourceData)
			{
				float heatHeight = pow(sourceData.y, _HeatHeightRamp) * _HeatHeightMax;// *_HeatHeightAlpha;

				float rootLongevity = abs(sourceData.z - _LongevityKey);
				float longevityHeight = pow(rootLongevity, _LongevityHeightRamp) * _LongevityHeightAlpha * _LongevityHeightMax;

				return heatHeight + longevityHeight;
			}

			v2f vert(uint meshId : SV_VertexID, uint instanceId : SV_InstanceID)
			{
				v2f o;
				float2 gridPos = _GridBuffer[instanceId];
				MeshData meshData = _MeshBuffer[meshId];

				fixed4 sourceData = tex2Dlod(_MainTex, float4(gridPos, 0, 0)); // X is pixel index. Y is heat. Z is color longevity.
				float3 credits = tex2Dlod(_Credits, float4(1 - gridPos.x, gridPos.y, 0, 0)).xyz;
				float3 color = GetColor(sourceData, meshData.Normal.y);
				float3 flatPosition = float3(gridPos.x, 0, gridPos.y);

				float height = GetHeight(sourceData);

				float3 heatScale = float3(1, height + 1, 1); 
				float3 basePoint = flatPosition * 1000;
				float3 mesh = (meshData.Position + float3(0, .5, 0)) * heatScale;
				float4 combinedPoint = float4(basePoint + mesh, 1);
				combinedPoint = mul(_Transformer, combinedPoint);
				o.vertex = UnityObjectToClipPos(combinedPoint);
				o.normal = meshData.Normal;
				o.viewDir = UnityWorldSpaceViewDir(combinedPoint);
				o.color = lerp(credits, color, saturate(meshData.Normal.y + 1)) * (1 - sourceData.a);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(i.color, 1);
				float shine = dot(i.normal, normalize(normalize(i.viewDir))) / 2 + .5;
				shine = pow(shine, 10) * .6;
				float3 ret = i.color;
				//ret += shine;
				return float4(ret, 1);
			}
			ENDCG
		}
	}
}
