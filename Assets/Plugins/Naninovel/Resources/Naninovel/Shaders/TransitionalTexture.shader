// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

Shader "Naninovel/TransitionalTexture"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "black" {}
        _TransitionTex("Transition Texture", 2D) = "black" {}
        _CloudsTex("Clouds Texture", 2D) = "black" {}
        _DissolveTex("Dissolve Texture", 2D) = "black" {}
        _TransitionProgress("Transition Progress", Float) = 0
        _TransitionParams("Transition Parameters", Vector) = (1,1,1,1)
        _TintColor("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            
            #include "UnityCG.cginc"
            #include "TransitionEffects.cginc"

            #pragma target 2.0
            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment
            #pragma multi_compile_local _ NANINOVEL_TRANSITION_BANDEDSWIRL NANINOVEL_TRANSITION_BLINDS NANINOVEL_TRANSITION_CIRCLEREVEAL NANINOVEL_TRANSITION_CIRCLESTRETCH NANINOVEL_TRANSITION_CLOUDREVEAL NANINOVEL_TRANSITION_CRUMBLE NANINOVEL_TRANSITION_DISSOLVE NANINOVEL_TRANSITION_DROPFADE NANINOVEL_TRANSITION_LINEREVEAL NANINOVEL_TRANSITION_PIXELATE NANINOVEL_TRANSITION_RADIALBLUR NANINOVEL_TRANSITION_RADIALWIGGLE NANINOVEL_TRANSITION_RANDOMCIRCLEREVEAL NANINOVEL_TRANSITION_RIPPLE NANINOVEL_TRANSITION_ROTATECRUMBLE NANINOVEL_TRANSITION_SATURATE NANINOVEL_TRANSITION_SHRINK NANINOVEL_TRANSITION_SLIDEIN NANINOVEL_TRANSITION_SWIRLGRID NANINOVEL_TRANSITION_SWIRL NANINOVEL_TRANSITION_WATER NANINOVEL_TRANSITION_WATERFALL NANINOVEL_TRANSITION_WAVE NANINOVEL_TRANSITION_CUSTOM
            #pragma multi_compile_local _ PREMULTIPLIED_ALPHA

            sampler2D _MainTex, _TransitionTex, _DissolveTex, _CloudsTex;
            float4 _MainTex_ST, _TransitionTex_ST;
            float _TransitionProgress, _FlipMainX;
            float2 _RandomSeed;
            fixed4 _TintColor;
            float4 _TransitionParams;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float4 Color : COLOR;
                float2 MainTexCoord : TEXCOORD0;
                float2 TransitionTexCoord : TEXCOORD1;
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                fixed4 Color : COLOR;
                float2 MainTexCoord : TEXCOORD0;
                float2 TransitionTexCoord : TEXCOORD1;
            };

            VertexOutput ComputeVertex(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;
                vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
                vertexOutput.MainTexCoord = TRANSFORM_TEX(vertexInput.MainTexCoord, _MainTex);
                vertexOutput.MainTexCoord.x = lerp(vertexOutput.MainTexCoord.x, 1 - vertexOutput.MainTexCoord.x, _FlipMainX);
                vertexOutput.TransitionTexCoord = TRANSFORM_TEX(vertexInput.TransitionTexCoord, _TransitionTex);
                vertexOutput.Color = vertexInput.Color * _TintColor;
                return vertexOutput;
            }

            fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
            {
                fixed4 color = ApplyTransitionEffect(_MainTex, vertexOutput.MainTexCoord,
                                                     _TransitionTex, vertexOutput.TransitionTexCoord,
                                                     _TransitionProgress, _TransitionParams, _RandomSeed, _CloudsTex, _DissolveTex);
                color *= vertexOutput.Color;
                return color;
            }
            
            ENDCG
        }
    }
}
