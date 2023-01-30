// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="TransitionalRenderer"/> implementation, that outputs the result to a render texture.
    /// </summary>
    public class TransitionalTextureRenderer : TransitionalRenderer
    {
        /// <summary>
        /// Render texture to output the render result.
        /// </summary>
        public virtual RenderTexture RenderTexture { get; set; }
        /// <summary>
        /// Modifier of the texture areas to render.
        /// </summary>
        public virtual Rect RenderRectangle { get; set; } = new Rect(0, 0, 1, 1);

        protected virtual void Update ()
        {
            if (ShouldRender())
                RenderToTexture(RenderTexture);
        }

        protected override (Vector2 offset, Vector2 scale) GetUVModifiers (Vector2 renderSize, Vector2 textureSize)
        {
            var (offset, scale) = base.GetUVModifiers(renderSize, textureSize);
            return CropUVs(offset, scale);
        }

        private (Vector2 offset, Vector2 scale) CropUVs (Vector2 offset, Vector2 scale)
        {
            offset = (offset + RenderRectangle.position) / RenderRectangle.size;
            scale /= RenderRectangle.size;
            return (offset, scale);
        }
    }
}
