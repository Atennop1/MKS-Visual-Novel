// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel.UI
{
    public readonly struct RevealableLine : IEquatable<RevealableLine>
    {
        public static readonly RevealableLine Invalid = new RevealableLine(-1, -1, -1, -1, -1);
        
        public readonly int Index;
        public readonly float Height;
        public readonly float Ascender;
        public readonly int FirstCharIndex;
        public readonly int LastCharIndex;
        
        public RevealableLine (int index, float height, float ascender, int firstCharIndex, int lastCharIndex)
        {
            Index = index;
            Height = height;
            Ascender = ascender;
            FirstCharIndex = firstCharIndex;
            LastCharIndex = lastCharIndex;
        }

        public bool Equals (RevealableLine other) => Index == other.Index;
        public override bool Equals (object obj) => obj is RevealableLine other && Equals(other);
        public override int GetHashCode () => Index;
        public static bool operator == (RevealableLine left, RevealableLine right) => left.Equals(right);
        public static bool operator != (RevealableLine left, RevealableLine right) => !left.Equals(right);
    }
}
