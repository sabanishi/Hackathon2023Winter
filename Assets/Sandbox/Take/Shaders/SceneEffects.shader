Shader "Unlit/SceneEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SubTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        
        Pass
        {
CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

#define PI 3.14159265
#define TAU (PI * 2.0)
#define NOISE_SCALE 8.0
#define OCTAVES 4
#define MAX_WARP 16

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
uniform sampler2D _SubTex;
uniform float4 _MainTex_ST;
uniform float4 _SubTex_ST;
uniform float4 _MainTex_TexelSize;
uniform float _AspectRatio;

// player info
uniform float2 circlePosition;
uniform float3 circleInfo; // scale and rotation
uniform float2 quadPosition;
uniform float3 quadInfo; // scale and rotation

// warp hole
uniform float4 warpInfo[MAX_WARP]; // position and color; color: 0: blue, otherwise: pink
uniform float isVanishing[MAX_WARP]; // 0: not vanishing, otherwise: vanishing
uniform float vanishTime[MAX_WARP]; // normalized: 0-1
uniform float4 warpSeed[MAX_WARP];
uniform int warpNum;

// scene transition
uniform int isTransition; // 0: not transition, otherwise: transition
uniform float transitionTime; // normalized: 0-1
uniform float4 transitionSeed;

float2x2 rotate2D(float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
}

float random(const float2 st)
{
    return frac(sin(dot(st, float2(12.9898,78.233))) * 43758.5453);
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

float fbm(const float2 v)
{
    float2 p = v;
    float result = 0.0;
    float amplitude = 0.5;

    for (int i = 0; i < OCTAVES; i++)
    {
        result += amplitude * noise(p);
        amplitude *= 0.5;
        p *= 2.0;
    }

    return result;
}

fixed4 circle(float2 uv, float2 pos, float3 info)
{
    if (info.x * info.y == 0) { return fixed4(0.0, 0.0, 0.0, 1.0); }
    
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
    if (info.x * info.y == 0) { return fixed4(0.0, 0.0, 0.0, 1.0); }
    
    uv -= pos;
    uv.x *= info.y / info.x;
    uv = mul(rotate2D(PI/4.0 + info.z), uv);
    float d = abs(uv.x) + abs(uv.y);
    float r = info.y;
    float s = sqrt(r) * 0.01 / abs(r-d);

    return fixed4(s*s, s, s*0.4, 1.0);
}

fixed4 sceneTransition(float2 uv, float time, float4 seed)
{
    float2 n = float2(fbm(uv * NOISE_SCALE + seed.xy), fbm(uv * NOISE_SCALE + seed.yz));
    time = clamp(time, 0.0, 1.0);
    float3 mainTex = tex2D(_MainTex, lerp(uv, n, 1.0 - time)).xyz;
    float3 subTex = tex2D(_SubTex, lerp(uv, n, time)).xyz;
    
    float s = step(n.x, time) * step(n.y, time);
    float3 col = (1.0 - s) * subTex + s * mainTex;
    
    return fixed4(col.x, col.y, col.z, 1.0);
}

fixed4 warpHole(float2 uv, float2 pos, float colorType, float isVanishing, float vanishTime, float animationTime, float2 seed)
{
    uv -= pos;
    float ratio = _MainTex_TexelSize.z * _MainTex_TexelSize.w == 0
                   ? 16.0 / 9.0
                   : _MainTex_TexelSize.z / _MainTex_TexelSize.w;
    uv.x *= ratio;
    
    float l = length(uv);
    float t = atan2(uv.y, uv.x);
    float2 val = float2(cos(t) + 1.0 + animationTime, sin(t) + 2.0 + animationTime);
    val *= 8.0;
    float n = noise(val + seed);
    l *= lerp(0.6, 1.0, n*n);
    float r = exp2(-200.0 * l + 3.0);
    if (isVanishing != 0) { r *= max(1.0 - vanishTime, 0.0); }
    float3 col = float3(r*r*4.0, 0.4*r/l, 1.6*r/l);
    fixed4 c = colorType == 0 ? fixed4(col.x, col.y, col.z, 1.0) : fixed4(col.z, col.y, col.x, 1.0);

    return c;
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
    fixed4 col;
    
    if (isTransition)
    {
        col = sceneTransition(i.uv, transitionTime, transitionSeed);
    }
    else
    {
        // display players
        fixed4 circleCol = circle(i.uv, circlePosition, circleInfo);
        fixed4 quadCol = quad(i.uv, quadPosition, quadInfo);
        col = tex2D(_MainTex, i.uv) + circleCol + quadCol;
        for (int j = 0; j < warpNum; j++)
        {
            col += warpHole(i.uv, warpInfo[j].xy, warpInfo[j].z, isVanishing[j],
                vanishTime[j], _Time.y * 0.5, warpSeed[j]);
        }
    }
    
    return col;
}
ENDCG
        }
    }
}
