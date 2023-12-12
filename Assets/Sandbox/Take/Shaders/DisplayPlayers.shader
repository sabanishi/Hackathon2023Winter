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

float2x2 rotate2D(float rad)
{
    return float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
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
    float2 uv2 = i.uv - position;
    uv2.x *= _MainTex_TexelSize.z / _MainTex_TexelSize.w;
    //uv2 = rotate2D(PI/4.0) * uv2;
    float d = abs(uv2.x) + abs(uv2.y);
    float s = 0.01 / abs(0.2 - d);
    
    fixed4 col = tex2D(_MainTex, i.uv) + fixed4(s, s, s, 0.0);
    
    return col;
}
ENDCG
        }
    }
}
