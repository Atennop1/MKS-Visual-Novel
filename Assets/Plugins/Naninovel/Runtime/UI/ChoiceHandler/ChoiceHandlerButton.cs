// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Naninovel.UI
{
    [RequireComponent(typeof(Button))]
    public class ChoiceHandlerButton : ScriptableButton
    {
        [Serializable]
        private class SummaryTextChangedEvent : UnityEvent<string> { }

        /// <summary>
        /// Invoked when the choice summary text is changed.
        /// </summary>
        public event Action<string> OnSummaryTextChanged;

        public ChoiceState ChoiceState { get; private set; }

        [Tooltip("Invoked when the choice summary text is changed.")]
        [SerializeField] private SummaryTextChangedEvent onSummaryTextChanged = default;

        public virtual void Initialize (ChoiceState choiceState)
        {
            ChoiceState = choiceState;

            OnSummaryTextChanged?.Invoke(choiceState.Summary);
            onSummaryTextChanged?.Invoke(choiceState.Summary);
        }
    }
}
