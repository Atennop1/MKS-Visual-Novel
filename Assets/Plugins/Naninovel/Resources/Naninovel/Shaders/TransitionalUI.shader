// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

Shader "Naninovel/TransitionalUI"
{
    Properties
    {
        [PerRendererData] _MainTex("Main Texture", 2D) = "black" {}
        [PerRendererData] _TransitionTex("Transition Texture", 2D) = "black" {}
        _TransitionProgress("Transition Progress", Float) = 0
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM

            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            sampler2D _MainTex, _TransitionTex;
            float _TransitionProgress;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST, _TransitionTex_ST;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float4 Color : COLOR;
                float2 MainTexCoord : TEXCOORD0;
                float2 TransitionTexCoord : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                fixed4 Color : COLOR;
                float2 MainTexCoord : TEXCOORD0;
                float2 TransitionTexCoord : TEXCOORD1;
                float4 WorldPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput ComputeVertex(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;

                UNITY_SETUP_INSTANCE_ID(vertexInput);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

                vertexOutput.WorldPosition = vertexInput.Vertex;
                vertexOutput.Vertex = UnityObjectToClipPos(vertexOutput.WorldPosition);

                vertexOutput.MainTexCoord = TRANSFORM_TEX(vertexInput.MainTexCoord, _MainTex);
                vertexOutput.TransitionTexCoord = TRANSFORM_TEX(vertexInput.TransitionTexCoord, _TransitionTex);

                vertexOutput.Color = vertexInput.Color * _Color;
                return vertexOutput;
            }

            fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
            {
                half4 mainColor = (tex2D(_MainTex, vertexOutput.MainTexCoord) + _TextureSampleAdd) * vertexOutput.Color;
                // Using main tex UVs here, as TMPro force-enables additional shader channels on canvas (and all the geometry inside it), which breaks transition texture UVs.
                half4 transitionColor = (tex2D(_TransitionTex, vertexOutput.MainTexCoord/*TransitionTexCoord*/) + _TextureSampleAdd) * vertexOutput.Color;

                half4 color = lerp(mainColor, transitionColor, _TransitionProgress);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(vertexOutput.WorldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }

            ENDCG
        }
    }
}
