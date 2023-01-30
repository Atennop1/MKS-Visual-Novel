// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    public class TitleExitButton : ScriptableButton
    {
        private const string titleLabel = "OnExit";

        private string titleScriptName;
        private IScriptPlayer scriptPlayer;
        private IScriptManager scriptManager;
        private IStateManager stateManager;

        protected override void Awake ()
        {
            base.Awake();

            scriptManager = Engine.GetService<IScriptManager>();
            titleScriptName = scriptManager.Configuration.TitleScript;
            scriptPlayer = Engine.GetService<IScriptPlayer>();
            stateManager = Engine.GetService<IStateManager>();
        }

        protected override async void OnButtonClick ()
        {
            if (!string.IsNullOrEmpty(titleScriptName) &&
                await scriptManager.LoadScriptAsync(titleScriptName) is Script titleScript &&
                titleScript.LabelExists(titleLabel))
            {
                scriptPlayer.ResetService();
                await scriptPlayer.PreloadAndPlayAsync(titleScript, label: titleLabel);
                await UniTask.WaitWhile(() => scriptPlayer.Playing);
            }

            await stateManager.SaveGlobalAsync();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
                Application.OpenURL("about:blank");
            else Application.Quit();
        }
    }
}
