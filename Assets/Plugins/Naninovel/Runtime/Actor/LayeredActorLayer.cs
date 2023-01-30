// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents a layer inside <see cref="LayeredActorBehaviour"/> object.
    /// </summary>
    public class LayeredActorLayer : IDisposable
    {
        public readonly string Name;
        public readonly string Group;
        public readonly Mesh Mesh;
        public readonly Renderer Renderer;
        public bool Enabled { get => Renderer.enabled; set => Renderer.enabled = value; }
        public Vector2 Position => Renderer.transform.position;
        public Quaternion Rotation => Renderer.transform.rotation;
        public Vector2 Scale => Renderer.transform.lossyScale;

        private static readonly int spriteColorId = Shader.PropertyToID("_RendererColor");

        private readonly SpriteRenderer spriteRenderer;

        public LayeredActorLayer (Renderer renderer, Mesh mesh)
        {
            Name = renderer.gameObject.name;
            Group = BuildGroupName(renderer.transform);
            Mesh = mesh;
            Renderer = renderer;

            if (Application.isPlaying)
                renderer.forceRenderingOff = true;
        }

        public LayeredActorLayer (SpriteRenderer spriteRenderer) :
            this(spriteRenderer, BuildSpriteMesh(spriteRenderer))
        {
            this.spriteRenderer = spriteRenderer;
        }

        public void Dispose ()
        {
            if (Mesh && Mesh.hideFlags == HideFlags.HideAndDontSave)
                ObjectUtils.DestroyOrImmediate(Mesh);
        }

        public MaterialPropertyBlock GetPropertyBlock (MaterialPropertyBlock block)
        {
            Renderer.GetPropertyBlock(block);
            if (spriteRenderer)
                block.SetColor(spriteColorId, spriteRenderer.color);
            return block;
        }

        private static string BuildGroupName (Transform layerTransform)
        {
            var group = string.Empty;
            var transform = layerTransform.parent;
            while (transform && !transform.TryGetComponent<LayeredActorBehaviour>(out _))
            {
                group = transform.name + (string.IsNullOrEmpty(group) ? string.Empty : $"/{group}");
                transform = transform.parent;
            }
            return group;
        }

        private static Mesh BuildSpriteMesh (SpriteRenderer spriteRenderer)
        {
            var sprite = spriteRenderer.sprite;
            var mesh = new Mesh();
            mesh.hideFlags = HideFlags.HideAndDontSave;
            mesh.name = $"{sprite.name} Sprite Mesh";
            mesh.vertices = Array.ConvertAll(sprite.vertices, i => new Vector3(i.x * (spriteRenderer.flipX ? -1 : 1), i.y * (spriteRenderer.flipY ? -1 : 1)));
            mesh.uv = sprite.uv;
            mesh.triangles = Array.ConvertAll(sprite.triangles, i => (int)i);
            return mesh;
        }
    }
}
