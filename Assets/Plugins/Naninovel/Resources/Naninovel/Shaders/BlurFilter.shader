// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


Shader "Hidden/Naninovel/BlurFilter"
{
    Properties
    {
        _MainTex("-", 2D) = "white" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;
    float _Intensity;

    half4 DownsampleToQuarterSize(v2f_img i) : SV_Target
    {
        const float4 deviation = _MainTex_TexelSize.xyxy * float4(1, 1, -1, -1);
        
        half4 color = tex2D(_MainTex, i.uv + deviation.xy);
        color += tex2D(_MainTex, i.uv + deviation.xw);
        color += tex2D(_MainTex, i.uv + deviation.zy);
        color += tex2D(_MainTex, i.uv + deviation.zw);
        
        return color * 0.25;
    }

    half4 Blur(const float2 uv, const float2 stride)
    {
        const float deviationStep1 = 1.3846153846;
        const float deviationStep2 = 3.2307692308;
        const float blurStep1 = 0.2270270270;
        const float blurStep2 = 0.3162162162;
        const float blurStep3 = 0.0702702703;
        
        const float2 deviation1 = stride * deviationStep1 * _Intensity;
        const float2 deviation2 = stride * deviationStep2 * _Intensity;
        
        half4 color = tex2D(_MainTex, uv) * blurStep1;
        color += tex2D(_MainTex, uv + deviation1) * blurStep2;
        color += tex2D(_MainTex, uv - deviation1) * blurStep2;
        color += tex2D(_MainTex, uv + deviation2) * blurStep3;
        color += tex2D(_MainTex, uv - deviation2) * blurStep3;
        
        return color;
    }

    half4 BlurHorizontal(v2f_img i) : SV_Target
    {
        return Blur(i.uv, float2(_MainTex_TexelSize.x, 0));
    }

    half4 BlurVertical(v2f_img i) : SV_Target
    {
        return Blur(i.uv, float2(0, _MainTex_TexelSize.y));
    }

    ENDCG

    Subshader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment DownsampleToQuarterSize
            ENDCG
        }
        
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment BlurHorizontal
            #pragma target 3.0
            ENDCG
        }
        
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment BlurVertical
            #pragma target 3.0
            ENDCG
        }
    }
}
