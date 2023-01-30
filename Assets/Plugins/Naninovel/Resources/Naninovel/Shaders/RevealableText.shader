// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

Shader "Naninovel/RevealableText"
{
    Properties
    {
        [PerRendererData] _MainTex("Font Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

        _LineClipRect("Lines Clip Rect", Vector) = (0,0,0,0)
        _CharClipRect("Characters Clip Rect", Vector) = (0,0,0,0)
        _CharFadeWidth("Characters Fade Width", Float) = 0
        _CharSlantAngle("Characters Slate Angle", Float) = 0
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

            #pragma target 2.0
            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "NaninovelCG.cginc"

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            sampler2D _MainTex;

            float4 _LineClipRect, _CharClipRect;
            float _CharFadeWidth, _CharSlantAngle;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float4 Color : COLOR;
                float2 TexCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                fixed4 Color : COLOR;
                float2 TexCoord : TEXCOORD0;
                float4 WorldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            VertexOutput ComputeVertex(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;

                UNITY_SETUP_INSTANCE_ID(vertexInput);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);
                vertexOutput.WorldPos = vertexInput.Vertex;
                vertexOutput.Vertex = UnityObjectToClipPos(vertexOutput.WorldPos);
                vertexOutput.TexCoord = vertexInput.TexCoord;
                vertexOutput.Color = vertexInput.Color * _Color;

                return vertexOutput;
            }

            fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
            {
                half4 color = tex2D(_MainTex, vertexOutput.TexCoord) + _TextureSampleAdd;
                color *= vertexOutput.Color;

                color.a *= UnityGet2DClipping(vertexOutput.WorldPos.xy, _ClipRect);

                // Hide unrevealed (not-printed-yet) text lines.
                color.a *= IsPositionOutsideRect(vertexOutput.WorldPos.xy, _LineClipRect);

                // Apply gradient fade to the current line.
                if (vertexOutput.WorldPos.y < _CharClipRect.w)
                {
                    float alpha = PI / 2.0 - radians(_CharSlantAngle);
                    float distance = (_CharClipRect.x - vertexOutput.WorldPos.x) + (vertexOutput.WorldPos.y - _CharClipRect.y) / tan(alpha);
                    color.a *= saturate(distance / _CharFadeWidth);
                }

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }

            ENDCG
        }
    }
}
