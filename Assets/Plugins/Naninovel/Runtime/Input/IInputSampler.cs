// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to sample player input.
    /// </summary>
    public interface IInputSampler
    {
        /// <summary>
        /// Invoked when input activation started.
        /// </summary>
        event Action OnStart;
        /// <summary>
        /// Invoked when input activation ended.
        /// </summary>
        event Action OnEnd;

        /// <summary>
        /// Assigned input binding.
        /// </summary>
        InputBinding Binding { get; }
        /// <summary>
        /// Whether input should be sampled; can be used to temporary "mute" specific inputs.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Whether input is being activated.
        /// </summary>
        bool Active { get; }
        /// <summary>
        /// Current value (activation force) of the input; zero means the input is not active.
        /// </summary>
        float Value { get; }
        /// <summary>
        /// Whether input started activation during current frame.
        /// </summary>
        bool StartedDuringFrame { get; }
        /// <summary>
        /// Whether input ended activation during current frame.
        /// </summary>
        bool EndedDuringFrame { get; }

        /// <summary>
        /// Activates the input.
        /// </summary>
        /// <param name="value">Value (force) of the activation, in 0.0 to 1.0 range.</param>
        void Activate (float value);
        /// <summary>
        /// When any of the provided game objects are clicked or touched, input event will trigger.
        /// </summary>
        void AddObjectTrigger (GameObject obj);
        /// <summary>
        /// Removes object added with <see cref="AddObjectTrigger"/>.
        /// </summary>
        void RemoveObjectTrigger (GameObject obj);
        /// <summary>
        /// Waits until input starts or ends activation.
        /// </summary>
        /// <returns>Whether input started or ended activation.</returns>
        UniTask<bool> WaitForInputAsync ();
        /// <summary>
        /// Waits until input starts activation.
        /// </summary>
        UniTask WaitForInputStartAsync ();
        /// <summary>
        /// Waits until input ends activation.
        /// </summary>
        UniTask WaitForInputEndAsync ();
        /// <summary>
        /// Returned token will be canceled on next input start activation.
        /// </summary>
        System.Threading.CancellationToken GetInputStartCancellationToken ();
        /// <summary>
        /// Returned token will be canceled on next input end activation.
        /// </summary>
        System.Threading.CancellationToken GetInputEndCancellationToken ();
    }
}
