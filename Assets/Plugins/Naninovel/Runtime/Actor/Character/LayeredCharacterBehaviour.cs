// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Represent a prefab resource used by <see cref="LayeredCharacter"/> actors.
    /// </summary>
    public class LayeredCharacterBehaviour : LayeredActorBehaviour
    {
        /// <summary>
        /// Invoked when the character becomes or cease to be the author of the last printed text message.
        /// </summary>
        public event Action<bool> OnIsSpeakingChanged;

        [Tooltip("Invoked when the character becomes the author of the printed text message.")]
        [SerializeField] private UnityEvent onStartedSpeaking = default;
        [Tooltip("Invoked after `On Started Speaking` when the message is fully revealed or (when auto voicing is enabled) voice clip finish playing.")]
        [SerializeField] private UnityEvent onFinishedSpeaking = default;

        public void NotifyIsSpeakingChanged (bool value)
        {
            OnIsSpeakingChanged?.Invoke(value);

            if (value) onStartedSpeaking?.Invoke();
            else onFinishedSpeaking?.Invoke();
        }
    }
}
