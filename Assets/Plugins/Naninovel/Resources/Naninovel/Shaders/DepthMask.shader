// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

Shader "Hidden/Naninovel/DepthMask"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "black" {}
        _DepthAlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "RenderType" = "TransparentCutout"
            "PreviewType" = "Plane"
            "LightMode" = "ShadowCaster"
        }

        Cull Off
        ColorMask 0

        Pass
        {
            Name "DepthMask"

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma target 2.0
            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _DepthAlphaCutoff;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float2 MainTexCoord : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                float2 MainTexCoord : TEXCOORD0;
            };

            VertexOutput ComputeVertex(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;
                vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
                vertexOutput.MainTexCoord = TRANSFORM_TEX(vertexInput.MainTexCoord, _MainTex);
                return vertexOutput;
            }

            fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, vertexOutput.MainTexCoord);
                clip(color.a - _DepthAlphaCutoff);
                return color;
            }

            ENDCG
        }
    }
}
