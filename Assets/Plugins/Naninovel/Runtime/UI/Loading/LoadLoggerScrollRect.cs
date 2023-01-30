// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Naninovel.UI
{
    public class LoadLoggerScrollRect : ScriptableUIComponent<ScrollRect>
    {
        protected Text LoggerText => loggerText;
        protected Text MemoryUsageText => memoryUsageText;

        [SerializeField] private Text loggerText = null;
        [SerializeField] private Text memoryUsageText = null;

        private IResourceProviderManager providerManager;

        public virtual async void Log (string message)
        {
            if (!providerManager.Configuration.LogResourceLoading) return;

            await AsyncUtils.WaitEndOfFrameAsync(); // Otherwise could get here inside a rebuild loop and Unity will become sad :(
            if (!Application.isPlaying) return;

            loggerText.text += message;
            loggerText.text += Environment.NewLine;
            UIComponent.verticalNormalizedPosition = 0;

            if (loggerText.text.Length > 10000) // UI.Text has char limit (depends on vertex count per char, 65k verts is the limit).
                loggerText.text = loggerText.text.GetAfterFirst(Environment.NewLine);
        }

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(loggerText);

            providerManager = Engine.GetService<IResourceProviderManager>();
            loggerText.text = string.Empty;
        }

        protected override void Start ()
        {
            base.Start();

            if (memoryUsageText)
                memoryUsageText.gameObject.SetActive(providerManager.Configuration.LogResourceLoading);

            if (providerManager.Configuration.LogResourceLoading)
                UpdateMemoryUsage();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            if (providerManager.Configuration.LogResourceLoading)
            {
                providerManager.OnProviderMessage += LogResourceProviderMessage;
                Application.logMessageReceived += LogDebug;
            }
        }

        protected override void OnDestroy ()
        {
            base.OnDestroy();

            if (providerManager != null)
                providerManager.OnProviderMessage -= LogResourceProviderMessage;
            Application.logMessageReceived -= LogDebug;
        }

        protected virtual void LogResourceProviderMessage (string message)
        {
            Log($"<color=lightblue>{message}</color>");
            UpdateMemoryUsage();

            //Debug.Log($"{message} {memoryUsageText.text}");
        }

        protected virtual void LogDebug (string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Assert || type == LogType.Error || type == LogType.Exception)
                Log($"<color=red>[System] {condition}</color>");
            else if (type == LogType.Warning)
                Log($"<color=yellow>[System] {condition}</color>");
            else Log($"[System] {condition}");
        }

        private void UpdateMemoryUsage ()
        {
            if (!memoryUsageText || !providerManager.Configuration.LogResourceLoading) return;
            memoryUsageText.text = $"<b>Total memory used: {StringUtils.FormatFileSize(Profiler.GetTotalAllocatedMemoryLong())}</b>";
        }
    }
}
