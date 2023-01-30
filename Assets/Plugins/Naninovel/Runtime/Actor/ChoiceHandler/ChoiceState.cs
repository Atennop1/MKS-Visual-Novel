// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable state of a choice in <see cref="ChoiceHandlerState"/>.
    /// </summary>
    [Serializable]
    public struct ChoiceState : IEquatable<ChoiceState>
    {
        /// <summary>
        /// GUID of the state object.
        /// </summary>
        public string Id => id;
        /// <summary>
        /// Text describing consequences of this choice.
        /// </summary>
        public string Summary => summary;
        /// <summary>
        /// Path (relative to a `Resources` folder) to a button prefab representing the choice.
        /// </summary>
        public string ButtonPath => buttonPath;
        /// <summary>
        /// Local position of the choice button inside the choice handler.
        /// </summary>
        public Vector2 ButtonPosition => buttonPosition;
        /// <summary>
        /// Whether to apply <see cref="ButtonPosition"/> (whether user provided a custom position in the script command).
        /// </summary>
        public bool OverwriteButtonPosition => overwriteButtonPosition;
        /// <summary>
        /// Script text to execute when the choice is selected.
        /// </summary>
        public string OnSelectScript => onSelectScript;
        /// <summary>
        /// Whether to continue playing next command when the choice is selected.
        /// </summary>
        public bool AutoPlay => autoPlay;

        [SerializeField] private string id;
        [SerializeField] private string summary;
        [SerializeField] private string buttonPath;
        [SerializeField] private Vector2 buttonPosition;
        [SerializeField] private bool overwriteButtonPosition;
        [SerializeField] private string onSelectScript;
        [SerializeField] private bool autoPlay;

        public ChoiceState (string summary = null, string buttonPath = null, Vector2? buttonPosition = null, 
            string onSelectScript = null, bool autoPlay = false)
        {
            this.id = Guid.NewGuid().ToString();
            this.summary = summary;
            this.buttonPath = buttonPath;
            this.buttonPosition = buttonPosition ?? default;
            this.overwriteButtonPosition = buttonPosition.HasValue;
            this.onSelectScript = onSelectScript;
            this.autoPlay = autoPlay;
        }
        
        public override bool Equals (object obj) => obj is ChoiceState state && Equals(state);

        public bool Equals (ChoiceState other) => id == other.id;

        public override int GetHashCode () => 1877310944 + EqualityComparer<string>.Default.GetHashCode(Id);

        public static bool operator == (ChoiceState left, ChoiceState right) => left.Equals(right);

        public static bool operator != (ChoiceState left, ChoiceState right) => !(left == right);
    }
}
