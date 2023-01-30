// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Slides (moves between two positions) an actor (character, background, text printer or choice handler) with the provided ID and optionally changes actor visibility and appearance.
    /// Can be used instead of multiple [@char] or [@back] commands to reveal or hide an actor with a slide animation.
    /// </summary>
    /// <remarks>
    /// Be aware, that this command searches for an existing actor with the provided ID over all the actor managers, 
    /// and in case multiple actors with the same ID exist (eg, a character and a text printer), this will affect only the first found one.
    /// Make sure the actor exist on scene before referencing it with this command; 
    /// eg, if it's a character, you can add it on scene imperceptibly to player with `@char CharID visible:false time:0`.
    /// </remarks>
    [CommandAlias("slide")]
    public class SlideActor : Command
    {
        /// <summary>
        /// ID of the actor to slide and (optionally) appearance to set.
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), RequiredParameter, ActorContext(namedIndex: 0), AppearanceContext(1)]
        public NamedStringParameter IdAndAppearance;
        /// <summary>
        /// Position in scene space to slide the actor from (slide start position).
        /// Described as follows: `0,0` is the bottom left, `50,50` is the center and `100,100` is the top right corner of the scene; Z-component (depth) is in world space.
        /// When not provided, will use current actor position in case it's visible and a random off-scene position otherwise (could slide-in from left or right borders).
        /// </summary>
        [ParameterAlias("from")]
        public DecimalListParameter FromPosition;
        /// <summary>
        /// Position in scene space to slide the actor to (slide finish position).
        /// </summary>
        [ParameterAlias("to"), RequiredParameter]
        public DecimalListParameter ToPosition;
        /// <summary>
        /// Change visibility status of the actor (show or hide).
        /// When not set and target actor is hidden, will still automatically show it.
        /// </summary>
        public BooleanParameter Visible;
        /// <summary>
        /// Name of the easing function to use for the modifications.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the actor's manager configuration settings.
        /// </summary>
        [ParameterAlias("easing"), ConstantContext(typeof(EasingType))]
        public StringParameter EasingTypeName;
        /// <summary>
        /// Duration (in seconds) of the slide animation.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var actorId = IdAndAppearance.Name;
            var manager = Engine.FindAllServices<IActorManager>(c => c.ActorExists(actorId)).FirstOrDefault();

            if (manager is null)
            {
                LogErrorWithPosition($"Can't find a manager with `{actorId}` actor.");
                return;
            }

            var tasks = new List<UniTask>();

            var cameraConfig = Engine.GetConfiguration<CameraConfiguration>();
            var actor = manager.GetActor(actorId);

            var fromPos = new Vector3(
                FromPosition?.ElementAtOrNull(0)?.HasValue ?? false ? cameraConfig.SceneToWorldSpace(new Vector2(FromPosition[0] / 100f, 0)).x : 
                    actor.Visible ? actor.Position.x : cameraConfig.SceneToWorldSpace(new Vector2(Random.value > .5f ? -.1f : 1.1f, 0)).x,
                FromPosition?.ElementAtOrNull(1)?.HasValue ?? false ? cameraConfig.SceneToWorldSpace(new Vector2(0, FromPosition[1] / 100f)).y : actor.Position.y,
                FromPosition?.ElementAtOrNull(2) ?? actor.Position.z);

            var toPos = new Vector3(
                ToPosition.ElementAtOrNull(0)?.HasValue ?? false ? cameraConfig.SceneToWorldSpace(new Vector2(ToPosition[0] / 100f, 0)).x : actor.Position.x,
                ToPosition.ElementAtOrNull(1)?.HasValue ?? false ? cameraConfig.SceneToWorldSpace(new Vector2(0, ToPosition[1] / 100f)).y : actor.Position.y,
                ToPosition.ElementAtOrNull(2) ?? actor.Position.z);

            var duration = Assigned(Duration) ? Duration.Value : manager.ActorManagerConfiguration.DefaultDuration;
            var easingType = manager.ActorManagerConfiguration.DefaultEasing;
            if (Assigned(EasingTypeName) && !System.Enum.TryParse(EasingTypeName, true, out easingType))
                LogWarningWithPosition($"Failed to parse `{EasingTypeName}` easing.");

            actor.Position = fromPos;

            if (!actor.Visible)
            {
                if (IdAndAppearance.NamedValue.HasValue)
                    actor.Appearance = IdAndAppearance.NamedValue;
                Visible = true;
            }
            else if (IdAndAppearance.NamedValue.HasValue)
                tasks.Add(actor.ChangeAppearanceAsync(IdAndAppearance.NamedValue, duration, easingType, null, asyncToken));

            if (Assigned(Visible)) tasks.Add(actor.ChangeVisibilityAsync(Visible, duration, easingType, asyncToken));

            tasks.Add(actor.ChangePositionAsync(toPos, duration, easingType, asyncToken));

            await UniTask.WhenAll(tasks);
        }
    } 
}
