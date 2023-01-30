// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class LoadingPanel : CustomUI, ILoadingUI
    {
        [Tooltip("Event invoked when script preload progress is changed, in 0.0 to 1.0 range.")]
        [SerializeField] private FloatUnityEvent onProgressChanged;

        private IStateManager stateManager;
        private IScriptPlayer scriptPlayer;

        protected override void Awake ()
        {
            base.Awake();

            stateManager = Engine.GetService<IStateManager>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.OnGameLoadStarted += HandleLoadStarted;
            stateManager.OnGameLoadFinished += HandleLoadFinished;
            stateManager.OnResetStarted += Show;
            stateManager.OnResetFinished += Hide;
            scriptPlayer.OnPreloadProgress += HandleProgressChanged;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            if (stateManager != null)
            {
                stateManager.OnGameLoadStarted -= HandleLoadStarted;
                stateManager.OnGameLoadFinished -= HandleLoadFinished;
                stateManager.OnResetStarted -= Show;
                stateManager.OnResetFinished -= Hide;
            }

            if (scriptPlayer != null)
                scriptPlayer.OnPreloadProgress -= HandleProgressChanged;
        }

        protected override void HandleVisibilityChanged (bool visible)
        {
            base.HandleVisibilityChanged(visible);
            onProgressChanged?.Invoke(0);
        }

        protected virtual void HandleLoadStarted (GameSaveLoadArgs args) => Show();
        protected virtual void HandleLoadFinished (GameSaveLoadArgs args) => Hide();
        protected virtual void HandleProgressChanged (float value) => onProgressChanged?.Invoke(value);
    }
}
