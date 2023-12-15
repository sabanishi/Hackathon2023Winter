Shader "Unlit/Load"
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

v2f vert (appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    return o;
}

fixed4 frag (v2f i) : SV_Target
{
    float2 uv = abs(i.uv - 0.5);
    float k1 = 0.5;
    float k2 = 0.5;
    for (int j = 0; j < 2; j++)
    {
        uv = (1.0-k1) + k1 * cos(uv * TAU - _Time.y);
        uv = (1.0-k2) + k2 * sin(uv * TAU + _Time.y * 2.0);
    }
    float v = dot(uv, uv);
    
    return fixed4(v, v, v, v);
}
ENDCG
        }
    }
}
