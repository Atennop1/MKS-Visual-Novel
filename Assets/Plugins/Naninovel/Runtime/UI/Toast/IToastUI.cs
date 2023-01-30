// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is a general-purpose UI for self-hiding popup notifications aka "toasts".
    /// </summary>
    public interface IToastUI : IManagedUI
    {
        /// <summary>
        /// Shows the UI with the provided text and (optionally) appearance and duration.
        /// The UI is automatically hidden after the specified (or default) duration.
        /// </summary>
        /// <param name="text">The text content to set for the toast.</param>
        /// <param name="appearance">Appearance variant of the toast; default is used when not specified.</param>
        /// <param name="duration">Seconds to wait before hiding the toast; default is used when not specified.</param>
        void Show (string text, string appearance = default, float? duration = default);
    }
}
