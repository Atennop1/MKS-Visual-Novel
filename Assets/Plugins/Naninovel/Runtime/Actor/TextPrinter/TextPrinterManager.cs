// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <inheritdoc cref="ITextPrinterManager"/>
    [InitializeAtRuntime]
    public class TextPrinterManager : OrthoActorManager<ITextPrinterActor, TextPrinterState, TextPrinterMetadata, TextPrintersConfiguration>, IStatefulService<SettingsStateMap>, ITextPrinterManager
    {
        [Serializable]
        public class Settings
        {
            public float BaseRevealSpeed = .5f;
            public float BaseAutoDelay = .5f;
        }

        [Serializable]
        public new class GameState
        {
            public string DefaultPrinterId = null;
        }

        public event Action<PrintTextArgs> OnPrintTextStarted;
        public event Action<PrintTextArgs> OnPrintTextFinished;

        public virtual string DefaultPrinterId { get; set; }
        public virtual float BaseRevealSpeed { get; set; }
        public virtual float BaseAutoDelay { get; set; }

        private readonly IScriptPlayer scriptPlayer;

        public TextPrinterManager (TextPrintersConfiguration config, CameraConfiguration cameraConfig, IScriptPlayer scriptPlayer)
            : base(config, cameraConfig)
        {
            this.scriptPlayer = scriptPlayer;
        }

        public override async UniTask InitializeServiceAsync ()
        {
            await base.InitializeServiceAsync();
            
            DefaultPrinterId = Configuration.DefaultPrinterId;
        }

        public override void ResetService ()
        {
            base.ResetService();
            DefaultPrinterId = Configuration.DefaultPrinterId;
        }

        public virtual void SaveServiceState (SettingsStateMap stateMap)
        {
            var settings = new Settings {
                BaseRevealSpeed = BaseRevealSpeed,
                BaseAutoDelay = BaseAutoDelay
            };
            stateMap.SetState(settings);
        }

        public virtual UniTask LoadServiceStateAsync (SettingsStateMap stateMap)
        {
            var settings = stateMap.GetState<Settings>();
            
            if (settings is null) // Apply default settings.
            {
                BaseRevealSpeed = Configuration.DefaultBaseRevealSpeed;
                BaseAutoDelay = Configuration.DefaultBaseAutoDelay;
                return UniTask.CompletedTask;
            }
            
            BaseRevealSpeed = settings.BaseRevealSpeed;
            BaseAutoDelay = settings.BaseAutoDelay;
            return UniTask.CompletedTask;
        }

        public override void SaveServiceState (GameStateMap stateMap)
        {
            base.SaveServiceState(stateMap);

            var gameState = new GameState {
                DefaultPrinterId = DefaultPrinterId ?? Configuration.DefaultPrinterId
            };
            stateMap.SetState(gameState);
        }

        public override async UniTask LoadServiceStateAsync (GameStateMap stateMap)
        {
            await base.LoadServiceStateAsync(stateMap);

            var state = stateMap.GetState<GameState>() ?? new GameState();
            DefaultPrinterId = state.DefaultPrinterId ?? Configuration.DefaultPrinterId;
        }

        public virtual async UniTask PrintTextAsync (string printerId, string text, string authorId = default, float speed = 1, AsyncToken asyncToken = default)
        {
            var printer = await GetOrAddActorAsync(printerId);
            asyncToken.ThrowIfCanceled();

            OnPrintTextStarted?.Invoke(new PrintTextArgs(printer, text, authorId, speed));

            printer.AuthorId = authorId;
            printer.Text += text;

            var revealDelay = scriptPlayer.SkipActive ? 0 : Mathf.Lerp(Configuration.MaxRevealDelay, 0, BaseRevealSpeed * speed);
            await printer.RevealTextAsync(revealDelay, asyncToken);

            OnPrintTextFinished?.Invoke(new PrintTextArgs(printer, text, authorId, speed));
        }
    }
}
