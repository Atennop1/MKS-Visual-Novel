// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;

namespace Naninovel.Commands
{
    /// <summary>
    /// Makes [UI elements](/guide/user-interface.md) with the specified resource names visible.
    /// When no names are specified, will reveal the entire UI (in case it was hidden with [@hideUI]).
    /// </summary>
    public class ShowUI : Command
    {
        /// <summary>
        /// Name of the UI resource to make visible.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ResourceContext(UIConfiguration.DefaultPathPrefix)]
        public StringListParameter UINames;
        /// <summary>
        /// Duration (in seconds) of the show animation. 
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
                uiManager.SetUIVisibleWithToggle(true);
                return;
            }

            changeVisibilityTasks.Clear();
            foreach (var name in UINames)
            {
                var ui = uiManager.GetUI(name);
                if (ui is null)
                {
                    LogWarningWithPosition($"Failed to show `{name}` UI: managed UI with the specified resource name not found.");
                    continue;
                }
                changeVisibilityTasks.Add(ui.ChangeVisibilityAsync(true, Assigned(Duration) ? Duration : null));
            }

            await UniTask.WhenAll(changeVisibilityTasks);
        }
    }
}
