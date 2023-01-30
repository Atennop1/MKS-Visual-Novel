// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents position of a playable element in a <see cref="Script"/>.
    /// </summary>
    [Serializable]
    public struct PlaybackSpot : IEquatable<PlaybackSpot>
    {
        /// <summary>
        /// An uninitialized playback spot, that doesn't belong to any script asset.
        /// </summary>
        public static readonly PlaybackSpot Invalid = new PlaybackSpot(null, -1, -1);

        /// <summary>
        /// Whether the spot is initialized and belong to an actual script asset.
        /// </summary>
        public bool Valid => !string.IsNullOrEmpty(scriptName) && lineIndex >= 0 && inlineIndex >= 0;
        public string ScriptName => scriptName;
        public int LineIndex => lineIndex;
        public int LineNumber => lineIndex + 1;
        public int InlineIndex => inlineIndex;

        [SerializeField] private string scriptName;
        [SerializeField] private int lineIndex;
        [SerializeField] private int inlineIndex;

        public PlaybackSpot (string scriptName, int lineIndex, int inlineIndex)
        {
            this.scriptName = scriptName;
            this.lineIndex = lineIndex;
            this.inlineIndex = inlineIndex;
        }

        public override bool Equals (object obj)
        {
            return obj is PlaybackSpot spot && Equals(spot);
        }

        public bool Equals (PlaybackSpot other)
        {
            return scriptName == other.scriptName &&
                   lineIndex == other.lineIndex &&
                   inlineIndex == other.inlineIndex;
        }

        public override int GetHashCode ()
        {
            var hashCode = 646664838;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ScriptName);
            hashCode = hashCode * -1521134295 + LineIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + InlineIndex.GetHashCode();
            return hashCode;
        }

        public static bool operator == (PlaybackSpot left, PlaybackSpot right)
        {
            return left.Equals(right);
        }

        public static bool operator != (PlaybackSpot left, PlaybackSpot right)
        {
            return !(left == right);
        }

        public override string ToString () => $"{ScriptName} #{LineNumber}.{InlineIndex}";
    }
}
