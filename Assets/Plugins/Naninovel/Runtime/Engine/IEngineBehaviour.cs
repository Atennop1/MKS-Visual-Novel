// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using System.Collections;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Implementation is able to represent Unity's <see cref="MonoBehaviour"/> proxy.
    /// </summary>
    public interface IEngineBehaviour
    {
        /// <summary>
        /// Event invoked on each render loop update phase.
        /// </summary>
        event Action OnBehaviourUpdate;
        /// <summary>
        /// Event invoked on each render loop late update phase.
        /// </summary>
        event Action OnBehaviourLateUpdate;
        /// <summary>
        /// Event invoked when the behaviour is destroyed.
        /// </summary>
        event Action OnBehaviourDestroy;

        /// <summary>
        /// Returns root game object of the behaviour.
        /// </summary>
        GameObject GetRootObject ();
        /// <summary>
        /// Adds provided game object to the behaviour's root object.
        /// </summary>
        void AddChildObject (GameObject obj);
        /// <summary>
        /// Destroys the behaviour.
        /// </summary>
        void Destroy ();
        /// <summary>
        /// Starts provided coroutine over behaviour.
        /// </summary>
        Coroutine StartCoroutine (IEnumerator routine);
        /// <summary>
        /// Stops a coroutine started with <see cref="StartCoroutine(IEnumerator)"/>.
        /// </summary>
        void StopCoroutine (Coroutine routine);
    }
}
