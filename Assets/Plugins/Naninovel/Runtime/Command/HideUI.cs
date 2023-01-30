// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel.Commands
{
    /// <summary>
    /// Makes [UI elements](/guide/user-interface.md#ui-customization) with the specified names invisible.
    /// When no names are specified, will stop rendering (hide) the entire UI (including all the built-in UIs).
    /// </summary>
    /// <remarks>
    /// When hiding the entire UI with this command and `allowToggle` parameter is false (default), user won't be able to re-show the UI 
    /// back with hotkeys or by clicking anywhere on the screen; use [@showUI] command to make the UI visible again.
    /// </remarks>
    public class HideUI : Command
    {
        /// <summary>
        /// Name of the UI elements to hide.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ResourceContext(UIConfiguration.DefaultPathPrefix)]
        public StringListParameter UINames;
        /// <summary>
        /// When hiding the entire UI, controls whether to allow the user to re-show the UI with hotkeys or by clicking anywhere on the screen (false by default).
        /// Has no effect when hiding a particular UI.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter AllowToggle = false;
        /// <summary>
        /// Duration (in seconds) of the hide animation. 
        /// When not specified, will use UI-specific duration.
        /// </summary>
        [ParameterAlias("time")]
        public DecimalParameter Duration;

        private readonly List<UniTask> changeVisibilityTasks = new List<UniTask>();

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var uiManager = Engine.GetService<IUIManager>();

            if (!Assigned(UINames))
            {
                uiManager.SetUIVisibleWithToggle(false, AllowToggle);
                return;
            }

            changeVisibilityTasks.Clear();
            foreach (var name in UINames)
            {
                var ui = uiManager.GetUI(name);
                if (ui is null)
                {
                    LogWarningWithPosition($"Failed to hide `{name}` UI: managed UI with the specified resource name not found.");
                    continue;
                }

                changeVisibilityTasks.Add(ui.ChangeVisibilityAsync(false, Assigned(Duration) ? Duration : null));
            }

            await UniTask.WhenAll(changeVisibilityTasks);
        }
    }
}
