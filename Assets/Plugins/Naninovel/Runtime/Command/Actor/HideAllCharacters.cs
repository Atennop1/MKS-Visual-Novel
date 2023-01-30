// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Linq;

namespace Naninovel.Commands
{
    /// <summary>
    /// Hides (removes) all the visible characters on scene.
    /// </summary>
    [CommandAlias("hideChars")]
    public class HideAllCharacters : Command
    {
        /// <summary>
        /// Duration (in seconds) of the fade animation.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;
        /// <summary>
        /// Whether to remove (destroy) the characters after they are hidden.
        /// Use to unload resources associated with the characters and prevent memory leaks.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Remove = false;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var manager = Engine.GetService<ICharacterManager>();
            var duration = Assigned(Duration) ? Duration.Value : manager.ActorManagerConfiguration.DefaultDuration;
            var easing = manager.ActorManagerConfiguration.DefaultEasing;
            await UniTask.WhenAll(manager.GetAllActors().Select(a => a.ChangeVisibilityAsync(false, duration, easing, asyncToken)));
            if (Remove) manager.RemoveAllActors();
        }
    }
}
