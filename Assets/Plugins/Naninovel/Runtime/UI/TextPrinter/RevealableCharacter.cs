// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel.UI
{
    public readonly struct RevealableCharacter : IEquatable<RevealableCharacter>
    {
        public static readonly RevealableCharacter Invalid = new RevealableCharacter(-1, -1, -1, -1, -1, -1);

        public readonly int CharIndex;
        public readonly int LineIndex;
        public readonly float Origin;
        public readonly float XAdvance;
        public readonly float SlantAngle;
        public readonly float BottomRightX;

        public RevealableCharacter (int charIndex, int lineIndex, float origin, float xAdvance, float slantAngle, float bottomRightX)
        {
            CharIndex = charIndex;
            LineIndex = lineIndex;
            Origin = origin;
            XAdvance = xAdvance;
            SlantAngle = slantAngle;
            BottomRightX = bottomRightX;
        }

        public bool Equals (RevealableCharacter other) => CharIndex == other.CharIndex && LineIndex == other.LineIndex;
        public override bool Equals (object obj) => obj is RevealableCharacter other && Equals(other);
        public static bool operator == (RevealableCharacter left, RevealableCharacter right) => left.Equals(right);
        public static bool operator != (RevealableCharacter left, RevealableCharacter right) => !left.Equals(right);
        public override int GetHashCode ()
        {
            unchecked
            {
                return (CharIndex * 397) ^ LineIndex;
            }
        }
    }
}
