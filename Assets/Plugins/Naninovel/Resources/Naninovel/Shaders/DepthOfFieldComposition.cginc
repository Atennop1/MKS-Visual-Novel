// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

#include "DepthOfFieldCommon.cginc"

sampler2D _BlurTex;
float4 _BlurTex_TexelSize;

// Fragment shader: Additional blur
half4 frag_Blur2(v2f i) : SV_Target
{
    // 9-tap tent filter
    float4 duv = _MainTex_TexelSize.xyxy * float4(1, 1, -1, 0);
    half4 acc;

    acc = tex2D(_MainTex, i.uv - duv.xy);
    acc += tex2D(_MainTex, i.uv - duv.wy) * 2;
    acc += tex2D(_MainTex, i.uv - duv.zy);

    acc += tex2D(_MainTex, i.uv + duv.zw) * 2;
    acc += tex2D(_MainTex, i.uv) * 4;
    acc += tex2D(_MainTex, i.uv + duv.xw) * 2;

    acc += tex2D(_MainTex, i.uv + duv.zy);
    acc += tex2D(_MainTex, i.uv + duv.wy) * 2;
    acc += tex2D(_MainTex, i.uv + duv.xy);

    return acc / 16;
}

// Fragment shader: Upsampling and composition
half4 frag_Composition(v2f i) : SV_Target
{
    half4 cs = tex2D(_MainTex, i.uv);
    half4 cb = tex2D(_BlurTex, i.uvAlt);
    #if defined(UNITY_COLORSPACE_GAMMA)
    cs.rgb = GammaToLinearSpace(cs.rgb);
    #endif
    half3 rgb = cs * cb.a + cb.rgb;
    #if defined(UNITY_COLORSPACE_GAMMA)
    rgb = LinearToGammaSpace(rgb);
    #endif

    return half4(rgb, cs.a);
}
