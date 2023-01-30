// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel.UI
{
    public class TitleNewGameButton : ScriptableButton
    {
        private const string titleLabel = "OnNewGame";

        [Tooltip("Services to exclude from state reset when starting a new game.")]
        [SerializeField] private string[] excludeFromReset = Array.Empty<string>();

        private string startScriptName;
        private string titleScriptName;
        private TitleMenu titleMenu;
        private IScriptPlayer scriptPlayer;
        private IStateManager stateManager;
        private IScriptManager scriptManager;

        protected override void Awake ()
        {
            base.Awake();

            scriptManager = Engine.GetService<IScriptManager>();
            startScriptName = scriptManager.StartGameScriptName;
            titleScriptName = scriptManager.Configuration.TitleScript;
            titleMenu = GetComponentInParent<TitleMenu>();
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            stateManager = Engine.GetService<IStateManager>();
            Debug.Assert(titleMenu && scriptPlayer != null);
        }

        protected override void Start ()
        {
            base.Start();

            if (string.IsNullOrEmpty(startScriptName))
                UIComponent.interactable = false;
        }

        protected override async void OnButtonClick ()
        {
            if (string.IsNullOrEmpty(startScriptName))
            {
                Debug.LogError("Can't start new game: specify start script name in the settings.");
                return;
            }

            if (!string.IsNullOrEmpty(titleScriptName) &&
                await scriptManager.LoadScriptAsync(titleScriptName) is Script titleScript &&
                titleScript.LabelExists(titleLabel))
            {
                scriptPlayer.ResetService();
                await scriptPlayer.PreloadAndPlayAsync(titleScript, label: titleLabel);
                await UniTask.WaitWhile(() => scriptPlayer.Playing);
            }

            titleMenu.Hide();
            stateManager.ResetStateAsync(excludeFromReset,
                () => scriptPlayer.PreloadAndPlayAsync(startScriptName)).Forget();
        }
    }
}
