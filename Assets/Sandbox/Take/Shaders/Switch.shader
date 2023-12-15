Shader "Unlit/Switch"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorType ("ColorType", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

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

uniform int _ColorType;

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float2 uv = i.uv - 0.5;
    uv.x = abs(uv.x);
    uv = 1.0 - uv * 2.0;
    
    float3 col;
    if (_ColorType == 0)
    {
        col = min(float3(uv.x, uv.x*uv.x, uv.x*uv.x*0.3),
                  float3(uv.y, uv.y*uv.y, uv.y*uv.y*0.3));
    }
    else
    {
        col = min(float3(uv.x*uv.x, uv.x, uv.x*0.4),
                  float3(uv.y*uv.y, uv.y, uv.y*0.4));
    }
    col *= col;
    
    return fixed4(col.x, col.y, col.z, 1.0);
}
ENDCG
        }
    }
}
