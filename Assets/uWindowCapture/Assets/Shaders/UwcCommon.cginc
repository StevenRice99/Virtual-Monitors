﻿#ifndef UWC_COMMON_CGINC
#define UWC_COMMON_CGINC

#include "UnityCG.cginc"

float4 _Color;
sampler2D _MainTex;
//UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); // VR CHANGE
float4 _MainTex_ST;

inline void UwcFlipUV(inout float2 uv)
{
    #ifdef UWC_FLIP_X
    uv.x = 1.0 - uv.x;
    #endif
    #ifdef UWC_FLIP_Y
    uv.y = 1.0 - uv.y;
    #endif
}

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;

    UNITY_VERTEX_INPUT_INSTANCE_ID // VR CHANGE.
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_FOG_COORDS(1)

    UNITY_VERTEX_OUTPUT_STEREO //VR CHANGE
};

v2f vert(appdata v)
{
    v2f o;

    UNITY_SETUP_INSTANCE_ID(v); // VR CHANGE
    UNITY_INITIALIZE_OUTPUT(v2f, o); // VR CHANGE
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); // VR CHANGE
    
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    UwcFlipUV(o.uv);
    UNITY_TRANSFER_FOG(o,o.vertex);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert
    //fixed4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv); //Insert
    float4 col = float4(tex2D(_MainTex, i.uv).rgb * _Color, _Color.a);
    //float4 col = float4(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv).rgb * _Color, _Color.a);
    UNITY_APPLY_FOG(i.fogCoord, col);
    return col;
}

#endif