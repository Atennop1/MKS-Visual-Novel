// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing a commentary left by the author of the script.
    /// </summary>
    [Serializable]
    public class CommentScriptLine : ScriptLine
    {
        /// <summary>
        /// Text contents of the commentary.
        /// </summary>
        public string CommentText => commentText;

        [SerializeField] private string commentText = default;

        public CommentScriptLine (string commentText, int lineIndex, string lineHash)
            : base(lineIndex, lineHash)
        {
            this.commentText = commentText;
        }
    }
}
