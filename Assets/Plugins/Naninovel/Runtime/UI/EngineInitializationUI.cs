// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.Runtime.UI
{
    /// <summary>
    /// Fullscreen UI shown when the engine is initializing to mask the process.
    /// </summary>
    /// <remarks>
    /// Be careful when using engine API's here, as it's not yet initialized.
    /// </remarks>
    public class EngineInitializationUI : ScriptableUIBehaviour
    {
        [Tooltip("Event invoked when engine initialization progress is changed, in 0.0 to 1.0 range.")]
        [SerializeField] private FloatUnityEvent onInitializationProgress;

        protected override void OnEnable ()
        {
            base.OnEnable();
            Engine.OnInitializationProgress += NotifyProgressChanged;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();
            Engine.OnInitializationProgress -= NotifyProgressChanged;
        }

        protected virtual void NotifyProgressChanged (float value)
        {
            onInitializationProgress?.Invoke(value);
        }
    }
}
