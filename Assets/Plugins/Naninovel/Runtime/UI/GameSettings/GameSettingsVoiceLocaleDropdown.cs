// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;

namespace Naninovel.UI
{
    public class GameSettingsVoiceLocaleDropdown : ScriptableDropdown
    {
        private readonly List<string> options = new List<string>();
        private IAudioManager audioManager;

        protected override void Awake ()
        {
            base.Awake();

            audioManager = Engine.GetService<IAudioManager>();
            
            InitializeOptions();
            if (options.Count == 0) 
                transform.parent.gameObject.SetActive(false);
        }

        protected override void OnValueChanged (int value)
        {
            var selectedLocale = options[value];
            audioManager.VoiceLocale = selectedLocale;
        }

        private void InitializeOptions ()
        {
            options.Clear();
            var voiceLocales = Engine.GetConfiguration<AudioConfiguration>().VoiceLocales;
            if (voiceLocales?.Count > 0)
                options.AddRange(voiceLocales);
            else return;

            UIComponent.ClearOptions();
            UIComponent.AddOptions(voiceLocales.Select(LanguageTags.GetLanguageByTag).ToList());
            UIComponent.value = options.IndexOf(audioManager.VoiceLocale);
            UIComponent.RefreshShownValue();
        }
    }
}
