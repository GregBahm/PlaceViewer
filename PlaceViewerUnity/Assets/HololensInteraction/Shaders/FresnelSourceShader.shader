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
            float3 VSNormal : TEXCOORD2;
        };

        sampler2D _GrabBlurTexture;
        sampler2D _GrabBaseTexture;

        v2f vert(appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            o.normal = mul(unity_ObjectToWorld, v.normal);
            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
            o.VSNormal = COMPUTE_VIEW_NORMAL;
            return o;
        }

        fixed4 frag(v2f i) : SV_Target
        {
            //return float4(i.VSNormal, 1);
            float2 screenCoords = i.vertex.xy / _ScreenParams.xy;
            float2 refractedCoords = screenCoords + i.VSNormal.xy * .4;
            fixed4 refracted = tex2D(_GrabBaseTexture, refractedCoords);
            fixed4 blurred = tex2D(_GrabBlurTexture, refractedCoords);
            fixed4 col = lerp( refracted, blurred, 1 - pow(i.VSNormal.z, 10));
            //return blurred;
            
            i.normal = normalize(i.normal);
            float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
            float theDot = dot(i.normal, viewDir);

            float fresnel = pow(1 - theDot, 2);
            return blurred + fresnel * .4;
            i.normal.xy = i.normal.xy * .5 + .5;
            i.normal.z = pow(1 - theDot, 2) * .5;
            float4 ret = float4(i.normal, 1);
            ret = lerp(blurred, ret, .5);
            return ret;
        }
      ENDCG
    }
  }
}
