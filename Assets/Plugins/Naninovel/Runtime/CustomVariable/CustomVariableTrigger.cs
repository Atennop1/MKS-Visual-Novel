// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Allows to listen for events when value of a custom state variable with specific name is changed.
    /// </summary>
    public class CustomVariableTrigger : MonoBehaviour
    {
        [Serializable]
        private class VariableValueChangedEvent : UnityEvent<string> { }
        [Serializable]
        private class FloatVariableValueChangedEvent : UnityEvent<float> { }
        [Serializable]
        private class IntVariableValueChangedEvent : UnityEvent<int> { }
        [Serializable]
        private class BoolVariableValueChangedEvent : UnityEvent<bool> { }

        /// <summary>
        /// Invoked when value of the listened variable is changed.
        /// </summary>
        public event Action<string> OnVariableValueChanged;

        /// <summary>
        /// Name of a custom state variable to listen for.
        /// </summary>
        public string CustomVariableName { get => customVariableName; set => customVariableName = value; }
        /// <summary>
        /// Attempts to retrieve current value of the listened variable.
        /// </summary>
        public string CustomVariableValue => variableManager?.GetVariableValue(CustomVariableName);

        [Tooltip("Name of a custom state variable to listen for.")]
        [SerializeField] private string customVariableName = default;
        [Tooltip("Invoked when value of a custom variable with specified name is changed; also invoked when the component is started.")]
        [SerializeField] private VariableValueChangedEvent onVariableValueChanged = default;
        [Tooltip("Invoked when value of a custom variable with specified name is changed and the value is a float; also invoked when the component is started.")]
        [SerializeField] private FloatVariableValueChangedEvent onFloatVariableValueChanged = default;
        [Tooltip("Invoked when value of a custom variable with specified name is changed and the value is an integer; also invoked when the component is started.")]
        [SerializeField] private IntVariableValueChangedEvent onIntVariableValueChanged = default;
        [Tooltip("Invoked when value of a custom variable with specified name is changed and the value is a boolean; also invoked when the component is started.")]
        [SerializeField] private BoolVariableValueChangedEvent onBoolVariableValueChanged = default;

        private ICustomVariableManager variableManager;
        private IStateManager stateManager;

        private void Awake ()
        {
            variableManager = Engine.GetService<ICustomVariableManager>();
            stateManager = Engine.GetService<IStateManager>();
        }

        private void OnEnable ()
        {
            variableManager.OnVariableUpdated += HandleVariableUpdated;
            stateManager.AddOnGameDeserializeTask(HandleGameDeserialized);
        }

        private void OnDisable ()
        {
            if (variableManager != null)
                variableManager.OnVariableUpdated -= HandleVariableUpdated;
            stateManager?.RemoveOnGameDeserializeTask(HandleGameDeserialized);
        }

        private void Start ()
        {
            OnVariableValueChanged?.Invoke(CustomVariableValue);
            onVariableValueChanged?.Invoke(CustomVariableValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && ParseUtils.TryInvariantFloat(CustomVariableValue, out var floatValue))
                onFloatVariableValueChanged?.Invoke(floatValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && ParseUtils.TryInvariantInt(CustomVariableValue, out var intValue))
                onIntVariableValueChanged?.Invoke(intValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && bool.TryParse(CustomVariableValue, out var boolValue))
                onBoolVariableValueChanged?.Invoke(boolValue);
        }

        private void HandleVariableUpdated (CustomVariableUpdatedArgs args)
        {
            if (!args.Name.EqualsFastIgnoreCase(CustomVariableName)) return;

            OnVariableValueChanged?.Invoke(args.Value);
            onVariableValueChanged?.Invoke(args.Value);
            if (!string.IsNullOrEmpty(args.Value) && ParseUtils.TryInvariantFloat(args.Value, out var floatValue))
                onFloatVariableValueChanged?.Invoke(floatValue);
            if (!string.IsNullOrEmpty(args.Value) && ParseUtils.TryInvariantInt(args.Value, out var intValue))
                onIntVariableValueChanged?.Invoke(intValue);
            if (!string.IsNullOrEmpty(args.Value) && bool.TryParse(args.Value, out var boolValue))
                onBoolVariableValueChanged?.Invoke(boolValue);
        }

        private UniTask HandleGameDeserialized (GameStateMap state)
        {
            OnVariableValueChanged?.Invoke(CustomVariableValue);
            onVariableValueChanged?.Invoke(CustomVariableValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && ParseUtils.TryInvariantFloat(CustomVariableValue, out var floatValue))
                onFloatVariableValueChanged?.Invoke(floatValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && ParseUtils.TryInvariantInt(CustomVariableValue, out var intValue))
                onIntVariableValueChanged?.Invoke(intValue);
            if (!string.IsNullOrEmpty(CustomVariableValue) && bool.TryParse(CustomVariableValue, out var boolValue))
                onBoolVariableValueChanged?.Invoke(boolValue);
            return UniTask.CompletedTask;
        }
    }
}
