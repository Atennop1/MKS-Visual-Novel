// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Globalization;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Allows to play a <see cref="Script"/> or execute script commands via Unity API.
    /// </summary>
    public class PlayScript : MonoBehaviour
    {
        [Tooltip("The script asset to play.")]
        [ResourcePopup(ScriptsConfiguration.DefaultPathPrefix, ScriptsConfiguration.DefaultPathPrefix, emptyOption: "None (play script text)")]
        [SerializeField] private string scriptName = default;
        [TextArea(3, 10), Tooltip("The naninovel script text (commands) to execute; has no effect when `Script Name` is specified. Argument of the event (if any) can be referenced in the script text via `{arg}` expression. Conditional block commands (if, else, etc) are not supported.")]
        [SerializeField] private string scriptText = default;
        [Tooltip("Whether to automatically play the script when the game object is instantiated.")]
        [SerializeField] private bool playOnAwake = false;
        [Tooltip("Whether to disable waiting for input mode when the script is played.")]
        [SerializeField] private bool disableWaitInput = false;

        private string argument;

        public void Play ()
        {
            argument = null;
            PlayScriptAsync();
        }

        public void Play (string argument)
        {
            this.argument = argument;
            PlayScriptAsync();
        }

        public void Play (float argument)
        {
            this.argument = argument.ToString(CultureInfo.InvariantCulture);
            PlayScriptAsync();
        }

        public void Play (int argument)
        {
            this.argument = argument.ToString(CultureInfo.InvariantCulture);
            PlayScriptAsync();
        }

        public void Play (bool argument)
        {
            this.argument = argument.ToString(CultureInfo.InvariantCulture).ToLower();
            PlayScriptAsync();
        }

        private void Awake ()
        {
            if (playOnAwake) Play();
        }

        private async void PlayScriptAsync ()
        {
            if (!string.IsNullOrEmpty(scriptName))
            {
                var player = Engine.GetService<IScriptPlayer>();
                if (player is null) throw new InvalidOperationException($"Failed to play a script via `{gameObject.name}` game object: player service is not available. Make sure the engine is initialized.");
                await player.PreloadAndPlayAsync(scriptName);
                return;
            }

            if (disableWaitInput) Engine.GetService<IScriptPlayer>().SetWaitingForInputEnabled(false);

            if (!string.IsNullOrWhiteSpace(scriptText))
            {
                var text = string.IsNullOrEmpty(argument) ? scriptText : scriptText.Replace("{arg}", argument);
                var script = Script.FromScriptText($"`{name}` generated script", text);
                var playlist = new ScriptPlaylist(script);
                await playlist.ExecuteAsync();
            }
        }
    }
}
