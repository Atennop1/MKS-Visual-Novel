// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Allows providing managed text values to game objects via Unity events.
    /// When attached to a managed UI, text printer or choice handler
    /// generated managed text documents will automatically include corresponding record.
    /// </summary>
    public class ManagedTextProvider : MonoBehaviour
    {
        [System.Serializable]
        private class ValueChangedEvent : UnityEvent<string> { }

        public string Category => category;
        public string Key => string.IsNullOrWhiteSpace(key) ? gameObject.name : key;
        public string DefaultValue => defaultValue;

        [Tooltip("Name of the managed text document, which contains tracked managed text record.")]
        [SerializeField] private string category = ManagedTextRecord.DefaultCategoryName;
        [Tooltip("ID of the tracked managed text record; when not provided (empty), will use name of the game object to which the component is attached.")]
        [SerializeField] private string key = default;
        [Tooltip("Default value to use when the tracked record is missing.")]
        [SerializeField] private string defaultValue = default;
        [Tooltip("Invoked when value of the tracked managed text record is changed (eg, when switching localization); also invoked when the engine is initialized.")]
        [SerializeField] private ValueChangedEvent onValueChanged = default;

        private ILocalizationManager localizationManager;
        private ITextManager textManager;

        private void OnEnable ()
        {
            if (Engine.Initialized) HandleEngineInitialized();
            else Engine.OnInitializationFinished += HandleEngineInitialized;
        }

        private void OnDisable ()
        {
            if (localizationManager != null)
                localizationManager.OnLocaleChanged -= HandleLocalizationChanged;
            Engine.OnInitializationFinished -= HandleEngineInitialized;
        }

        private void HandleEngineInitialized ()
        {
            Engine.OnInitializationFinished -= HandleEngineInitialized;

            textManager = Engine.GetService<ITextManager>();
            localizationManager = Engine.GetService<ILocalizationManager>();
            localizationManager.OnLocaleChanged += HandleLocalizationChanged;

            InvokeValueChanged();
        }

        private void HandleLocalizationChanged (string locale) => InvokeValueChanged();

        private void InvokeValueChanged ()
        {
            var recordKey = string.IsNullOrWhiteSpace(key) ? gameObject.name : key;
            var value = textManager.GetRecordValue(recordKey, category);
            onValueChanged?.Invoke(value ?? DefaultValue);
        }
    }
}
