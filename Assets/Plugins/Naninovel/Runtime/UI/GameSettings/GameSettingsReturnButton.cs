// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    public class GameSettingsReturnButton : ScriptableButton
    {
        private GameSettingsMenu settingsMenu;
        private IStateManager settingsManager;

        protected override void Awake ()
        {
            base.Awake();

            settingsMenu = GetComponentInParent<GameSettingsMenu>();
            settingsManager = Engine.GetService<IStateManager>();
        }

        protected override void OnButtonClick () => ApplySettingsAsync();

        private async void ApplySettingsAsync ()
        {
            settingsMenu.SetInteractable(false);
            await settingsManager.SaveSettingsAsync();
            settingsMenu.SetInteractable(true);
            settingsMenu.Hide();
        }
    }
}
