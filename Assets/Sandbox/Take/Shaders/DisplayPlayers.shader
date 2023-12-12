Shader "Unlit/DisplayPlayers"
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
uniform float4 _MainTex_TexelSize;
uniform float2 position;
uniform float2 circlePosition;
uniform float3 circleInfo; // scale and rotation
uniform float2 quadPosition;
uniform float3 quadInfo; // scale and rotation

float2x2 rotate2D(float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
}

fixed4 circle(float2 uv, float2 pos, float3 info)
{
    uv -= pos;
    uv.x *= info.y / info.x;
    float d = length(uv);
    float r = info.y;
    float s = sqrt(r) * 0.02 / abs(r-d);
    float t = atan2(uv.y, uv.x) + info.z;
    t = 0.6 + 0.4 * sin(t);
    s *= t;

    return fixed4(s, s*s, s*s*0.3, 1.0);
}

fixed4 quad(float2 uv, float2 pos, float3 info)
{
    uv -= pos;
    uv.x *= info.y / info.x;
    uv = mul(rotate2D(PI/4.0 + info.z), uv);
    float d = abs(uv.x) + abs(uv.y);
    float r = info.y;
    float s = sqrt(r) * 0.01 / abs(r-d);

    return fixed4(s*s, s, s*0.4, 1.0);
}

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    // float2 uv2 = i.uv - position;
    // uv2.x *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;
    // //uv2 = rotate2D(PI/4.0) * uv2;
    // float d = abs(uv2.x) + abs(uv2.y);
    // float s = 0.01 / abs(0.2 - d);
    fixed4 circleCol = circle(i.uv, circlePosition, circleInfo);
    fixed4 quadCol = quad(i.uv, quadPosition, quadInfo);
    fixed4 col = tex2D(_MainTex, i.uv) + circleCol + quadCol;
    
    return col;
}
ENDCG
        }
    }
}
