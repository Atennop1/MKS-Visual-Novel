// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.Commands
{
    /// <summary>
    /// Begins scene transition masking the real scene content with anything that is visible at the moment (except the UI).
    /// When the new scene is ready, finish with [@finishTrans] command.
    /// </summary>
    /// <remarks>
    /// The UI will be hidden and user input blocked while the transition is in progress. 
    /// You can change that by overriding the `ISceneTransitionUI`, which handles the transition process.<br/><br/>
    /// For the list of available transition effect options see [transition effects](/guide/transition-effects.md) guide.
    /// </remarks>
    [CommandAlias("startTrans")]
    public class StartSceneTransition : Command
    {
        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var transitionUI = Engine.GetService<IUIManager>().GetUI<UI.ISceneTransitionUI>();
            if (transitionUI != null) await transitionUI.CaptureSceneAsync();
        }
    }
}
