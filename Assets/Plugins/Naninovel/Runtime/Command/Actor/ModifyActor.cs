// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Linq;
using UnityEngine;

namespace Naninovel.Commands
{
    public abstract class ModifyActor<TActor, TState, TMeta, TConfig, TManager> : Command, Command.IPreloadable
        where TActor : class, IActor
        where TState : ActorState<TActor>, new()
        where TMeta : ActorMetadata
        where TConfig : ActorManagerConfiguration<TMeta>, new()
        where TManager : class, IActorManager<TActor, TState, TMeta, TConfig>
    {
        /// <summary>
        /// ID of the actor to modify; specify `*` to affect all visible actors.
        /// </summary>
        public StringParameter Id;
        /// <summary>
        /// Appearance to set for the modified actor.
        /// </summary>
        [AppearanceContext]
        public StringParameter Appearance;
        /// <summary>
        /// Pose to set for the modified actor.
        /// </summary>
        public StringParameter Pose;
        /// <summary>
        /// Type of the [transition effect](/guide/transition-effects.md) to use (crossfade is used by default).
        /// </summary>
        [ConstantContext(typeof(TransitionType))]
        public StringParameter Transition;
        /// <summary>
        /// Parameters of the transition effect.
        /// </summary>
        [ParameterAlias("params")]
        public DecimalListParameter TransitionParams;
        /// <summary>
        /// Path to the [custom dissolve](/guide/transition-effects.md#custom-transition-effects) texture (path should be relative to a `Resources` folder).
        /// Has effect only when the transition is set to `Custom` mode.
        /// </summary>
        [ParameterAlias("dissolve")]
        public StringParameter DissolveTexturePath;
        /// <summary>
        /// Visibility status to set for the modified actor.
        /// </summary>
        public BooleanParameter Visible;
        /// <summary>
        /// Position (in world space) to set for the modified actor. 
        /// Use Z-component (third member) to move (sort) by depth while in ortho mode.
        /// </summary>
        public DecimalListParameter Position;
        /// <summary>
        /// Rotation to set for the modified actor.
        /// </summary>
        public DecimalListParameter Rotation;
        /// <summary>
        /// Scale to set for the modified actor.
        /// </summary>
        public DecimalListParameter Scale;
        /// <summary>
        /// Tint color to set for the modified actor.
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
        /// Name of the easing function to use for the modification.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the actor's manager configuration settings.
        /// </summary>
        [ParameterAlias("easing"), ConstantContext(typeof(EasingType))]
        public StringParameter EasingTypeName;
        /// <summary>
        /// Duration (in seconds) of the modification.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration;

        protected virtual string AssignedId => Id;
        protected virtual string AssignedTransition => Transition;
        protected virtual string AssignedAppearance => Assigned(Appearance) ? Appearance.Value : PosedAppearance ?? (PoseAssigned ? null : AlternativeAppearance);
        protected virtual bool? AssignedVisibility => Assigned(Visible) ? Visible.Value : PosedVisibility ?? ActorManager.Configuration.AutoShowOnModify ? (bool?)true : null;
        protected virtual float?[] AssignedPosition => Assigned(Position) ? Position : PosedPosition;
        protected virtual float?[] AssignedRotation => Assigned(Rotation) ? Rotation : PosedRotation;
        protected virtual float?[] AssignedScale => Assigned(Scale) ? Scale : PosedScale;
        protected virtual Color? AssignedTintColor => Assigned(TintColor) ? ParseColor(TintColor) : PosedTintColor;
        protected virtual float AssignedDuration => Assigned(Duration) ? Duration.Value : ActorManager.ActorManagerConfiguration.DefaultDuration;
        protected virtual TManager ActorManager => Engine.GetService<TManager>();
        protected virtual string AlternativeAppearance => null;
        protected virtual bool AllowPreload => Assigned(Id) && !Id.DynamicValue && Assigned(Appearance) && !Appearance.DynamicValue;
        protected virtual bool PoseAssigned => GetPoseOrNull() != null;

        protected string PosedAppearance => GetPosed(nameof(ActorState.Appearance))?.Appearance;
        protected bool? PosedVisibility => GetPosed(nameof(ActorState.Visible))?.Visible;
        protected float?[] PosedPosition => GetPosed(nameof(ActorState.Position))?.Position.ToNullableArray();
        protected float?[] PosedRotation => GetPosed(nameof(ActorState.Rotation))?.Rotation.eulerAngles.ToNullableArray();
        protected float?[] PosedScale => GetPosed(nameof(ActorState.Scale))?.Scale.ToNullableArray();
        protected Color? PosedTintColor => GetPosed(nameof(ActorState.TintColor))?.TintColor;

        private Texture2D preloadedDissolveTexture;

        public virtual async UniTask PreloadResourcesAsync ()
        {
            if (Assigned(DissolveTexturePath) && !DissolveTexturePath.DynamicValue)
            {
                var loader = Resources.LoadAsync<Texture2D>(DissolveTexturePath);
                await loader;
                preloadedDissolveTexture = loader.asset as Texture2D;
            }

            if (!AllowPreload || string.IsNullOrEmpty(AssignedId)) return;
            var actor = await ActorManager.GetOrAddActorAsync(AssignedId);
            await actor.HoldResourcesAsync(AssignedAppearance, this);
        }

        public virtual void ReleasePreloadedResources ()
        {
            preloadedDissolveTexture = null;

            if (!AllowPreload || ActorManager is null || string.IsNullOrEmpty(AssignedId)) return;
            if (ActorManager.ActorExists(AssignedId))
                ActorManager.GetActor(AssignedId).ReleaseResources(AssignedAppearance, this);
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            if (ActorManager is null)
            {
                LogErrorWithPosition("Can't resolve actors manager.");
                return;
            }

            if (string.IsNullOrEmpty(AssignedId))
            {
                LogErrorWithPosition("Actor ID was not provided.");
                return;
            }

            var easingType = ActorManager.Configuration.DefaultEasing;
            if (Assigned(EasingTypeName) && !Enum.TryParse(EasingTypeName, true, out easingType))
                LogWarningWithPosition($"Failed to parse `{EasingTypeName}` easing.");

            if (AssignedId == "*")
            {
                var actors = ActorManager.GetAllActors().Where(a => a.Visible);
                await UniTask.WhenAll(actors.Select(a => ApplyModificationsAsync(a, easingType, asyncToken)));
            }
            else
            {
                var actor = await ActorManager.GetOrAddActorAsync(AssignedId);
                asyncToken.ThrowIfCanceled();
                await ApplyModificationsAsync(actor, easingType, asyncToken);
            }
        }

        protected virtual async UniTask ApplyModificationsAsync (TActor actor, EasingType easingType, AsyncToken asyncToken)
        {
            // In case the actor is hidden, apply all the modifications (except visibility) without animation.
            var durationOrZero = actor.Visible ? AssignedDuration : 0;
            await UniTask.WhenAll(
                // Change appearance with normal duration when a transition is assigned to preserve the effect.
                ApplyAppearanceModificationAsync(actor, easingType, string.IsNullOrEmpty(AssignedTransition) ? durationOrZero : AssignedDuration, asyncToken),
                ApplyPositionModificationAsync(actor, easingType, durationOrZero, asyncToken),
                ApplyRotationModificationAsync(actor, easingType, durationOrZero, asyncToken),
                ApplyScaleModificationAsync(actor, easingType, durationOrZero, asyncToken),
                ApplyTintColorModificationAsync(actor, easingType, durationOrZero, asyncToken),
                ApplyVisibilityModificationAsync(actor, easingType, AssignedDuration, asyncToken)
            );
        }

        protected virtual async UniTask ApplyAppearanceModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            if (string.IsNullOrEmpty(AssignedAppearance)) return;

            var transitionName = !string.IsNullOrEmpty(AssignedTransition) ? AssignedTransition : TransitionUtils.DefaultTransition;
            var defaultParams = TransitionUtils.GetDefaultParams(transitionName);
            var transitionParams = Assigned(TransitionParams)
                ? new Vector4(
                    TransitionParams.ElementAtOrNull(0) ?? defaultParams.x,
                    TransitionParams.ElementAtOrNull(1) ?? defaultParams.y,
                    TransitionParams.ElementAtOrNull(2) ?? defaultParams.z,
                    TransitionParams.ElementAtOrNull(3) ?? defaultParams.w)
                : defaultParams;
            if (Assigned(DissolveTexturePath) && !ObjectUtils.IsValid(preloadedDissolveTexture))
                preloadedDissolveTexture = Resources.Load<Texture2D>(DissolveTexturePath);
            var transition = new Transition(transitionName, transitionParams, preloadedDissolveTexture);

            await actor.ChangeAppearanceAsync(AssignedAppearance, duration, easingType, transition, asyncToken);
        }

