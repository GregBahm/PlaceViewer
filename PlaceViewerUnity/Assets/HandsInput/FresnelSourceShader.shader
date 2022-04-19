Shader "Frosted/Frosted"
{
  Properties
  {
  }
    SubShader
  {
    // No culling or depth
    //Cull Off 
    //ZWrite Off 
    //ZTest Always

    Pass
    {
      Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
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

        sampler2D _GrabBlurTexture;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            o.normal = mul(unity_ObjectToWorld, v.normal);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            float2 screenCoords = i.vertex.xy / _ScreenParams.xy;
            fixed4 blur = tex2D(_GrabBlurTexture, screenCoords);
            return blur;
            i.normal = normalize(i.normal);
          
            float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
            float theDot = dot(i.normal, viewDir);
            i.normal.xy = i.normal.xy * .5 + .5;
            i.normal.z = pow(1 - theDot, 2) * .5;
            float4 ret = float4(i.normal, 1);
            ret = lerp(blur, ret, .5);
            return ret;
        }
      ENDCG
    }
  }
}
