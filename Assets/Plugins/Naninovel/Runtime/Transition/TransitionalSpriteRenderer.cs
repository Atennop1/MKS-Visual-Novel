// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="TransitionalRenderer"/> implementation, that outputs the result to a quad mesh (sprite).
    /// </summary>
    public class TransitionalSpriteRenderer : TransitionalRenderer
    {
        public override Vector2 Pivot { get => pivot; set { if (value != Pivot) { pivot = value; BuildMesh(); } } }
        public virtual int PixelsPerUnit { get => pixelsPerUnit; set { if (value != PixelsPerUnit) { pixelsPerUnit = value; BuildMesh(); } } }
        public virtual Rect Bounds => spriteMesh != null ? new Rect(spriteMesh.bounds.min, spriteMesh.bounds.size) : default;
        public virtual bool DepthPassEnabled { get; set; }
        public virtual float DepthAlphaCutoff { get => depthMaterial.GetFloat(depthCutoffId); set => depthMaterial.SetFloat(depthCutoffId, value); }

        private const string defaultSpriteShaderName = "Hidden/Naninovel/Transparent";
        private const string depthShaderName = "Hidden/Naninovel/DepthMask";
        private static readonly int depthCutoffId = Shader.PropertyToID("_DepthAlphaCutoff");
        private static readonly int opacityId = Shader.PropertyToID("_Opacity");

        private TransitionalSpriteBuilder spriteBuilder;
        private Mesh spriteMesh;
        private Material renderMaterial;
        private Material depthMaterial;
        private RenderTexture renderTexture;
        private Vector2 pivot;
        private int pixelsPerUnit;

        /// <inheritdoc cref="TransitionalRenderer.Initialize"/>
        /// <param name="pivot">Pivot (anchors) of the sprite.</param>
        /// <param name="pixelsPerUnit">How many texture pixels correspond to one unit of the sprite geometry.</param>
        public virtual void Initialize (Vector2 pivot, int pixelsPerUnit, bool premultipliedAlpha, 
            Shader customShader = default, Shader customSpriteShader = default)
        {
            base.Initialize(premultipliedAlpha, customShader);

            this.pivot = pivot;
            this.pixelsPerUnit = pixelsPerUnit;

            spriteMesh = gameObject.AddComponent<MeshFilter>().sharedMesh = new Mesh();
            spriteMesh.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
            spriteMesh.name = "Transitional Sprite Mesh";
            spriteMesh.MarkDynamic();
            spriteBuilder = new TransitionalSpriteBuilder(spriteMesh);

            renderMaterial = new Material(customSpriteShader ? customSpriteShader : Shader.Find(defaultSpriteShaderName));
            renderMaterial.hideFlags = HideFlags.HideAndDontSave;

            depthMaterial = new Material(Shader.Find(depthShaderName));
            depthMaterial.hideFlags = HideFlags.HideAndDontSave;
        }

        protected virtual void Update ()
        {
            if (!ShouldRender()) return;
            renderMaterial.SetFloat(opacityId, TintColor.a);
            PrepareRenderTexture();
            RenderToTexture(renderTexture);
            var matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Graphics.DrawMesh(spriteMesh, matrix, renderMaterial, gameObject.layer);
            if (DepthPassEnabled) Graphics.DrawMesh(spriteMesh, matrix, depthMaterial, gameObject.layer);
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();
            if (renderTexture) RenderTexture.ReleaseTemporary(renderTexture);
            ObjectUtils.DestroyOrImmediate(renderMaterial);
            ObjectUtils.DestroyOrImmediate(depthMaterial);
        }

        protected virtual Vector2Int GetMeshSize ()
        {
            if (!TransitionTexture) return new Vector2Int(MainTexture.width, MainTexture.height);
            var width = Mathf.Max(TransitionTexture.width, MainTexture.width);
            var height = Mathf.Max(TransitionTexture.height, MainTexture.height);
            return new Vector2Int(width, height);
        }

        private void PrepareRenderTexture ()
        {
            var size = GetMeshSize();
            if (renderTexture && renderTexture.width == size.x && renderTexture.height == size.y) return;
            if (renderTexture) RenderTexture.ReleaseTemporary(renderTexture);
            renderTexture = RenderTexture.GetTemporary(size.x, size.y);
            renderMaterial.mainTexture = renderTexture;
            depthMaterial.mainTexture = renderTexture;
            BuildMesh();
        }

        private void BuildMesh ()
        {
            if (!MainTexture) return;
            spriteBuilder.Build(GetMeshSize(), Pivot, PixelsPerUnit);
        }
    } 
}
