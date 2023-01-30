// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using UnityEngine;

namespace Naninovel
{
    /// <summary>
    /// Represents serializable data required to construct and initialize a <see cref="IActor"/>.
    /// </summary>
    [System.Serializable]
    public abstract class ActorMetadata
    {
        /// <summary>
        /// Globally-unique identifier of the medata instance.
        /// </summary>
        public string Guid => guid;

        [Tooltip("Assembly-qualified type name of the actor implementation.")]
        public string Implementation = default;
        [Tooltip("Data describing how to load actor's resources.")]
        public ResourceLoaderConfiguration Loader = default;

        [HideInInspector]
        [SerializeField] private string guid = System.Guid.NewGuid().ToString();
        [SerializeReference] private CustomMetadata customData = default;

        /// <summary>
        /// Attempts to retrieve an actor pose associated with the provided name;
        /// returns null when not found.
        /// </summary>
        public abstract ActorPose<TState> GetPoseOrNull<TState> (string poseName) where TState : ActorState;

        /// <summary>
        /// Attempts to retrieve a custom data of type <typeparamref name="TData"/>.
        /// </summary>
        /// <typeparam name="TData">Type of the custom data to retrieve.</typeparam>
        public virtual TData GetCustomData<TData> () where TData : CustomMetadata
        {
            return customData as TData;
        }

        /// <summary>
        /// Returns ID of the resource category associated with the metadata.
        /// </summary>
        public string GetResourceCategoryId () => $"{Loader.PathPrefix}/{Guid}";
    }
}
