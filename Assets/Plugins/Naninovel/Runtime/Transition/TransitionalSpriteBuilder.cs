// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;
using UnityEngine;

namespace Naninovel
{
    public class TransitionalSpriteBuilder
    {
        private readonly Vector3[] vertices = new Vector3[4];
        private readonly Vector2[] uvs = { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        private readonly ushort[] triangles = { 0, 1, 2, 3, 2, 1 };
        private readonly Mesh mesh;

        public TransitionalSpriteBuilder (Mesh mesh)
        {
            this.mesh = mesh;
        }

        public void Build (Vector2 size, Vector2 pivot, int ppu)
        {
            BuildVertices(size, ppu);
            ApplyPivot(pivot);
            SetMeshData();
        }

        private void BuildVertices (Vector2 size, int ppu)
        {
            var quadHalfWidth = size.x * .5f / ppu;
            var quadHalfHeight = size.y * .5f / ppu;
            vertices[0] = new Vector3(-quadHalfWidth, -quadHalfHeight, 0);
            vertices[1] = new Vector3(-quadHalfWidth, quadHalfHeight, 0);
            vertices[2] = new Vector3(quadHalfWidth, -quadHalfHeight, 0);
            vertices[3] = new Vector3(quadHalfWidth, quadHalfHeight, 0);
        }

        private void ApplyPivot (Vector2 pivot)
        {
            var spriteRect = EvaluateSpriteRect();

            var curPivot = new Vector2(-spriteRect.min.x / spriteRect.size.x, -spriteRect.min.y / spriteRect.size.y);
            if (curPivot == pivot) return;

            var curDeltaX = spriteRect.size.x * curPivot.x;
            var curDeltaY = spriteRect.size.y * curPivot.y;
            var newDeltaX = spriteRect.size.x * pivot.x;
            var newDeltaY = spriteRect.size.y * pivot.y;

            var deltaPos = new Vector3(newDeltaX - curDeltaX, newDeltaY - curDeltaY);
            for (int i = 0; i < vertices.Length; i++)
                vertices[i] -= deltaPos;
        }

        private Rect EvaluateSpriteRect ()
        {
            var minVertPos = new Vector2(vertices.Min(v => v.x), vertices.Min(v => v.y));
            var maxVertPos = new Vector2(vertices.Max(v => v.x), vertices.Max(v => v.y));
            var spriteSizeX = Mathf.Abs(maxVertPos.x - minVertPos.x);
            var spriteSizeY = Mathf.Abs(maxVertPos.y - minVertPos.y);
            var spriteSize = new Vector2(spriteSizeX, spriteSizeY);
            return new Rect(minVertPos, spriteSize);
        }

        private void SetMeshData ()
        {
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(1, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.UploadMeshData(false);
        }
    }
}
