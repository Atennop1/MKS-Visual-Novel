// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.


namespace Naninovel.UI
{
    /// <summary>
    /// Handles scene transition (<see cref="Commands.StartSceneTransition"/> and <see cref="Commands.FinishSceneTransition"/> commands).
    /// </summary>
    public interface ISceneTransitionUI : IManagedUI 
    {
        /// <summary>
        /// Saves the current main camera content to a temporary render texture to use during the transition.
        /// </summary>
        UniTask CaptureSceneAsync ();
        /// <summary>
        /// Performs transition between the previously captured scene texture and current main camera content.
        /// </summary>
        UniTask TransitionAsync (Transition transition, float duration, EasingType easingType = default, AsyncToken asyncToken = default);
    }
}
