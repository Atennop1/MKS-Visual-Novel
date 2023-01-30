// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel.Commands
{
    /// <summary>
    /// Hides (removes) all the actors (characters, backgrounds, text printers, choice handlers) on scene.
    /// </summary>
    [CommandAlias("hideAll")]
    public class HideAllActors : Command
    {
        /// <summary>
        /// Duration (in seconds) of the fade animation.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;
        /// <summary>
        /// Whether to remove (destroy) the actors after they are hidden.
        /// Use to unload resources associated with the actors and prevent memory leaks.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Remove = false;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var managers = Engine.FindAllServices<IActorManager>();
            await UniTask.WhenAll(managers.Select(m => HideManagedActorsAsync(m, asyncToken)));
            if (Remove)
                foreach (var manager in managers)
                    manager.RemoveAllActors();
        }

        private UniTask HideManagedActorsAsync (IActorManager manager, AsyncToken asyncToken)
        {
            var duration = Assigned(Duration) ? Duration.Value : manager.ActorManagerConfiguration.DefaultDuration;
            var easing = manager.ActorManagerConfiguration.DefaultEasing;
            return UniTask.WhenAll(manager.GetAllActors().Select(a => a.ChangeVisibilityAsync(false, duration, easing, asyncToken)));
        }
    }
}
