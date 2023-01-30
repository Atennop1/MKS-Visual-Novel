// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Naninovel
{
    public class LayeredDrawer : IDisposable
    {
        public IReadOnlyCollection<LayeredActorLayer> Layers => layers;

        private readonly Transform transform;
        private readonly Material sharedMaterial;
        private readonly bool reversed;
        private readonly List<LayeredActorLayer> layers = new List<LayeredActorLayer>();
        private readonly CommandBuffer commandBuffer = new CommandBuffer();
        private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        private RenderCanvas renderCanvas;
        private Rect canvasRect;

        public LayeredDrawer (Transform transform, Material sharedMaterial = default, bool reversed = false)
        {
            this.transform = transform;
            this.sharedMaterial = sharedMaterial;
            this.reversed = reversed;
            commandBuffer.name = $"Naninovel-DrawLayered-{transform.name}";
            BuildLayers();
        }

        public void Dispose () => ClearLayers();

        public void BuildLayers ()
        {
            ClearLayers();

            var renderers = GetRenderers();
            if (renderers.Count == 0) return;

            foreach (var renderer in renderers)
            {
                if (renderer is SpriteRenderer spriteRenderer)
                {
                    if (!spriteRenderer.sprite) continue;
                    layers.Add(new LayeredActorLayer(spriteRenderer));
                    continue;
                }

                if (!renderer.TryGetComponent<MeshFilter>(out var meshFilter)) continue;
                layers.Add(new LayeredActorLayer(renderer, meshFilter.sharedMesh ? meshFilter.sharedMesh : meshFilter.mesh));
            }

            UpdateCanvas();
        }

        public virtual RenderTexture DrawLayers (int pixelsPerUnit, RenderTexture renderTexture = default)
        {
            if (layers is null || layers.Count == 0)
                throw new Exception($"Can't render layered actor `{transform.name}`: layers data is empty. Make sure the actor prefab contains child objects with at least one renderer.");

            var drawDimensions = canvasRect.size * pixelsPerUnit;
            var drawPosition = (Vector2)transform.position + canvasRect.position;
            var orthoMin = -drawDimensions / 2f + drawPosition * pixelsPerUnit;
            var orthoMax = drawDimensions / 2f + drawPosition * pixelsPerUnit;
            var orthoMatrix = Matrix4x4.Ortho(orthoMin.x, orthoMax.x, orthoMin.y, orthoMax.y, float.MinValue, float.MaxValue);
            renderTexture = PrepareRenderTexture(renderTexture, drawDimensions);
            PrepareCommandBuffer(renderTexture, orthoMatrix);

            if (reversed)
            {
                for (int i = layers.Count - 1; i >= 0; i--)
                    DrawLayer(layers[i], pixelsPerUnit);
            }
            else
            {
                for (int i = 0; i < layers.Count; i++)
                    DrawLayer(layers[i], pixelsPerUnit);
            }

            Graphics.ExecuteCommandBuffer(commandBuffer);

            return renderTexture;
        }

        public void DrawGizmos ()
        {
            if (renderCanvas) return; // Render canvas draws its own gizmo.
            if (!Application.isPlaying)
            {
                if (CountRenderers() != layers.Count) BuildLayers();
                else UpdateCanvas();
            }
            Gizmos.DrawWireCube(transform.position + (Vector3)canvasRect.position, canvasRect.size);
        }

        private void ClearLayers ()
        {
            foreach (var layer in layers)
                layer.Dispose();
            layers.Clear();
        }

        private IReadOnlyCollection<Renderer> GetRenderers ()
        {
            return transform.GetComponentsInChildren<Renderer>()
                .OrderBy(s => s.sortingOrder)
                .ThenByDescending(s => s.transform.position.z).ToArray();
        }

        private void UpdateCanvas ()
        {
            if (transform.TryGetComponent<RenderCanvas>(out renderCanvas))
                canvasRect = renderCanvas.Rect;
            else canvasRect = EvaluateCanvasRect();
        }

        private Rect EvaluateCanvasRect ()
        {
            if (layers is null || layers.Count == 0) return Rect.zero;

            float minX = float.PositiveInfinity, minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity, maxY = float.NegativeInfinity;
            foreach (var layer in layers)
            {
                var min = layer.Renderer.bounds.min - transform.position;
                var max = layer.Renderer.bounds.max - transform.position;
                if (min.x < minX) minX = min.x;
                if (min.y < minY) minY = min.y;
                if (max.x > maxX) maxX = max.x;
                if (max.y > maxY) maxY = max.y;
            }
            var offset = new Vector2(minX + maxX, minY + maxY) / 2;
            var size = new Vector2(maxX - minX, maxY - minY);
            return new Rect(offset, size);
        }

        private RenderTexture PrepareRenderTexture (RenderTexture renderTexture, Vector2 drawDimensions)
        {
            var width = Mathf.RoundToInt(drawDimensions.x);
            var height = Mathf.RoundToInt(drawDimensions.y);
            if (!renderTexture || renderTexture.width != width || renderTexture.height != height)
                return RenderTexture.GetTemporary(width, height);
            return renderTexture;
        }

        private void PrepareCommandBuffer (RenderTexture renderTexture, Matrix4x4 orthoMatrix)
        {
            commandBuffer.Clear();
            commandBuffer.SetRenderTarget(renderTexture);
            commandBuffer.ClearRenderTarget(true, true, Color.clear);
            commandBuffer.SetProjectionMatrix(orthoMatrix);
        }

        private void DrawLayer (LayeredActorLayer layer, int pixelsPerUnit)
        {
            if (!layer.Enabled) return;

            var drawMaterial = sharedMaterial ? sharedMaterial : layer.Renderer.sharedMaterial;
            var drawTransform = Matrix4x4.TRS(layer.Position * pixelsPerUnit, layer.Rotation, layer.Scale * pixelsPerUnit);
            layer.GetPropertyBlock(propertyBlock);
            commandBuffer.DrawMesh(layer.Mesh, drawTransform, drawMaterial, 0, -1, propertyBlock);
        }

        private int CountRenderers ()
        {
            var result = 0;
            CountIn(transform);
            return result;

            void CountIn (Transform trs)
            {
                for (int i = 0; i < trs.childCount; i++)
                {
                    var child = trs.GetChild(i);
                    if (child.TryGetComponent<SpriteRenderer>(out _) ||
                        child.TryGetComponent<MeshFilter>(out _)) result++;
                    CountIn(child);
                }
            }
        }
    }
}
