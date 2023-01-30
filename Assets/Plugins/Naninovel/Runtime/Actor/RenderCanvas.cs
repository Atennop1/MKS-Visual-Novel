// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows specifying a canvas of arbitrary size and offset from the transform origin.
    /// </summary>
    public class RenderCanvas : MonoBehaviour
    {
        public Vector2 Size = Vector2.one;
        public Vector2 Offset = Vector2.zero;
        public Rect Rect => new Rect(Offset, Size);

        private void OnDrawGizmos ()
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)Offset, Size);
        }
    }
}
