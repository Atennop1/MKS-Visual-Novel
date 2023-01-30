// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable data required to construct and initialize a <see cref="IActor"/> 
    /// managed in the orthographic scene space.
    /// </summary>
    [System.Serializable]
    public abstract class OrthoActorMetadata : ActorMetadata
    {
        [Tooltip("Pivot point of the actor.")]
        public Vector2 Pivot = Vector2.zero;
        [Tooltip("PPU value of the actor.")]
        public int PixelsPerUnit = 100;
        [Tooltip("Whether to perform an additional render pass writing to z-buffer. Required for some effects (eg, depth of field).")]
        public bool EnableDepthPass = true;
        [Range(0.001f, 0.999f), Tooltip("When depth pass is enabled, controls the transparency level (alpha) threshold to cut off rendered pixels of the depth mask.")]
        public float DepthAlphaCutoff = 0.5f;
        [Tooltip("A custom shader to use when drawing and transitioning appearance textures of the actor. Be aware, that the shader is expected to have specific properties; check default shader (Naninovel/Resources/Naninovel/Shaders/TransitionalTexture) for a reference.")]
        public Shader CustomTextureShader = default;
        [Tooltip("A custom shader to use when drawing sprite of the actor (in case it's represented by a sprite on scene). By default a simple unlit transparent shader is used; in case you want to support lighting or surface effects, assign a custom shader here.")]
        public Shader CustomSpriteShader = default;
        [Tooltip("When assigned and supported by the implementation, the actor will be rendered to the texture instead of a game object on scene.")]
        public RenderTexture RenderTexture = default;
        [Tooltip("When rendering to texture, allows modifying area of the actor to render. X and Y are horizontal and vertical offsets from bottom-left corner, W and H are width and height scale of the area. Both parameters are relative, eg for 10% offset, use 0.1.\n\nExample: [X0 Y0.5 W1.5 H1.5] will render upper-half part of the actor uniformly stretched to the entire texture.")]
        public Rect RenderRectangle = new Rect(0, 0, 1, 1);
    }
}
