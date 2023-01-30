// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a <see cref="ToastUI"/> appearance.
    /// </summary>
    public class ToastAppearance : MonoBehaviour
    {
        [Serializable]
        private class TextChangedEvent : UnityEvent<string> { }

        [SerializeField] private TextChangedEvent onTextChanged = default;
        [SerializeField] private UnityEvent onSelected = default;
        [SerializeField] private UnityEvent onDeselected = default;

        public virtual void SetText (string text) => onTextChanged?.Invoke(text);

        public virtual void SetSelected (bool selected)
        {
            gameObject.SetActive(selected);
            if (selected) onSelected?.Invoke();
            else onDeselected?.Invoke();
        }
    }
}
