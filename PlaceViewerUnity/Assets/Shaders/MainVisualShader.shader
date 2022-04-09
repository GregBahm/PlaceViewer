Shader "MainVisualShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_PixelIndexLut("Pixel Index Lut", 2D) = "white" {}
		_HeatLut("Heat Lut", 2D) = "white" {}
		_LongevityLut("Longevity Lyt", 2D) = "white" {}
		_MainLightColor("Main Light Color", Color) = (1,1,1,1)
		_BackLightColor("Back Light Color", Color) = (0,0,0,0)
		_ShineColor("Shine Color", Color) = (1,1,1,1)
		_ShinePower("Shine Power", Float) = 1
		_HeatHeightMax("Height Scale", Float) = .3
		_LongevityHeightMax("Longevity Scale", Float) = .3
		_HeatHeightRamp("Heat Height Ramp", Range(0, 5)) = 1
		_LongevityHeightRamp("Longevity Height Ramp", Range(0, 5)) = 1
		_HeatBounceScale("Heat Bounce Scale", Float) = 100
		_LongevityBounceScale("Longevity Bounce Scale", Float) = 100
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geo
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			struct g2f
			{
				float3 color : COLOR;
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float3 worldPos : TEXCOORD1;
				float distFromBounce : TEXCOORD2;
				float3 bounceLightColor :TEXCOORD3;
				float2 uv : TEXCOORD4;
			};

			struct sideQuadVert
			{
				float3 pos : TEXCOORD0;
				float distFromBounce : TEXCOORD1;
				float3 bounceLightColor :TEXCOORD2;
				float2 uv : TEXCOORD3;
			};

			sampler2D _MainTex;
			sampler2D _PixelIndexLut;
			sampler2D _HeatLut;
			sampler2D _LongevityLut;

			float _HeatAlpha;
			float _LongevityAlpha;

			float3 _MainLightColor;
			float3 _BackLightColor;
			float3 _ShineColor;
			float _ShinePower;
			float3 _LightPos;
			float _HeatBounceScale;
			float _LongevityBounceScale;

			float _HeatHeightRamp;
			float _HeatHeightAlpha;
			float _HeatHeightMax;
			float _LongevityHeightRamp;
			float _LongevityHeightAlpha;
			float _LongevityHeightMax;

			float GetHeight(fixed4 sourceData)
			{
				float heatHeight = pow(sourceData.y, _HeatHeightRamp) * _HeatHeightMax *_HeatHeightAlpha;

				float rootLongevity = 1 - sourceData.z;
				float longevityHeight = pow(rootLongevity, _LongevityHeightRamp) * _LongevityHeightAlpha * _LongevityHeightMax;

				return heatHeight + longevityHeight;
			}

			float3 GetColor(fixed4 sourceData)
			{
				float shadow = 1 - sourceData.a;
				shadow = lerp(1, shadow, _HeatHeightAlpha);
				float4 colorLutUVs = float4(sourceData.x, 0, 0, 0);
				float3 baseColor = tex2Dlod(_PixelIndexLut, colorLutUVs).xyz;
				baseColor = pow(baseColor * shadow, 2);

				float heatLut = pow(sourceData.y * 10, .5);
				float4 heatLutUVs = float4(heatLut + sourceData.a, 0, 0, 0);
				float3 heatColor = tex2Dlod(_HeatLut, heatLutUVs).xyz;
				heatColor += (baseColor.x + baseColor.y + baseColor.z).xxx / 3 * .1;

				float longevityLut = sourceData.z;
				float4 longevityLutUVs = float4(longevityLut, 0, 0, 0);
				float3 longevityColor = tex2Dlod(_LongevityLut, longevityLutUVs).xyz;
				longevityColor *= shadow;

				float3 ret = lerp(baseColor, heatColor, _HeatAlpha);
				ret = lerp(ret, longevityColor, _LongevityAlpha);
				
				return ret;
			}

			v2g vert (appdata v)
			{
				v2g o;
				v.uv.x = 1 - v.uv.x;
				v.uv *= (float)1000 / 1024;
				o.uv = v.uv;
				o.vertex = v.vertex;
				return o;
			}

			void DrawQuad(
				sideQuadVert cornerA, 
				sideQuadVert cornerB, 
				sideQuadVert cornerC, 
				sideQuadVert cornerD, 
				g2f o,
				inout TriangleStream<g2f> triStream)
			{
				triStream.RestartStrip();

				o.uv = cornerA.uv;
				o.distFromBounce = cornerA.distFromBounce;
				o.bounceLightColor = cornerA.bounceLightColor;
				o.worldPos = mul(unity_ObjectToWorld, cornerA.pos);
				o.vertex = UnityObjectToClipPos(cornerA.pos);
				triStream.Append(o);

				o.uv = cornerB.uv;
				o.distFromBounce = cornerB.distFromBounce;
				o.bounceLightColor = cornerB.bounceLightColor;
				o.worldPos = mul(unity_ObjectToWorld, cornerB.pos);
				o.vertex = UnityObjectToClipPos(cornerB.pos);
				triStream.Append(o);

				o.uv = cornerC.uv;
				o.distFromBounce = cornerC.distFromBounce;
				o.bounceLightColor = cornerC.bounceLightColor;
				o.worldPos = mul(unity_ObjectToWorld, cornerC.pos);
				o.vertex = UnityObjectToClipPos(cornerC.pos);
				triStream.Append(o);

				o.uv = cornerD.uv;
				o.distFromBounce = cornerD.distFromBounce;
				o.bounceLightColor = cornerD.bounceLightColor;
				o.worldPos = mul(unity_ObjectToWorld, cornerD.pos);
				o.vertex = UnityObjectToClipPos(cornerD.pos);
				triStream.Append(o);
			}

			void DrawHorizontal(float2 uvs, 
				fixed currentHeight, 
				float3 p0vertex,
				float3 p1vertex,
				float3 p2vertex,
				g2f o,
				inout TriangleStream<g2f> triStream)
			{
				const float halfPixelOffset = ((float)1000 / 1024) / 1000 / 2;
				float2 left = float2(uvs.x - halfPixelOffset, uvs.y);
				float2 right = float2(uvs.x + halfPixelOffset, uvs.y);
				fixed4 leftPixel = tex2Dlod(_MainTex, float4(left, 0, 0));
				fixed leftHeight = GetHeight(leftPixel);
				fixed4 rightPixel = tex2Dlod(_MainTex, float4(right, 0, 0));
				fixed rightHeight = GetHeight(rightPixel);

				if (leftHeight < currentHeight)
				{
					float xDimension = min(p0vertex.x, min(p1vertex.x, p2vertex.x));
					float upperZ = max(p0vertex.z, max(p1vertex.z, p2vertex.z));
					float lowerZ = min(p0vertex.z, min(p1vertex.z, p2vertex.z));

					fixed3 bounceLightColor = GetColor(leftPixel);

					sideQuadVert cornerA;
					cornerA.uv = left;
					cornerA.bounceLightColor = bounceLightColor;
					cornerA.distFromBounce = currentHeight - leftHeight;
					cornerA.pos = float3(xDimension, currentHeight  * _HeatHeightMax, upperZ);

					sideQuadVert cornerB;
					cornerB.uv = left;
					cornerB.bounceLightColor = bounceLightColor;
					cornerB.distFromBounce = currentHeight - leftHeight;
					cornerB.pos = float3(xDimension, currentHeight * _HeatHeightMax, lowerZ);
					
					sideQuadVert cornerC;
					cornerC.uv = left;
					cornerC.bounceLightColor = bounceLightColor;
					cornerC.distFromBounce = 0;
					cornerC.pos = float3(xDimension, leftHeight * _HeatHeightMax, upperZ);
					
					sideQuadVert cornerD;
					cornerD.uv = left;
					cornerD.bounceLightColor = bounceLightColor;
					cornerD.distFromBounce = 0;
					cornerD.pos = float3(xDimension, leftHeight * _HeatHeightMax, lowerZ);

					o.normal = float3(-1, 0, 0);
					DrawQuad(cornerA, cornerB, cornerC, cornerD, o, triStream);
				}
				if (rightHeight < currentHeight)
				{
					float xDimension = max(p0vertex.x, max(p1vertex.x, p2vertex.x));
					float upperZ = max(p0vertex.z, max(p1vertex.z, p2vertex.z));
					float lowerZ = min(p0vertex.z, min(p1vertex.z, p2vertex.z));

					fixed3 bounceLightColor = GetColor(rightPixel);
				
					sideQuadVert cornerA;
					cornerA.uv = right;
					cornerA.bounceLightColor = bounceLightColor;
					cornerA.distFromBounce = currentHeight - rightHeight;
					cornerA.pos = float3(xDimension, currentHeight  * _HeatHeightMax, upperZ);
					
					sideQuadVert cornerB;
					cornerB.uv = right;
					cornerB.bounceLightColor = bounceLightColor;
					cornerB.distFromBounce = currentHeight - rightHeight;
					cornerB.pos = float3(xDimension, currentHeight * _HeatHeightMax, lowerZ);
					
					sideQuadVert cornerC;
					cornerC.uv = right;
					cornerC.bounceLightColor = bounceLightColor;
					cornerC.distFromBounce = 0;
					cornerC.pos = float3(xDimension, rightHeight * _HeatHeightMax, upperZ);
					
					sideQuadVert cornerD;
					cornerD.uv = right;
					cornerD.bounceLightColor = bounceLightColor;
					cornerD.distFromBounce = 0;
					cornerD.pos = float3(xDimension, rightHeight * _HeatHeightMax, lowerZ);
					
					o.normal = float3(1, 0, 0);
					DrawQuad(cornerA, cornerC, cornerB, cornerD, o, triStream);
				}
			}

			void DrawVertical(float2 uvs,
				fixed currentHeight,
				float3 p0vertex,
				float3 p1vertex,
				float3 p2vertex,
				g2f o,
				inout TriangleStream<g2f> triStream)
			{
				const float halfPixelOffset = ((float)1000 / 1024) / 1000 / 2;
				float2 north = float2(uvs.x , uvs.y + halfPixelOffset);
				float2 south = float2(uvs.x, uvs.y - halfPixelOffset);
				fixed4 northPixel = tex2Dlod(_MainTex, float4(north, 0, 0));
				fixed northHeight = GetHeight(northPixel);
				fixed4 southPixel = tex2Dlod(_MainTex, float4(south, 0, 0));
				fixed southHeight = GetHeight(southPixel);

				if (northHeight < currentHeight)
				{
					float zDimension = min(p0vertex.z, min(p1vertex.z, p2vertex.z));
					float upperX = max(p0vertex.x, max(p1vertex.x, p2vertex.x));
					float lowerX = min(p0vertex.x, min(p1vertex.x, p2vertex.x));

					fixed3 bounceLightColor = GetColor(northPixel);

					sideQuadVert cornerA;
					cornerA.uv = north;
					cornerA.bounceLightColor = bounceLightColor;
					cornerA.distFromBounce = currentHeight - northHeight;
					cornerA.pos = float3(upperX, currentHeight  * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerB;
					cornerB.uv = north;
					cornerB.bounceLightColor = bounceLightColor;
					cornerB.distFromBounce = currentHeight - northHeight;
					cornerB.pos = float3(lowerX, currentHeight * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerC;
					cornerC.uv = north;
					cornerC.bounceLightColor = bounceLightColor;
					cornerC.distFromBounce = 0;
					cornerC.pos = float3(upperX, northHeight * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerD;
					cornerD.uv = north;
					cornerD.bounceLightColor = bounceLightColor;
					cornerD.distFromBounce = 0;
					cornerD.pos = float3(lowerX, northHeight * _HeatHeightMax, zDimension);

					o.normal = float3(0, 0, -1);
					DrawQuad(cornerB, cornerA, cornerD, cornerC, o, triStream);
				}
				if (southHeight < currentHeight)
				{
					float zDimension = max(p0vertex.z, max(p1vertex.z, p2vertex.z));
					float upperX = max(p0vertex.x, max(p1vertex.x, p2vertex.x));
					float lowerX = min(p0vertex.x, min(p1vertex.x, p2vertex.x));

					fixed3 bounceLightColor = GetColor(southPixel);
				
					sideQuadVert cornerA;
					cornerA.uv = south;
					cornerA.bounceLightColor = bounceLightColor;
					cornerA.distFromBounce = currentHeight - southHeight;
					cornerA.pos = float3(upperX, currentHeight  * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerB;
					cornerB.uv = south;
					cornerB.bounceLightColor = bounceLightColor;
					cornerB.distFromBounce = currentHeight - southHeight;
					cornerB.pos = float3(lowerX, currentHeight * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerC;
					cornerC.uv = south;
					cornerC.bounceLightColor = bounceLightColor;
					cornerC.distFromBounce = 0;
					cornerC.pos = float3(upperX, southHeight * _HeatHeightMax, zDimension);
					
					sideQuadVert cornerD;
					cornerD.uv = south;
					cornerD.bounceLightColor = bounceLightColor;
					cornerD.distFromBounce = 0;
					cornerD.pos = float3(lowerX, southHeight * _HeatHeightMax, zDimension);
					
					o.normal = float3(0, 0, 1);
					DrawQuad(cornerC, cornerA, cornerD, cornerB, o, triStream);
				}
			}

			[maxvertexcount(11)]
			void geo(triangle v2g p[3], inout TriangleStream<g2f> triStream)
			{
				g2f o;
				o.distFromBounce = 1;
				o.bounceLightColor = 0;

				float2 uvs = (p[0].uv + p[1].uv + p[2].uv) / 3;
				fixed4 sourceData = tex2Dlod(_MainTex, float4(uvs, 0, 0));
				float3 color = GetColor(sourceData);
				o.color = color;

				fixed currentHeight = GetHeight(sourceData);

				float3 p0vertex = p[0].vertex + float3(0, currentHeight * _HeatHeightMax, 0);
				float3 p1vertex = p[1].vertex + float3(0, currentHeight * _HeatHeightMax, 0);
				float3 p2vertex = p[2].vertex + float3(0, currentHeight * _HeatHeightMax, 0);


				float3 baseNormal = mul(unity_ObjectToWorld, float3(0,1,0));
				o.normal = baseNormal;

				o.uv = p[0].uv;
				o.worldPos = mul(unity_ObjectToWorld, p[0].vertex);
				o.vertex = UnityObjectToClipPos(p0vertex);
				triStream.Append(o);

				o.uv = p[1].uv;
				o.worldPos = mul(unity_ObjectToWorld, p[1].vertex);
				o.vertex = UnityObjectToClipPos(p1vertex);
				triStream.Append(o);

				o.uv = p[2].uv;
				o.worldPos = mul(unity_ObjectToWorld, p[2].vertex);
				o.vertex = UnityObjectToClipPos(p2vertex);
				triStream.Append(o);

				float longevityLut = sourceData.z;
				float4 longevityLutUVs = float4(longevityLut, 0, 0, 0);
				float3 longevityColor = tex2Dlod(_LongevityLut, longevityLutUVs).xyz;

				o.color = lerp(color, longevityColor, _LongevityHeightAlpha);
				 
				if (currentHeight == 0)
				{
					return;
				}

				DrawHorizontal(uvs, currentHeight, p0vertex, p1vertex, p2vertex, o, triStream);
				DrawVertical(uvs, currentHeight, p0vertex, p1vertex, p2vertex, o, triStream);
			}
			 
			fixed4 frag (g2f i) : SV_Target
			{
				float3 toLight = normalize(_LightPos - i.worldPos);
				float lambert = dot(i.normal, toLight);
				float3 ret = lerp(i.color * _BackLightColor, i.color * _MainLightColor, lambert);

				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 halfVector = normalize(viewDir + toLight);
				float shine = saturate(dot(i.normal, halfVector));
				shine = pow(shine, _ShinePower);
				ret += shine * _ShineColor;

				float bounceScale = _HeatBounceScale * _HeatHeightAlpha + _LongevityBounceScale * _LongevityHeightAlpha;
				float3 bounceLight = i.bounceLightColor * ( 1 - saturate(i.distFromBounce * bounceScale)) * .5;
				ret += bounceLight;
				float psuedoOcclusion = pow(saturate(i.distFromBounce * bounceScale), .2);
				ret *= max(psuedoOcclusion, 1 - _HeatHeightAlpha); // Only want psuedoOcclusion with heat height

				return float4(ret, 1);
			}
			ENDCG
		}
	} 
}