        protected virtual async UniTask ApplyVisibilityModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            if (!AssignedVisibility.HasValue) return;
            await actor.ChangeVisibilityAsync(AssignedVisibility.Value, duration, easingType, asyncToken);
        }

        protected virtual async UniTask ApplyPositionModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            var position = AssignedPosition;
            if (position is null) return;
            await actor.ChangePositionAsync(new Vector3(
                position.ElementAtOrDefault(0) ?? actor.Position.x,
                position.ElementAtOrDefault(1) ?? actor.Position.y,
                position.ElementAtOrDefault(2) ?? actor.Position.z), duration, easingType, asyncToken);
        }

        protected virtual async UniTask ApplyRotationModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            var rotation = AssignedRotation;
            if (rotation is null) return;
            await actor.ChangeRotationAsync(Quaternion.Euler(
                rotation.ElementAtOrDefault(0) ?? actor.Rotation.eulerAngles.x,
                rotation.ElementAtOrDefault(1) ?? actor.Rotation.eulerAngles.y,
                rotation.ElementAtOrDefault(2) ?? actor.Rotation.eulerAngles.z), duration, easingType, asyncToken);
        }

        protected virtual async UniTask ApplyScaleModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            var scale = AssignedScale;
            if (scale is null) return;
            await actor.ChangeScaleAsync(new Vector3(
                scale.ElementAtOrDefault(0) ?? actor.Scale.x,
                scale.ElementAtOrDefault(1) ?? actor.Scale.y,
                scale.ElementAtOrDefault(2) ?? actor.Scale.z), duration, easingType, asyncToken);
        }

        protected virtual async UniTask ApplyTintColorModificationAsync (TActor actor, EasingType easingType, float duration, AsyncToken asyncToken)
        {
            if (!AssignedTintColor.HasValue) return;
            await actor.ChangeTintColorAsync(AssignedTintColor.Value, duration, easingType, asyncToken);
        }

        protected virtual Color? ParseColor (string color)
        {
            if (string.IsNullOrEmpty(color)) return null;

            if (!ColorUtility.TryParseHtmlString(TintColor, out var result))
            {
                LogErrorWithPosition($"Failed to parse `{TintColor}` color to apply tint modification. See the API docs for supported color formats.");
                return null;
            }
            return result;
        }

        protected virtual ActorPose<TState> GetPoseOrNull ()
        {
            var poseName = Assigned(Pose) ? Pose.Value : AlternativeAppearance;
            if (string.IsNullOrEmpty(poseName)) return null;
            return ActorManager.Configuration.GetMetadataOrDefault(AssignedId).GetPoseOrNull<TState>(poseName);
        }

        protected virtual TState GetPosed (string propertyName)
        {
            var pose = GetPoseOrNull();
            return pose != null && pose.IsPropertyOverridden(propertyName) ? pose.ActorState : null;
        }
    }
}
