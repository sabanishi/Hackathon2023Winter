Shader "Unlit/Title"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Background" }

        Pass
        {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

#define PI 3.14159265
#define TAU (PI * 2.0)
#define MAX_PARTICLE 512

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

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform float4 particlePositions[MAX_PARTICLE]; // position
uniform float4 particleInfo[MAX_PARTICLE]; // scale and rotation (radians)
uniform int particleNum;

float2x2 rotate2D(float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
}

float3 quad(float2 uv, float2 pos, float3 info)
{
    if (info.x * info.y == 0) { return float3(0.0, 0.0, 0.0); }
    
    uv -= pos;
    uv.x *= info.y / info.x;
    uv = mul(rotate2D(PI/4.0 + info.z), uv);
    float d = abs(uv.x) + abs(uv.y);
    float r = info.y;
    float s = 0.01 / abs(r-d);
    s = sqrt(r)*s*s;

    return float3(s*s, s, s*0.4);
}

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float3 col = float3(0.0, 0.0, 0.0);

    for (int j = 0; j < particleNum; j++)
    {
        col += quad(i.uv, particlePositions[j].xy, particleInfo[j].xyz);
    }
    
    return tex2D(_MainTex, i.uv) +  fixed4(col.x, col.y, col.z, 1.0);
}
ENDCG
        }
    }
}
