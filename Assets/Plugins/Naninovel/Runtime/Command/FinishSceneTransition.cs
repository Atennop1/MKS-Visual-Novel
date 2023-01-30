// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel.Commands
{
    /// <summary>
    /// Finishes scene transition started with [@startTrans] command;
    /// see the start command reference for more information and usage examples.
    /// </summary>
    [CommandAlias("finishTrans")]
    public class FinishSceneTransition : Command, Command.IPreloadable
    {
        /// <summary>
        /// Type of the [transition effect](/guide/transition-effects.md) to use (crossfade is used by default).
        /// </summary>
        [ParameterAlias(NamelessParameterAlias), ConstantContext(typeof(TransitionType))]
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
        /// Name of the easing function to use for the modification.
        /// <br/><br/>
        /// Available options: Linear, SmoothStep, Spring, EaseInQuad, EaseOutQuad, EaseInOutQuad, EaseInCubic, EaseOutCubic, EaseInOutCubic, EaseInQuart, EaseOutQuart, EaseInOutQuart, EaseInQuint, EaseOutQuint, EaseInOutQuint, EaseInSine, EaseOutSine, EaseInOutSine, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCirc, EaseOutCirc, EaseInOutCirc, EaseInBounce, EaseOutBounce, EaseInOutBounce, EaseInBack, EaseOutBack, EaseInOutBack, EaseInElastic, EaseOutElastic, EaseInOutElastic.
        /// <br/><br/>
        /// When not specified, will use a default easing function set in the actor's manager configuration settings.
        /// </summary>
        [ParameterAlias("easing"), ConstantContext(typeof(EasingType))]
        public StringParameter EasingTypeName;
        /// <summary>
        /// Duration (in seconds) of the transition.
        /// </summary>
        [ParameterAlias("time"), ParameterDefaultValue("0.35")]
        public DecimalParameter Duration = .35f;

        private Texture2D preloadedDissolveTexture;

        public virtual async UniTask PreloadResourcesAsync ()
        {
            if (Assigned(DissolveTexturePath) && !DissolveTexturePath.DynamicValue)
            {
                var loader = Resources.LoadAsync<Texture2D>(DissolveTexturePath);
                await loader;
                preloadedDissolveTexture = loader.asset as Texture2D;
            }
        }

        public virtual void ReleasePreloadedResources ()
        {
            preloadedDissolveTexture = null;
        }

        public override async UniTask ExecuteAsync (AsyncToken asyncToken = default)
        {
            var easingType = EasingType.Linear;
            if (Assigned(EasingTypeName) && !Enum.TryParse(EasingTypeName, true, out easingType))
                LogWarningWithPosition($"Failed to parse `{EasingTypeName}` easing.");

            var transitionName = TransitionUtils.DefaultTransition;
            if (Assigned(Transition))
                transitionName = Transition.Value;
            var defaultParams = TransitionUtils.GetDefaultParams(transitionName);
            var transitionParams = Assigned(TransitionParams) ? new Vector4(
                    TransitionParams.ElementAtOrNull(0) ?? defaultParams.x,
                    TransitionParams.ElementAtOrNull(1) ?? defaultParams.y,
                    TransitionParams.ElementAtOrNull(2) ?? defaultParams.z,
                    TransitionParams.ElementAtOrNull(3) ?? defaultParams.w) : defaultParams;

            if (Assigned(DissolveTexturePath) && !ObjectUtils.IsValid(preloadedDissolveTexture))
                preloadedDissolveTexture = Resources.Load<Texture2D>(DissolveTexturePath);

            var transition = new Transition(transitionName, transitionParams, preloadedDissolveTexture);
            var transitionUI = Engine.GetService<IUIManager>().GetUI<UI.ISceneTransitionUI>();
            if (transitionUI != null)
                await transitionUI.TransitionAsync(transition, Duration, easingType, asyncToken);
            else LogErrorWithPosition($"Failed to finish scene transition: `{nameof(UI.ISceneTransitionUI)}` UI is not available.");
        }
    }
}
