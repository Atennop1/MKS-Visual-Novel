// Copyright 2017-2021 Elringus (Artyom Sovetnikov). All rights reserved.

using System;
using UnityEngine;
using UnityEngine.Events;

namespace Naninovel
{
    /// <summary>
    /// Retrieves <see cref="OrthoActorMetadata.RenderTexture"/> when the engine is ready
    /// and invokes a Unity event with the retrieved texture.
    /// </summary>
    /// <remarks>
    /// Used as a workaround instead of directly assigning actor render texture to an addressable
    /// prefab due to https://issuetracker.unity3d.com/product/unity/issues/guid/1277169.
    /// </remarks>
    public class GetActorRenderTexture : MonoBehaviour
    {
        [Serializable]
        private class RenderTextureRetrievedEvent : UnityEvent<RenderTexture> { }
        
        private enum ActorType { Character, Background }
        
        [Tooltip("Whether the actor is a character or a background.")]
        [SerializeField] private ActorType actorType = default;
        [Tooltip("ID of the actor from which to get the render texture.")]
        [SerializeField] private string actorId = default;
        [Tooltip("Invoked when render texture is retrieved from the specified actor.")]
        [SerializeField] private RenderTextureRetrievedEvent onRenderTextureRetrieved = default;
        
        private void OnEnable ()
        {
            if (Engine.Initialized) RetrieveTexture();
            else Engine.OnInitializationFinished += RetrieveTexture;
        }
        
        private void OnDisable ()
        {
            Engine.OnInitializationFinished -= RetrieveTexture;
        }

        private void RetrieveTexture ()
        {
            var renderTexture = actorType == ActorType.Character
                ? Engine.GetConfiguration<CharactersConfiguration>().GetMetadataOrDefault(actorId).RenderTexture
                : Engine.GetConfiguration<BackgroundsConfiguration>().GetMetadataOrDefault(actorId).RenderTexture;
            if (renderTexture == null)
            {
                Debug.LogError($"Failed to retrieve `{actorId}` {actorType} actor render texture: either the actor doesn't exist or the render texture is not assigned in the configuration.");
                return;
            }
            
            onRenderTextureRetrieved?.Invoke(renderTexture);
        }
    }
}
