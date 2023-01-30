// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Globalization;

namespace Naninovel.Commands
{
    /// <summary>
    /// Animate properties of the actors with the specified IDs via key frames.
    /// Key frames for the animated parameters are delimited with `|` literals.
    /// </summary>
    /// <remarks>
    /// Be aware, that this command searches for actors with the provided IDs over all the actor managers, 
    /// and in case multiple actors with the same ID exist (eg, a character and a text printer), this will affect only the first found one.
    /// <br/><br/>
    /// When running the animate commands in parallel (`wait` is set to false) the affected actors state can mutate unpredictably.
    /// This could cause unexpected results when rolling back or performing other commands that affect state of the actor. Make sure to reset
    /// affected properties of the animated actors (position, tint, appearance, etc) after the command finishes or use `@animate CharacterId` 
    /// (without any args) to stop the animation prematurely.
    /// </remarks>
    [CommandAlias("animate")]
    public class AnimateActor : Command
    {
        /// <summary>
        /// Literal used to delimit adjacent animation key values.
        /// </summary>
        public const char KeyDelimiter = '|';
        /// <summary>
        /// Path to the prefab to spawn with <see cref="ISpawnManager"/>.
        /// </summary>
        public const string prefabPath = "Animate";

        /// <summary>
        /// IDs of the actors to animate.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter]
        public StringListParameter ActorIds;
        /// <summary>
        /// Whether to loop the animation; make sure to set `wait` to false when loop is enabled,
        /// otherwise script playback will loop indefinitely.
        /// </summary>
        [ParameterDefaultValue("false")]
        public BooleanParameter Loop = false;
        /// <summary>
        /// Appearances to set for the animated actors.
        /// </summary>
        public StringParameter Appearance;
        /// <summary>
        /// Type of the [transition effect](/guide/transition-effects.md) to use when animating appearance change (crossfade is used by default).
        /// </summary>
        public StringParameter Transition;
        /// <summary>
        /// Visibility status to set for the animated actors.
        /// </summary>
        public StringParameter Visibility;
        /// <summary>
        /// Position values over X-axis (in 0 to 100 range, in percents from the left border of the scene) to set for the animated actors.
        /// </summary>
        [ParameterAlias("posX")]
        public StringParameter ScenePositionX;
        /// <summary>
        /// Position values over Y-axis (in 0 to 100 range, in percents from the bottom border of the scene) to set for the animated actors.
        /// </summary>
        [ParameterAlias("posY")]
        public StringParameter ScenePositionY;
        /// <summary>
        /// Position values over Z-axis (in world space) to set for the animated actors; while in ortho mode, can only be used for sorting.
        /// </summary>
        [ParameterAlias("posZ")]
        public StringParameter PositionZ;
        /// <summary>
        /// Rotation values (over Z-axis) to set for the animated actors.
        /// </summary>
        public StringParameter Rotation;
        /// <summary>
        /// Scale (`x,y,z` or a single uniform value) to set for the animated actors.
        /// </summary>
        public StringParameter Scale;
        /// <summary>
        /// Tint colors to set for the animated actors.
        /// <br/><br/>
        /// Strings that begin with `#` will be parsed as hexadecimal in the following way: 
        /// `#RGB` (becomes RRGGBB), `#RRGGBB`, `#RGBA` (becomes RRGGBBAA), `#RRGGBBAA`; when alpha is not specified will default to FF.
        /// <br/><br/>
        /// Strings that do not begin with `#` will be parsed as literal colors, with the following supported:
        /// red, cyan, blue, darkblue, lightblue, purple, yellow, lime, fuchsia, white, silver, grey, black, orange, brown, maroon, green, olive, navy, teal, aqua, magenta.
        /// </summary>
        [ParameterAlias("tint")]
        public StringParameter TintColor;
        /// <summary>
        /// Names of the easing functions to use for the animations.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the actor's manager configuration settings.
        /// </summary>
        [ParameterAlias("easing")]
        public StringParameter EasingTypeName;
        /// <summary>
        /// Duration of the animations per key, in seconds.
        /// When a key value is missing, will use one from a previous key.
        /// When not assigned, will use 0.35 seconds duration for all keys.
        /// </summary>
        [ParameterAlias("time")]
        public StringParameter Duration;

        protected virtual ISpawnManager SpawnManager => Engine.GetService<ISpawnManager>();

        private const string defaultDuration = "0.35";

        public static string BuildSpawnPath (string actorId) => $"{prefabPath}{SpawnConfiguration.IdDelimiter}{actorId}";

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var tasks = new List<UniTask>();
            foreach (var actorId in ActorIds)
                tasks.Add(AnimateActorAsync(actorId, asyncToken));
            await UniTask.WhenAll(tasks);
        }

        protected virtual async UniTask AnimateActorAsync (string actorId, AsyncToken asyncToken)
        {
            var spawnPath = BuildSpawnPath(actorId);
            var spawnedObject = await SpawnManager.GetOrSpawnAsync(spawnPath, asyncToken);
            var parameters = GetParametersForActor(actorId);
            spawnedObject.SetSpawnParameters(parameters);
            await spawnedObject.AwaitSpawnAsync(asyncToken);
        }

        protected virtual IReadOnlyList<string> GetParametersForActor (string actorId)
        {
            var parameters = new string[13]; // Don't cache it, otherwise parameters will leak across actors on async spawn init.
            parameters[0] = actorId;
            parameters[1] = Loop.Value.ToString(CultureInfo.InvariantCulture);
            parameters[2] = Assigned(Appearance) ? Appearance : null;
            parameters[3] = Assigned(Transition) ? Transition : null;
            parameters[4] = Assigned(Visibility) ? Visibility : null;
            parameters[5] = Assigned(ScenePositionX) ? ScenePositionX : null;
            parameters[6] = Assigned(ScenePositionY) ? ScenePositionY : null;
            parameters[7] = Assigned(PositionZ) ? PositionZ : null;
            parameters[8] = Assigned(Rotation) ? Rotation : null;
            parameters[9] = Assigned(Scale) ? Scale : null;
            parameters[10] = Assigned(TintColor) ? TintColor : null;
            parameters[11] = Assigned(EasingTypeName) ? EasingTypeName : null;
            parameters[12] = Assigned(Duration) ? Duration.Value : defaultDuration;
            return parameters;
        }
    }
}
