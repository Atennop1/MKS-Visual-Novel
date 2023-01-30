// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Hosts events routed by <see cref="GenericActor{TBehaviour,TMeta}"/>.
    /// </summary>
    public abstract class GenericActorBehaviour : MonoBehaviour
    {
        [System.Serializable]
        private class AppearanceChangedEvent : UnityEvent<string> { }
        [System.Serializable]
        private class VisibilityChangedEvent : UnityEvent<bool> { }
        [System.Serializable]
        private class TintColorChangedEvent : UnityEvent<Color> { }

        /// <summary>
        /// Invoked when appearance of the actor is changed.
        /// </summary>
        public event Action<string> OnAppearanceChanged;
        /// <summary>
        /// Invoked when visibility of the actor is changed.
        /// </summary>
        public event Action<bool> OnVisibilityChanged;
        /// <summary>
        /// Invoked when tint color of the actor is changed.
        /// </summary>
        public event Action<Color> OnTintColorChanged;

        [Tooltip("Invoked when appearance of the actor is changed.")]
        [SerializeField] private AppearanceChangedEvent onAppearanceChanged = default;
        [Tooltip("Invoked when visibility of the actor is changed.")]
        [SerializeField] private VisibilityChangedEvent onVisibilityChanged = default;
        [Tooltip("Invoked when tint color of the actor is changed.")]
        [SerializeField] private TintColorChangedEvent onTintColorChanged = default;

        public void InvokeAppearanceChangedEvent (string value)
        {
            OnAppearanceChanged?.Invoke(value);
            onAppearanceChanged?.Invoke(value);
        }

        public void InvokeVisibilityChangedEvent (bool value)
        {
            OnVisibilityChanged?.Invoke(value);
            onVisibilityChanged?.Invoke(value);
        }

        public void InvokeTintColorChangedEvent (Color value)
        {
            OnTintColorChanged?.Invoke(value);
            onTintColorChanged?.Invoke(value);
        }
    }
}
