// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;

namespace Naninovel
{
    public class CharacterLipSyncer : IDisposable
    {
        public bool SyncAllowed { get; set; } = true;

        private readonly string authorId;
        private readonly Action<bool> setIsSpeaking;
        private readonly ITextPrinterManager textPrinterManager;
        private readonly IAudioManager audioManager;

        public CharacterLipSyncer (string authorId, Action<bool> setIsSpeaking)
        {
            this.authorId = authorId;
            this.setIsSpeaking = setIsSpeaking;
            audioManager = Engine.GetService<IAudioManager>();
            textPrinterManager = Engine.GetService<ITextPrinterManager>();
            textPrinterManager.OnPrintTextStarted += HandlePrintTextStarted;
            setIsSpeaking.Invoke(false);
        }

        public void Dispose ()
        {
            if (textPrinterManager != null)
            {
                textPrinterManager.OnPrintTextStarted -= HandlePrintTextStarted;
                textPrinterManager.OnPrintTextFinished -= HandlePrintTextFinished;
            }
        }

        private void HandlePrintTextStarted (PrintTextArgs args)
        {
            if (!SyncAllowed || args.AuthorId != authorId) return;

            setIsSpeaking.Invoke(true);

            var playedVoicePath = audioManager.GetPlayedVoicePath();
            if (!string.IsNullOrEmpty(playedVoicePath))
            {
                var track = audioManager.GetVoiceTrack(playedVoicePath);
                track.OnStop -= HandleVoiceClipStopped;
                track.OnStop += HandleVoiceClipStopped;
            }
            else textPrinterManager.OnPrintTextFinished += HandlePrintTextFinished;
        }

        private void HandlePrintTextFinished (PrintTextArgs args)
        {
            if (args.AuthorId != authorId) return;

            setIsSpeaking.Invoke(false);
            textPrinterManager.OnPrintTextFinished -= HandlePrintTextFinished;
        }

        private void HandleVoiceClipStopped ()
        {
            setIsSpeaking.Invoke(false);
        }
    }
}
