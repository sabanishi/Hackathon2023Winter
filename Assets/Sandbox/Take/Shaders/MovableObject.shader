Shader "Unlit/MovableObject"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ObjectType ("ObjectType", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Geometry" }
        Blend SrcAlpha OneMinusSrcAlpha

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
uniform int _ObjectType;
uniform float velocity; // normalized: 0-1

float2x2 rotate2D(const float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
}

float isInCircle(float2 uv, float r)
{
    float l = length(uv);
    return step(l, r);
}

fixed4 outLineCircle(float2 uv, float r)
{
    float d = length(uv);
    float s = 0.02 / abs(d - r);

    return fixed4(s, s*s, s*s*0.3, 1.0);
}

fixed4 outLineGreenQuad(float2 uv, float r)
{
    uv = mul(rotate2D(PI/4.0), uv);
    float d = abs(uv.x) + abs(uv.y);
    float s = 0.03 / abs(d - r);

    return fixed4(s*s, s, s*0.4, 1.0);
}

fixed4 outLineYellowQuad(float2 uv, float r)
{
    uv = mul(rotate2D(PI/4.0), uv);
    float d = abs(uv.x) + abs(uv.y);
    float s = 0.03 / abs(d - r);

    return fixed4(s, s, s*0.2, 1.0);
}

fixed4 circle(float val)
{
    return fixed4(sqrt(val), val*val, val*val*0.3, 1.0);
}

fixed4 greenQuad(float val)
{
    return fixed4(val*val, sqrt(val), val*val*0.4, 1.0);
}

fixed4 yellowQuad(float val)
{
    return fixed4(sqrt(val), sqrt(val), val*val*0.2, 1.0);
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
    float2 uv = i.uv - 0.5;
    float2 uv2 = uv * 16.0;
    float l = 0.0;
    float time = _Time.y;
    for (int i = 0; i < 4; i++)
    {
        l = lerp(l, length(uv2), lerp(0.05, 1.0, velocity));
        uv2 = float2(uv2.x + sin(uv2.y + cos(l + time * 2.0) + time * 4.0),
                     uv2.y + cos(uv2.x + sin(l + time * 2.0) + time * 4.0));
    }
    float val = 0.5 + 0.5 * cos(l);
    val = 1.0 - val;

    fixed4 col = fixed4(0.0, 0.0, 0.0, 0.0);
    if (_ObjectType == 0)
    { // circle
        float r = 0.5;
        if (isInCircle(uv, r))
        {
            col = circle(val);
            col += outLineCircle(uv, r);
        }
    }
    else if (_ObjectType == 1)
    { // greenQuad
        float r = 1./sqrt(2.);
        col = greenQuad(val);
        col += outLineGreenQuad(uv, r);
    }
    else
    { // yellowQuad
        float r = 1./sqrt(2.);
        col = yellowQuad(val);
        col += outLineYellowQuad(uv, r);
    }
    
    return col;
}
ENDCG
        }
    }
}
