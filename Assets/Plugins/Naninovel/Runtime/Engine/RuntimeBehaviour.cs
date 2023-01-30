// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// A <see cref="IEngineBehaviour"/> implementation using <see cref="MonoBehaviour"/> for runtime environment.
    /// </summary>
    public class RuntimeBehaviour : MonoBehaviour, IEngineBehaviour
    {
        public event Action OnBehaviourUpdate;
        public event Action OnBehaviourLateUpdate;
        public event Action OnBehaviourDestroy;

        private GameObject rootObject;
        private MonoBehaviour monoBehaviour;


        /// <param name="dontDestroyOnLoad">Whether behaviour lifetime should be independent of the loaded Unity scenes.</param>
        public static RuntimeBehaviour Create (bool dontDestroyOnLoad = true)
        {
            var go = new GameObject("Naninovel<Runtime>");
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(go);
            var behaviourComp = go.AddComponent<RuntimeBehaviour>();
            behaviourComp.rootObject = go;
            behaviourComp.monoBehaviour = behaviourComp;
            return behaviourComp;
        }

        public GameObject GetRootObject () => rootObject;

        public void AddChildObject (GameObject obj)
        {
            if (ObjectUtils.IsValid(obj))
                obj.transform.SetParent(transform);
        }

        public void Destroy ()
        {
            if (monoBehaviour && monoBehaviour.gameObject)
                Destroy(monoBehaviour.gameObject);
        }

        private void Update ()
        {
            OnBehaviourUpdate?.Invoke();
        }

        private void LateUpdate ()
        {
            OnBehaviourLateUpdate?.Invoke();
        }

        private void OnDestroy ()
        {
            OnBehaviourDestroy?.Invoke();
        }
    }
}
