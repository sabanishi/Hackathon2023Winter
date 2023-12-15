Shader "Unlit/ConnectLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }

        Pass
        {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

#define PI 3.14159265
#define TAU (PI*2.0)
#define MAX_ANKER 32

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

uniform float4 ankerPoints[MAX_ANKER]; // normalized: 0-1
uniform int ankerNum; // max: 32
uniform float activeTime; // normalized: 0-1
uniform int colorType; // 0: orange, otherwise: green

float2x2 rotate2D(float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
}

float point2lineDist(float2 uv, float2 v1, float2 v2)
{
    float2 v12 = v2 - v1;
    float2 n = normalize(mul(rotate2D(PI * 0.5), v12));

    return abs(dot(n, uv - v1)) / length(n);
}

bool isProjectionBetweenLine(float2 uv, float2 v1, float2 v2)
{
    return dot(uv-v1, v2-v1) > 0.0 && dot(uv-v2, v1-v2) > 0.0;
}

float calcTotalLength()
{
    float l = 0.0;
    for (int i = 0; i < ankerNum-1; i++)
    {
        l += distance(ankerPoints[i].xy, ankerPoints[i+1].xy);
    }
    return l;
}

float3 endPositionAndIndex(float ratio)
{
    ratio = clamp(ratio, 0.0, 1.0);
    
    float totalLength = calcTotalLength();
    float l = 0.0;
    int index = 0;
    float3 ret;
    for (int i = 0; i < ankerNum-1; i++)
    {
        float partLength = distance(ankerPoints[i].xy, ankerPoints[i+1].xy);
        l += partLength;
        if (l >= totalLength * ratio)
        {
            float remLength = partLength + totalLength * ratio - l;
            float2 pos = ankerPoints[i].xy + (ankerPoints[i+1].xy - ankerPoints[i].xy) * remLength / partLength;
            ret = float3(pos, index);
            break;
        }
        index++;
    }

    return ret;
}

float lineLight(float2 uv)
{
    float d = 8.0;
    
    float3 info = endPositionAndIndex(activeTime);

    for (int i = 0; i < ankerNum-1; i++)
    {
        float2 v1 = ankerPoints[i].xy;
        float2 v2 = float(i) != info.z ? ankerPoints[i+1].xy : info.xy;
        
        if (isProjectionBetweenLine(uv, v1, v2))
        {
            float d12 = point2lineDist(uv, v1, v2);
            d = min(d, d12);
        }
        else
        {
            float d1 = distance(uv, v1);
            float d2 = distance(uv, v2);
            d = min(d, min(d1, d2));
        }

        if (float(i) == info.z) { break; }
    }

    return 0.005 / d;
}

float linePath(float2 uv)
{
    float d = 8.0;

    for (int i = 0; i < ankerNum-1; i++)
    {
        float2 v1 = ankerPoints[i].xy;
        float2 v2 = ankerPoints[i+1].xy;
        
        if (isProjectionBetweenLine(uv, v1, v2))
        {
            float d12 = point2lineDist(uv, v1, v2);
            d = min(d, d12);
        }
        else
        {
            float d1 = distance(uv, v1);
            float d2 = distance(uv, v2);
            d = min(d, min(d1, d2));
        }
    }

    return 0.00001 / (d*d);
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
    fixed4 col = tex2D(_MainTex, i.uv);
    
    float lightVal = lineLight(i.uv);
    float path = linePath(i.uv);
    
    col += fixed4(path, path, path, 0.0);
    
    if (colorType == 0)
    { // orange
        col += fixed4(lightVal, lightVal*0.5, 0.0, 0.0);
    }
    else
    { // green
        col += fixed4(lightVal*0.5, lightVal, lightVal*0.4, 0.0);
    }
    
    return col;
}
ENDCG
        }
    }
}
