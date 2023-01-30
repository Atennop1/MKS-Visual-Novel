// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class TitleContinueButton : ScriptableButton
    {
        private IStateManager gameState;
        private IUIManager uiManager;

        protected override void Awake ()
        {
            base.Awake();

            gameState = Engine.GetService<IStateManager>();
            uiManager = Engine.GetService<IUIManager>();
        }

        protected override void Start ()
        {
            base.Start();

            ControlInteractability();
        }

        protected override void OnEnable ()
        {
            base.OnEnable();

            gameState.GameSlotManager.OnSaved += ControlInteractability;
        }

        protected override void OnDisable ()
        {
            base.OnDisable();

            gameState.GameSlotManager.OnSaved -= ControlInteractability;
        }

        protected override void OnButtonClick ()
        {
            var saveLoadUI = uiManager.GetUI<ISaveLoadUI>();
            if (saveLoadUI is null) return;

            var lastLoadMode = saveLoadUI.GetLastLoadMode();
            saveLoadUI.PresentationMode = lastLoadMode;
            saveLoadUI.Show();
        }

        private void ControlInteractability () => UIComponent.interactable = gameState.AnyGameSaveExists;
    }
}
