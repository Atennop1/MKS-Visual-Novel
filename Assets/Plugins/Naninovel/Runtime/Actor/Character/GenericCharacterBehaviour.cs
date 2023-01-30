// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Hosts events routed by <see cref="GenericActor{TBehaviour,TMeta}"/>.
    /// </summary>
    public class GenericCharacterBehaviour : GenericActorBehaviour
    {
        [Serializable]
        private class LookDirectionChangedEvent : UnityEvent<CharacterLookDirection> { }

        /// <summary>
        /// Invoked when look direction of the character is changed.
        /// </summary>
        public event Action<CharacterLookDirection> OnLookDirectionChanged;
        /// <summary>
        /// Invoked when the character becomes or cease to be the author of the last printed text message.
        /// </summary>
        public event Action<bool> OnIsSpeakingChanged;

        public bool TransformByLookDirection => transformByLookDirection;
        public float LookDeltaAngle => lookDeltaAngle;

        [Tooltip("Invoked when look direction of the character is changed.")]
        [SerializeField] private LookDirectionChangedEvent onLookDirectionChanged = default;
        [Tooltip("Invoked when the character becomes the author of the printed text message.")]
        [SerializeField] private UnityEvent onStartedSpeaking = default;
        [Tooltip("Invoked after `On Started Speaking` when the message is fully revealed or (when auto voicing is enabled) voice clip finish playing.")]
        [SerializeField] private UnityEvent onFinishedSpeaking = default;
        [Tooltip("Whether to react to look direction changes by rotating the object's transform.")]
        [SerializeField] private bool transformByLookDirection = true;
        [Tooltip("When `" + nameof(transformByLookDirection) + "` is enabled, controls the rotation angle.")]
        [SerializeField] private float lookDeltaAngle = 30;

        public void NotifyLookDirectionChanged (CharacterLookDirection value)
        {
            OnLookDirectionChanged?.Invoke(value);
            onLookDirectionChanged?.Invoke(value);
        }

        public void NotifyIsSpeakingChanged (bool value)
        {
            OnIsSpeakingChanged?.Invoke(value);

            if (value) onStartedSpeaking?.Invoke();
            else onFinishedSpeaking?.Invoke();
        }
    }
}
