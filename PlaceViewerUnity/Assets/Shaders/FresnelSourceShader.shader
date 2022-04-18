Shader "PostProcess/FresnelSourceShader"
{
  Properties
  {
      _MainTex("Texture", 2D) = "white" {}
  }
    SubShader
  {
    // No culling or depth
    //Cull Off ZWrite Off ZTest Always

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
          float3 normal : NORMAL;
      };

      struct v2f
      {
          float2 uv : TEXCOORD0;
          float4 vertex : SV_POSITION;
          float3 normal : NORMAL;
          float3 worldPos: TEXCOORD1;
      };

      v2f vert(appdata v)
      {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          o.normal = mul(unity_ObjectToWorld, v.normal);
          o.worldPos = mul(unity_ObjectToWorld, v.vertex);
          return o;
      }

      sampler2D _MainTex;

      fixed4 frag(v2f i) : SV_Target
      {
          i.normal = normalize(i.normal);
          float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
          float theDot = dot(i.normal, viewDir);
          i.normal.xy = i.normal.xy * .5 + .5;
          i.normal.z = pow(1 - theDot, 2) * .5;
          return float4(i.normal, 1);
      }
      ENDCG
    }
  }
}
