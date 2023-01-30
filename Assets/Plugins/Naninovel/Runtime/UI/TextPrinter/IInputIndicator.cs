// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to indicate <see cref="IScriptPlayer.WaitingForInput"/>.
    /// </summary>
    public interface IInputIndicator
    {
        /// <summary>
        /// Whether the indicator is currently visible.
        /// </summary>
        bool Visible { get; }
        /// <summary>
        /// Transform of the indicator's game object.
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// Shows the indicator.
        /// </summary>
        void Show ();
        /// <summary>
        /// Hides the indicator.
        /// </summary>
        void Hide ();
    }
}
