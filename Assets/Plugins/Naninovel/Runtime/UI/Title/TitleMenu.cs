// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TitleMenu : CustomUI, ITitleUI
    {
        private IScriptPlayer scriptPlayer;
        private string titleScriptName;

        protected override void Awake ()
        {
            base.Awake();

            scriptPlayer = Engine.GetService<IScriptPlayer>();
            titleScriptName = Engine.GetConfiguration<ScriptsConfiguration>().TitleScript;
        }

        public override async UniTask ChangeVisibilityAsync (bool visible, float? duration = null, AsyncToken asyncToken = default)
        {
            if (visible && !string.IsNullOrEmpty(titleScriptName))
            {
                await scriptPlayer.PreloadAndPlayAsync(titleScriptName);
                asyncToken.ThrowIfCanceled();
                await UniTask.WaitWhile(() => scriptPlayer.Playing);
                asyncToken.ThrowIfCanceled();
            }

            await base.ChangeVisibilityAsync(visible, duration, asyncToken);
        }
    }
}
