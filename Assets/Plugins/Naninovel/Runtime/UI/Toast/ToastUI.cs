// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel.UI
{
    /// <inheritdoc cref="IToastUI"/>
    public class ToastUI : CustomUI, IToastUI
    {
        [Tooltip("The appearance used by default, when `appearance` parameter is not provided.")]
        [SerializeField] private ToastAppearance defaultAppearance = default;
        [Tooltip("Seconds to wait before hiding the toast; used by default, when `duration` parameter is not provided.")]
        [SerializeField] private float defaultDuration = 5f;

        private readonly Dictionary<string, ToastAppearance> appearances = new Dictionary<string, ToastAppearance>(StringComparer.OrdinalIgnoreCase);
        private Timer hideTimer;

        protected override void Awake ()
        {
            base.Awake();

            this.AssertRequiredObjects(defaultAppearance);

            hideTimer = new Timer(ignoreTimeScale: true, onCompleted: Hide);

            foreach (var appearance in GetComponentsInChildren<ToastAppearance>(true))
                appearances[appearance.name] = appearance;
        }

        public void Show (string text, string appearance = default, float? duration = default)
        {
            if (!TrySelectAppearance(appearance, out var selectedAppearance)) return;
            if (hideTimer.Running) hideTimer.Stop();
            selectedAppearance.SetText(text);
            hideTimer.Run(duration ?? defaultDuration, target: this);
            base.Show();
        }

        private bool TrySelectAppearance (string appearanceName, out ToastAppearance selectedAppearance)
        {
            var appearanceId = appearanceName ?? defaultAppearance.name;
            if (!appearances.TryGetValue(appearanceId, out selectedAppearance))
            {
                Debug.LogError($"Failed to show toast with `{appearanceId}` appearance: the appearance game object is not found under the toast prefab.");
                selectedAppearance = null;
                return false;
            }

            foreach (var toastAppearance in appearances.Values)
                if (toastAppearance != selectedAppearance)
                    toastAppearance.SetSelected(false);
            selectedAppearance.SetSelected(true);

            return true;
        }
    }
}
