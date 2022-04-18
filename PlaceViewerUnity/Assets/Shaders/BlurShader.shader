Shader "PostProcess/BlurShader"
{
  Properties
  {
      _MainTex("Texture", 2D) = "white" {}
  }
    SubShader
  {
    // No culling or depth
    Cull Off ZWrite Off ZTest Always

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
      };

      v2f vert(appdata v)
      {
          v2f o;
          o.vertex = UnityObjectToClipPos(v.vertex);
          o.uv = v.uv;
          return o;
      }

      sampler2D _MainTex;
      sampler2D _FresnelSource;

      fixed4 frag(v2f i) : SV_Target
      {
          fixed4 fresnelSource = tex2D(_FresnelSource, i.uv);
          float2 offset = (fresnelSource.xy - .5) * 2;
          offset = fresnelSource.xy;
          float2 newUvs = i.uv - offset * .05;
          fixed4 col = tex2D(_MainTex, newUvs);
          //return fresnelSource.z;
          col *= 1  + fresnelSource.z;
          return col;
      }
      ENDCG
    }
  }
}
