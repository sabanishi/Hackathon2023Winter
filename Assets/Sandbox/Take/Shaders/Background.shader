Shader "Unlit/Background"
{
    Properties
    {
        _AspectRatio ("Aspect Ratio", Float) = 1.77777778 // 16.0 / 9.0
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

uniform float _AspectRatio;

float random(const float2 st)
{
    return frac(sin(dot(st, float2(12.9898,78.233))) * 43758.5453);
}

float2 random2D(const float2 st)
{
    float2 val = float2( dot(st,float2(127.1,311.7)),
                         dot(st,float2(269.5,183.3)) );
    return frac(sin(val)*43758.5453123);
}

float noise(const float2 v)
{
    float2 i = floor(v);
    float2 f = frac(v);
    float2 u = f*f*(3.-2.*f);
    return lerp( lerp( random( i + float2(0., 0.) ),
                       random( i + float2(1., 0.) ), u.x),
                 lerp( random( i + float2(0., 1.) ),
                       random( i + float2(1., 1.) ), u.x), u.y);
}

float voronoi(float2 x)
{
    float2 p = floor(x);
    float2 f = frac(x);

    float res = 0.0;
    for (int j = -2; j <= 2; j++)
    {
        for (int i = -2; i <= 2; i++)
        {
            float2 b = float2(i, j);
            float2 rand = random2D(p + b);
            float2 time = lerp(float2(0.3, 0.3), float2(1.6, 1.6), rand) * _Time.y;
            float2 rand2 = 0.5 + 0.5 * float2(cos(TAU * rand.x + time.x), sin(TAU * rand.x - time.y));
            rand *= 0.5 + 0.5 * cos(TAU * sin(TAU * rand) + time);
            rand2 *= lerp(float2(0.5, 0.5), float2(1.0, 1.0), rand);
            float2 r = float2(b) + float2(rand2) - f;
            float d = dot(r, r);

            res += exp2(-16.0*d);
        }
    }

    return -(1.0/16.0)*log2(res);
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
    float2 uv2 = i.uv * 8.0;
    uv2.x *= _AspectRatio;
    float v = voronoi(uv2);

    float3 lig = normalize(float3(1.3, 0.1, 0.4));
    float3 dir = normalize(float3(0.0, 1.0, 0.0));
    float2 ex = float2(0.03, 0.03);
    float3 nor = normalize(float3(voronoi(uv2 + float2(ex.x, 0.0)) - v,
                                  2.0 * min(ex.x, ex.y),
                                  voronoi(uv2 + float2(0.0, ex.y)) - v));
    float dif = dot(nor, lig);
    float3 ref = reflect(lig, nor);
    float spe = dot(dir, ref);
    spe *= spe * 0.2;

    float3 col = float3(v, v, v);
    col = col * max(dif, -spe) + spe;
    col *= 0.6;
    
    return fixed4(col.x, col.y, col.z, 1.0);
}
ENDCG
        }
    }
}
