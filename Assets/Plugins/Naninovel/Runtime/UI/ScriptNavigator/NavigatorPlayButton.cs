// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel.UI
{
    public class NavigatorPlayButton : ScriptableButton
    {
        [Serializable]
        private class OnLabelChangedEvent : UnityEvent<string> { }

        [SerializeField] private OnLabelChangedEvent onLabelChanged = default;

        private ScriptNavigatorPanel navigator;
        private string scriptName;
        private IScriptPlayer player;
        private IStateManager stateManager;
        private bool isInitialized;

        public virtual void Initialize (ScriptNavigatorPanel navigator, string scriptName, IScriptPlayer player)
        {
            this.navigator = navigator;
            this.scriptName = scriptName;
            this.player = player;
            name = "PlayScript: " + scriptName;
            SetLabel(scriptName);
            isInitialized = true;
            UIComponent.interactable = true;
        }

        protected override void Awake ()
        {
            base.Awake();

            SetLabel(null);
            UIComponent.interactable = false;

            stateManager = Engine.GetService<IStateManager>();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            stateManager.GameSlotManager.OnBeforeLoad += ControlInteractability;
            stateManager.GameSlotManager.OnLoaded += ControlInteractability;
            stateManager.GameSlotManager.OnBeforeSave += ControlInteractability;
            stateManager.GameSlotManager.OnSaved += ControlInteractability;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            stateManager.GameSlotManager.OnBeforeLoad -= ControlInteractability;
            stateManager.GameSlotManager.OnLoaded -= ControlInteractability;
            stateManager.GameSlotManager.OnBeforeSave -= ControlInteractability;
            stateManager.GameSlotManager.OnSaved -= ControlInteractability;
        }

        protected override void OnButtonClick ()
        {
            Debug.Assert(isInitialized);
            navigator.Hide();
            Engine.GetService<IUIManager>()?.GetUI<ITitleUI>()?.Hide();
            PlayScriptAsync();
        }

        protected virtual void SetLabel (string value)
        {
            onLabelChanged?.Invoke(value);
        }

        private async void PlayScriptAsync ()
        {
            await stateManager.ResetStateAsync(() => player.PreloadAndPlayAsync(scriptName));
        }

        private void ControlInteractability ()
        {
            UIComponent.interactable = !stateManager.GameSlotManager.Loading && !stateManager.GameSlotManager.Saving;
        }
    }
}
