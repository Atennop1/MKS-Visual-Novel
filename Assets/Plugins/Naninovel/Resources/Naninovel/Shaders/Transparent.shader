// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

Shader "Hidden/Naninovel/Transparent"
{
    Properties
    {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Opacity ("Opacity", float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
        }

        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            
            #include "UnityCG.cginc"

            #pragma target 2.0
            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Opacity;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float2 TexCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                float2 TexCoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput ComputeVertex (VertexInput vertexInput)
            {
                VertexOutput vertexOutput;
                UNITY_SETUP_INSTANCE_ID(vertexInput);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);
                vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
                vertexOutput.TexCoord = TRANSFORM_TEX(vertexInput.TexCoord, _MainTex);
                return vertexOutput;
            }

            fixed4 ComputeFragment (VertexOutput vertexOutput) : SV_Target
            {
                fixed4 color = tex2D(_MainTex, vertexOutput.TexCoord);
                color.rgb *= _Opacity;
                return color;
            }
            
            ENDCG
        }
    }
}
