// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    public enum CharacterLookDirection
    {
        Center,
        Left,
        Right
    }

    public static class CharacterLookDirectionExtensions
    {
        public static Vector2 ToVector2 (this CharacterLookDirection lookDirection)
        {
            switch (lookDirection)
            {
                case CharacterLookDirection.Left: return Vector2.left;
                case CharacterLookDirection.Right: return Vector2.right;
                default: return Vector2.zero;
            }
        }
    }
}
