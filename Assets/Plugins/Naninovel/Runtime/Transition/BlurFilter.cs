// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows applying gaussian blur effect to <see cref="RenderTexture"/>.
    /// </summary>
    public class BlurFilter : IDisposable
    {
        private const string shaderName = "Hidden/Naninovel/BlurFilter";
        private static readonly int intensityId = Shader.PropertyToID("_Intensity");

        private readonly Material blurMaterial;
        private readonly int iterations;
        private readonly bool downsample;

        public BlurFilter (int iterations, bool downsample)
        {
            if (iterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations));
            this.downsample = downsample;
            this.iterations = iterations;
            blurMaterial = CreateBlurMaterial();
        }

        public void BlurTexture (RenderTexture texture, float intensity)
        {
            blurMaterial.SetFloat(intensityId, intensity);
            
            var rt1 = GetTemporaryTexture(texture);
            var rt2 = GetTemporaryTexture(texture);

            CopySourceTexture(texture, rt1);

            for (var i = 0; i < iterations; i++)
            {
                Graphics.Blit(rt1, rt2, blurMaterial, 1);
                Graphics.Blit(rt2, rt1, blurMaterial, 2);
            }

            Graphics.Blit(rt1, texture);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);
        }

        public void Dispose () => ObjectUtils.DestroyOrImmediate(blurMaterial);

        private static Material CreateBlurMaterial ()
        {
            var shader = Shader.Find(shaderName);
            var blurMaterial = new Material(shader);
            blurMaterial.hideFlags = HideFlags.HideAndDontSave;
            return blurMaterial;
        }

        private RenderTexture GetTemporaryTexture (Texture source)
        {
            var width = downsample ? source.width / 4 : source.width;
            var height = downsample ? source.height / 4 : source.height;
            return RenderTexture.GetTemporary(width, height);
        }

        private void CopySourceTexture (Texture source, RenderTexture target)
        {
            if (downsample) Graphics.Blit(source, target, blurMaterial, 0);
            else Graphics.Blit(source, target);
        }
    }
}
