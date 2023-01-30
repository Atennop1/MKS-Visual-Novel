// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class GameSettingsVoiceVolumeSlider : ScriptableSlider
    {
        [Tooltip("When provided, the slider will control voice volume of the printed message author (character actor) with the provided ID. When empty will control voice mixer group volume.")]
        [SerializeField] private string authorId = default;

        private IAudioManager audioManager;

        protected override void Awake ()
        {
            base.Awake();

            audioManager = Engine.GetService<IAudioManager>();
        }

        protected override void Start ()
        {
            base.Start();

            var authorVolume = audioManager.GetAuthorVolume(authorId);
            UIComponent.value = Mathf.Approximately(authorVolume, -1) ? audioManager.VoiceVolume : authorVolume;
        }

        protected override void OnValueChanged (float value)
        {
            if (string.IsNullOrEmpty(authorId))
                audioManager.VoiceVolume = value;
            else audioManager.SetAuthorVolume(authorId, value);
        }
    }
}
