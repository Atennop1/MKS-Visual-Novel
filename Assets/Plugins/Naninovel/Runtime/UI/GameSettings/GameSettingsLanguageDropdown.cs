// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Naninovel.UI
{
    public class GameSettingsLanguageDropdown : ScriptableDropdown
    {
        private const string tempSaveSlotId = "TEMP_LOCALE_CHANGE";

        private readonly Dictionary<int, string> optionToLocaleMap = new Dictionary<int, string>();
        private ILocalizationManager localizationManager;

        protected override void Awake ()
        {
            base.Awake();

            localizationManager = Engine.GetService<ILocalizationManager>();
            var availableLocales = localizationManager.GetAvailableLocales().ToList();
            InitializeOptions(availableLocales);
        }

        protected override void OnValueChanged (int value)
        {
            var selectedLocale = optionToLocaleMap[value];
            HandleLocaleChangedAsync(selectedLocale);
        }

        private void InitializeOptions (List<string> availableLocales)
        {
            optionToLocaleMap.Clear();
            for (int i = 0; i < availableLocales.Count; i++)
                optionToLocaleMap.Add(i, availableLocales[i]);

            UIComponent.ClearOptions();
            UIComponent.AddOptions(availableLocales.Select(LanguageTags.GetLanguageByTag).ToList());
            UIComponent.value = availableLocales.IndexOf(localizationManager.SelectedLocale);
            UIComponent.RefreshShownValue();
        }

        private async void HandleLocaleChangedAsync (string locale)
        {
            var clickThroughPanel = Engine.GetService<IUIManager>()?.GetUI<ClickThroughPanel>();
            if (clickThroughPanel != null) clickThroughPanel.Show(false, null);

            await localizationManager.SelectLocaleAsync(locale);
            
            var scriptPlayer = Engine.GetService<IScriptPlayer>();
            var scriptManager = Engine.GetService<IScriptManager>();
            if (scriptPlayer.PlayedScript != null)
            {
                var stateManager = Engine.GetService<IStateManager>();

                // Reload the game to start playing localized version of the scripts.
                await stateManager.SaveGameAsync(tempSaveSlotId);
                await stateManager.ResetStateAsync();
                await scriptManager.ReloadAllScriptsAsync();
                await stateManager.LoadGameAsync(tempSaveSlotId);
                stateManager.GameSlotManager.DeleteSaveSlot(tempSaveSlotId);
                
                // If possible, rollback to the start of the played line to localize the printed content.
                await stateManager.RollbackAsync(s => s.PlaybackSpot.InlineIndex == 0);
            }
            else await scriptManager.ReloadAllScriptsAsync();

            if (clickThroughPanel != null) clickThroughPanel.Hide();
        }
    }
}
