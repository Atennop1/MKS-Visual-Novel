// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="Script"/> line representing a text marker used to navigate within the script.
    /// </summary>
    [System.Serializable]
    public class LabelScriptLine : ScriptLine
    {
        /// <summary>
        /// Text contents of the label.
        /// </summary>
        public string LabelText => labelText;

        [SerializeField] private string labelText = default;

        public LabelScriptLine (string labelText, int lineIndex, string lineHash)
            : base(lineIndex, lineHash)
        {
            this.labelText = labelText;
        }
    }
}
