// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to gradually reveal text.
    /// </summary>
    public interface IRevealableText
    {
        /// <summary>
        /// Text to reveal.
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Text color.
        /// </summary>
        Color TextColor { get; set; }
        /// <summary>
        /// Object that hosts the implementation.
        /// </summary>
        GameObject GameObject { get; }
        /// <summary>
        /// Progress (in 0.0 to 1.0 range) of the <see cref="Text"/> reveal process.
        /// </summary>
        float RevealProgress { get; set; }
        /// <summary>
        /// Whether currently revealing characters.
        /// </summary>
        bool Revealing { get; }

        /// <summary>
        /// Reveals next <paramref name="count"/> visible (formatting tags don't count) 
        /// <see cref="Text"/> characters over <paramref name="duration"/> (in seconds, per character).
        /// </summary>
        /// <param name="count">Number of characters to reveal.</param>
        /// <param name="duration">Duration of the reveal per character, in seconds.</param>
        /// <param name="asyncToken">The reveal should be canceled ASAP when requested.</param>
        void RevealNextChars (int count, float duration, AsyncToken asyncToken);
        /// <summary>
        /// Returns position (in world space) of the last revealed <see cref="Text"/> character.
        /// </summary>
        Vector2 GetLastRevealedCharPosition ();
        /// <summary>
        /// Returns last revealed visible (excluding formatting tags) <see cref="Text"/> character.
        /// </summary>
        char GetLastRevealedChar ();
        /// <summary>
        /// Renders the reveal effect.
        /// </summary>
        void Render ();
    }
}
