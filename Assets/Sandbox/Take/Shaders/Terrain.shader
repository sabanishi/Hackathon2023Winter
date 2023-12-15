Shader "Unlit/Terrain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AspectRatio ("Aspect Ratio", Float) = 1.77777778 // 16.0 / 9.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            Stencil
            {
                Ref 2
                Comp Equal
            }
            
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

uniform sampler2D _MainTex;
uniform float4 _MainTex_ST;
uniform float _AspectRatio;

float2 random2D(const float2 st)
{
    float2 val = float2( dot(st,float2(127.1,311.7)),
                     dot(st,float2(269.5,183.3)) );
    return frac(sin(val)*43758.5453123);
}

float voronoi( in float2 x )
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
            float t = _Time.y * lerp(0.25, 0.75, rand);
            float2 r = float2(b) + (0.5 + 0.5 * sin(16.0 * rand + t)) - f;
            float d = abs(r.x) + abs(r.y); // Manhattan distance

            res += exp2(-32.0 * d);
        }
    }

    return -(1.0 / 32.0) * log2(res);
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
    float2 uv2 = i.uv * 16.0;
    uv2.x *= _AspectRatio;
    float v = voronoi(uv2);

    float3 lig = normalize(float3(0.6, 0.1, 0.2));
    float3 dir = normalize(float3(0.0, 1.0, 0.2));
    float2 ex = float2(0.03, 0.03);
    float3 nor = normalize(float3(voronoi(uv2 + float2(ex.x, 0.0)) - v,
                                  2.0 * min(ex.x, ex.y),
                                  voronoi(uv2 + float2(0.0, ex.y)) - v));
    float dif = dot(nor, lig);
    float3 ref = reflect(lig, nor);
    float spe = dot(dir, ref);
    spe *= spe;
    
    float3 col = float3(v, v, v);
    col = col * max(dif, -spe) + spe;
    
    return fixed4(col.x, col.y, col.z, 1.0);
}
ENDCG
        }
    }
}
